using Shared.Helpers;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amigo.Controllers
{
    public sealed class CGame6MaxHumansOnly: CGameController
    {
        /// <summary>
        /// Indice (correspondant à FTabJoueurs) qui représente le premier joueur à parler postflop pour le prochain tour (tour étant, soit preflop, flop turn ou river)
        /// </summary>
        //private int FFIndPremierJoueurAParlerPostflopProchaineMain;

        /// <summary>
        /// Interface reliée à la partie.
        /// </summary>
        private frmJeu FFrmJeu;

        /// <summary>
        /// Use this constructor if you want to use a interface.
        /// </summary>
        /// <param name="_lstPlayers">List of players in the game. The objects will be used by the simulator.</param>
        /// <param name="_nbJetonsDepart">Nombre de jetons pour chaque joueur au départ de la partie.</param>
        /// <param name="_smallBlind">Small bind pour chaque main dans la partie.</param>
        /// <param name="_bigBlind">Big blind pour chaque main dans la partie.</param>
        /// <param name="_antes">Antes pour chaque main de la partie.</param>
        /// <param name="_useInterface">If you want to show a interface to the user.</param>
        public CGame6MaxHumansOnly(List<CPlayer> _lstPlayers, double _smallBlind, double _bigBlind, double _antes, bool _resetStackEveryHand): base(_lstPlayers, _smallBlind, _bigBlind, _antes, _resetStackEveryHand)
        {
        }

        public override void PlayNewHand()
        {
            throw new NotImplementedException();
        }

        public override void StopGame()
        {
            throw new NotImplementedException();
        }

        protected override void GameStateChanged()
        {
            throw new NotImplementedException();
        }

        protected override void GameStopped()
        {
            throw new NotImplementedException();
        }

        protected override void Play()
        {
            throw new NotImplementedException();
        }

        protected override void ReceivedAction(CAction _action)
        {
            throw new NotImplementedException();
        }

        private void SelectFirstPlayerToActPostflop()
        {
            if (FFLstJoueursPasFold.Count() <= 0)
                throw new InvalidOperationException("Il n'y a aucun joueur dans la liste des joueurs qui joue actuellement!");

            PIndPremierJoueurAParlerPostflop = (int)CListHelper.PremierElemQuiEstPlusGrandOuEgal<int>(FFLstJoueursPasFold.Cast<IComparable<int>>().ToList(), PIndPremierJoueurAParlerPostflop);
        }
        private void SelectLastPlayerToActPostflop()
        {
            if (!FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

            FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndPremierJoueurAParlerPostflop);
        }
    }
}
