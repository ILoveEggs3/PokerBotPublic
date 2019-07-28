using Amigo.Helpers;
using Amigo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public CGame6MaxHumansOnly(List<CPlayer> _lstPlayers, decimal _nbJetonsDepart, decimal _smallBlind, decimal _bigBlind, decimal _antes, bool _resetStackEveryHand, bool _useInterface): base(_lstPlayers, _nbJetonsDepart, _smallBlind, _bigBlind, _antes, _resetStackEveryHand)
        {
            #region Local methods            
            Func<frmJeu> LFCreateFrmJeu = new Func<frmJeu>(() => { return new frmJeu(); });

            Action LFAfficherJoueurs = () =>
            {
                for (int indexPlayer = 0; indexPlayer < FFTabJoueurs.Length; ++indexPlayer)
                {
                    CPlayer player = FFTabJoueurs[indexPlayer];

                    if (player.PName != null)
                        FFrmJeu.ChangerNomJoueur(player.PName, indexPlayer + 1);
                }
                FFrmJeu.AfficherJoueurs(1, _lstPlayers.Count);
            };
            #endregion

            if (_useInterface)
            {
                // Create the new form on UI thread
                CApplication.ExecuteOnMainThread(delegate { FFrmJeu = LFCreateFrmJeu(); }, null);
                // Show the new form on UI thread
                CApplication.ExecuteOnMainThread(delegate { FFrmJeu.Show(); }, null);
                FFrmJeu.PController = this;

                LFAfficherJoueurs();
            }
        }

        private void SelectFirstPlayerToActPostflop()
        {
            if (FFLstJoueursPasFold.Count() <= 0)
                throw new InvalidOperationException("Il n'y a aucun joueur dans la liste des joueurs qui joue actuellement!");

            FFIndPremierJoueurAParlerPostflop = (int)CListHelper.PremierElemQuiEstPlusGrandOuEgal<int>(FFLstJoueursPasFold.Cast<IComparable<int>>().ToList(), FFIndPremierJoueurAParlerPostflop);
        }
        private void SelectLastPlayerToActPostflop()
        {
            if (!FFLstJoueursPasFold.Contains(FFIndPremierJoueurAParlerPostflop))
                throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

            FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, FFIndPremierJoueurAParlerPostflop);
        }

        public override void Bet(decimal _mise)
        {
            throw new NotImplementedException();
        }

        public override void Call()
        {
            throw new NotImplementedException();
        }

        public override void Check()
        {
            throw new NotImplementedException();
        }

        public override void Fold()
        {
            throw new NotImplementedException();
        }

        public override void PlayNewHand()
        {
            throw new NotImplementedException();
        }

        public override void Raise(decimal _mise)
        {
            throw new NotImplementedException();
        }

        public override void StopGame()
        {
            throw new NotImplementedException();
        }

        protected override void Play()
        {
            throw new NotImplementedException();
        }
    }
}
