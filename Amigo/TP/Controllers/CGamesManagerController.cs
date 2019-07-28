using Amigo.Bots;
using Amigo.Helpers;
using HandHistories.Objects.GameDescription;
using HandHistories.Objects.Hand;
using HandHistories.Objects.Players;
using HandHistories.Parser.Parsers.Base;
using HandHistories.Parser.Parsers.Factory;
using HoldemHand;
using Shared.Helpers;
using Shared.Models.Database;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using Shared.Poker.Helpers;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CTableInfos;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CAction;
using Amigo.Views;

namespace Amigo.Controllers
{
    public class CGamesManagerController
    {
        /// <summary>
        /// Pointeur qui pointe vers une liste de CPartie qui représente toutes les parties.
        /// </summary>
        private List<CGameController> FFLstParties;

        private frmCreerPartie FFView;

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dicRange">Range to update the probabilities</param>
        /// <param name="_cardMask">The new card on the board (this function works for turn or river boards only)</param>
        /// <returns></returns>
        
        public CGamesManagerController(frmCreerPartie _view)
        /*
            Objectif: Constructeur de la classe. Afficher l'interface principale du programme.
        */
        {
            FFView = _view;
            // Création d'une liste des parties.
            FFLstParties = new List<CGameController>();

            //Test();
        }

        /*public static void Test()
        {
            var lst = new List<CPlayer>()
            {
                new CPlayer(100, "Amigo"),
                new CPlayer(100, "Ante-Amigo")
            };
            var gfdgfd = new HandHistory();
            var qwe = new CGame2MaxManualController(lst, 0.5m, 1, 0, false, gfdgfd);
            qwe.PlayNewHand();

            CalculateEV(qwe);
            var sda = 2;
        }

        public static double CalculateEV(CGame2MaxManualController _gameState)
        {
            var fdsfsd = CBotPokerAmigo.GetPossibleFutureStatesFromCurrentState(_gameState);
            var value = -123123123m;

            foreach (var item in fdsfsd)
            {
                var tempVal = -23232323m;
                var lstNewStates = CBotPokerAmigo.GetPossibleFutureStatesFromCurrentState(_gameState);
                foreach (var state in lstNewStates)
                {
                    tempVal = Math.Max(CalculateEV(state), tempVal);
                }
                value = Math.Max(tempVal, value);
            }

            return value;
        }*/

        public void Close()
        {
            FFLstParties.Clear();
            FFLstParties = null;
            Application.Exit();
        }

        public Task StartGameAsync(List<CPlayer> _lstPlayers, double _smallBlind, double _bigBlind, double _antes)
        {
            throw new NotImplementedException();
            return Task.Run(() =>
            {
                CGameController newGame = new CGame2MaxBotsOnlyController((_lstPlayers[0], new CBotPokerAmigo()), (_lstPlayers[1], new CBotPokerAmigo()), _smallBlind, _bigBlind, _antes, true);
                //newGame.PEventGameStopped += GameStopped;

                FFLstParties.Add(newGame);
                // Execute the method on another thread, since this method is infinite and it is NOT our job to wait for it on "this" thread.
                newGame.PlayNewHandAsync();
            });
        }

        public void StartReplayerAsync(string _handHistory)
        {
            //return Task.Run(() =>
            //{
                IHandHistoryParserFactory factory = new HandHistoryParserFactoryImpl();
                IHandHistoryParser handParser = factory.GetFullHandHistoryParser(SiteName.PokerStars);

                //try
                //{
                    HandHistory handHistory = handParser.ParseFullHandHistory(_handHistory, true);

                    var handReplayer = new CHandReplayerController();
                    var view = new frmReplayer(handReplayer);

                    CApplication.CreateNewView(view);
                    CApplication.ShowNewView(view);

                    handReplayer.ParseNewHand(handHistory);
               // }
                //catch (Exception)
                //{
                    //MessageBox.Show("Impossible de lire la main donnée!", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
          //  });
        }

        public Task SimulateGamesAsync(List<CPlayer> _lstBots, double _smallBlind, double _bigBlind, double _antes, bool _showInterface)
        {
            return Task.Run(() =>
            {
                //CGame2MaxHumanBotController newGame = new CGame2MaxHumanBotController(new CPlayer(100.0d, "Jonathan89"), (new CPlayer(100.0d, "Amigo"), new CBotPokerAmigo()), _smallBlind, _bigBlind, _antes, true);
                CGame2MaxBotsOnlyController newGame = new CGame2MaxBotsOnlyController((_lstBots[0], new CBotPokerAmigo()), (_lstBots[1], new CBotPokerAmigo()), _smallBlind, _bigBlind, _antes, true);

                if (_showInterface)
                {
                    var view = new frmJeu(newGame);
                    CApplication.CreateNewView(view);
                    CApplication.ShowNewView(view);
                }

          //  CDBHelperHandInfos.LoadBoardInfos(Hand.ParseHand("Ah2c2d"), Hand.ParseHand("JhJc"), 0);
                FFLstParties.Add(newGame);
                newGame.PlayNewHandAsync();
                

             /*   System.Timers.Timer timer = new System.Timers.Timer(3000);
                timer.Elapsed += Timer_Tick;
                timer.Start();*/

      //          foreach (CGameController game in FFLstParties)
        //            game.PlayNewHandAsync();
            });
        }

        public Task StopSimulatingAsync()
        {
            return Task.Run(() =>
            {
                foreach (CGameController game in FFLstParties)
                {
                    game.StopGame();
                    FFLstParties.Remove(game);
                }
            });
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (CGameController game in FFLstParties)
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
