using Amigo.Bots;
using Amigo.Controllers;
using Amigo.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amigo.Controllers {

    
    public class CGamesManagerController {
        /// <summary>
        /// Pointeur qui pointe vers une liste de CPartie qui représente toutes les parties.
        /// </summary>
        private List<CGameController> FFLstParties;
        private List<CPlayer> FFBotPoker;

        private frmCreerPartie FFView;

        public CGamesManagerController(frmCreerPartie _view)
        /*
            Objectif: Constructeur de la classe. Afficher l'interface principale du programme.
        */
        {
            FFView = _view;
            // Création d'une liste des parties.
            FFLstParties = new List<CGameController>();            
        }

        public void Close()
        {
            FFLstParties.Clear();
            FFLstParties = null;
            Application.Exit();
        }

        public Task StartGameAsync(List<CPlayer> _lstPlayers, decimal _nbJetonsDepart, decimal _smallBlind, decimal _bigBlind, decimal _antes)
        {
            return Task.Run(() =>
            {
                // Create the new controller on current thread
                Dictionary<CPlayer, CBotPoker> dicBots = new Dictionary<CPlayer, CBotPoker>(10);

                foreach (CPlayer player in _lstPlayers)
                    dicBots.Add(player, new CBotPokerAmigo());

                CGameController newGame = new CGame2MaxBotsOnlyController(dicBots, _nbJetonsDepart, _smallBlind, _bigBlind, _antes, false, true);
                newGame.PEventGameStopped += GameStopped;

                FFLstParties.Add(newGame);
                // Execute the method on another thread, since this method is infinite and it is NOT our job to wait for it on "this" thread.
                newGame.PlayNewHandAsync();
            });
        }

        public Task SimulateGamesAsync(List<CPlayer> _lstBots, decimal _nbJetonsDepart, decimal _smallBlind, decimal _bigBlind, decimal _antes)
        {
            return Task.Run(() =>
            {
                // Create the new controller on current thread
                Dictionary<CPlayer, CBotPoker> dicBots = new Dictionary<CPlayer, CBotPoker>(10);
                
                dicBots.Add(_lstBots[0], new CBotPokerAmigo());
                dicBots.Add(_lstBots[1], new CBotPokerIFoldEverything());

                // Execute the method on another thread, since this method is infinite and it is NOT our job to wait for it on "this" thread.
                for (int indGame = 0; indGame < 1; ++indGame)
                {
                    CGameController newGame = new CGame2MaxBotsOnlyController(dicBots, _nbJetonsDepart, _smallBlind, _bigBlind, _antes, false, false);

                    FFLstParties.Add(newGame);                    
                }
                
                System.Timers.Timer timer = new System.Timers.Timer(3000);
                timer.Elapsed += Timer_Tick;
                timer.Start();

                foreach(CGameController game in FFLstParties)
                    game.PlayNewHandAsync();
            });
        }

        public Task StopSimulatingAsync()
        {
            return Task.Run(() =>
            {
                foreach (CGameController game in FFLstParties)
                {
                    game.StopGameAsync();
                    FFLstParties.Remove(game);
                }                
            });
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach(CGameController game in FFLstParties)
            {
                Dictionary<string, CSessionInfo> dicProfitsPlayers = game.GetSessionInfosForEveryPlayer();
                FFView.UpdateSimulationInfos(dicProfitsPlayers, game.PHandCount);
            }            
        }

        public void GameStopped(CGameController _game)
        {
            if (_game == null)
                throw new ArgumentNullException("_game");

            bool foundGame = false;
            int currentGameIndex = 0;

            while (!foundGame && currentGameIndex < FFLstParties.Count)
                foundGame = (FFLstParties[currentGameIndex++] == _game);

            if (foundGame)
                FFLstParties.RemoveAt(--currentGameIndex);
        }

        public void SimulatedGameStopped(CGameController _game)
        {            
            //TODO: Montrer le bouton pour démarrer
        }
    }
}
