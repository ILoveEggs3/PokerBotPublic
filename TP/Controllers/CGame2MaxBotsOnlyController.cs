using System;
using System.Collections.Generic;
using System.Linq;

using HoldemHand;
using static Amigo.Models.CAction;
using static Amigo.Core.CTableInfos;
using System.Threading;
using Amigo.Helpers;
using System.IO;
using System.Windows.Forms;
using static Amigo.Models.CPlayer;
using System.Threading.Tasks;
using Amigo.Models;
using Amigo.Core;
using Amigo.Bots;

namespace Amigo.Controllers
{
    public class CGame2MaxBotsOnlyController: CGameController
    {
        private static object lockObject = new object();
        #region Données membres, accesseurs, constantes et énumérés

        /// <summary>
        /// Interface reliée à la partie.
        /// </summary>
        private frmJeu FFrmJeu;

        private List<CBotPoker> FFLstBots;

        #endregion

        /// <summary>
        /// Use this constructor if you want to use a interface.
        /// </summary>
        /// <param name="_dctPlayers">List of players in the game. The objects will be used by the simulator.</param>
        /// <param name="_nbJetonsDepart">Nombre de jetons pour chaque joueur au départ de la partie.</param>
        /// <param name="_smallBlind">Small bind pour chaque main dans la partie.</param>
        /// <param name="_bigBlind">Big blind pour chaque main dans la partie.</param>
        /// <param name="_antes">Antes pour chaque main de la partie.</param>
        /// <param name="_useInterface">If you want to show a interface to the user.</param>
        public CGame2MaxBotsOnlyController(Dictionary<CPlayer, CBotPoker> _dctPlayers, decimal _nbJetonsDepart, decimal _smallBlind, decimal _bigBlind, decimal _antes, bool _resetStackEveryHand, bool _useInterface): base(_dctPlayers.Keys.ToList(), _nbJetonsDepart, _smallBlind, _bigBlind, _antes, _resetStackEveryHand)
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
                FFrmJeu.AfficherJoueurs(1, _dctPlayers.Count);
            };
            #endregion

            FFLstBots = _dctPlayers.Values.ToList();

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

        #region Common methods
        /// <summary>
        /// Play a whole new hand
        /// </summary>
        public override void PlayNewHand()
        {
            base.PlayNewHand();

            if (PContinuePlaying)
            {
                int indPremierJoueurAParlerPreflop = 0;

                #region Local methods
                Action SelectionnerPremierJoueurAJouerPostflop = () =>
                {
                    // S'il n'y a aucun joueur à parler postflop de sélectionné
                    if (FFIndPremierJoueurAParlerPostflop == -1)
                        FFIndPremierJoueurAParlerPostflop = FFLstJoueursPasFold.Last(); // Sélectionne le BB automatiquement                    
                    else                    
                        FFIndPremierJoueurAParlerPostflop = FFIndPremierJoueurAParlerPostflopProchaineMain;

                    FFIndPremierJoueurAParlerPostflopProchaineMain = FFLstJoueursPasFold.ElemNextOf(FFIndPremierJoueurAParlerPostflop);                    
                };
                Action SelectionnerPremierJoueurAJouerPreflop = () =>
                {
                    if (!FFLstJoueursPasFold.Contains(FFIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    indPremierJoueurAParlerPreflop = CListHelper.ElemNextOf(FFLstJoueursPasFold, FFIndPremierJoueurAParlerPostflop);
                };
                Action SelectionnerDernierJoueurAJouerSelonFLstJoueursPasFold = () =>
                {
                    if (!FFLstJoueursPasFold.Contains(FFIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    FFIndDernierJoueurAParler = FFIndPremierJoueurAParlerPostflop;
                };
                Action AjouterSmallBlind = () =>
                {
                    if (!FFLstJoueursPasFold.Contains(indPremierJoueurAParlerPreflop) || !FFLstJoueursPasFold.Contains(FFIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop et indPremierJoueurAParlerPreflop");

                    // Si le joueur a assez de jetons pour mettre le small blind
                    if (FFTabJoueurs[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft >= PSmallBlind)
                    {
                        FFTabJoueurs[indPremierJoueurAParlerPreflop].PLastBet = PSmallBlind;
                        FFTabJoueurs[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft = (FFTabJoueurs[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft - PSmallBlind);
                    }
                    else
                    {
                        FFTabJoueurs[indPremierJoueurAParlerPreflop].PLastBet = FFTabJoueurs[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft;
                        FFTabJoueurs[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft = 0;
                    }
                    
                };
                Action AjouterBigBlind = () =>
                {
                    if (!FFLstJoueursPasFold.Contains(FFIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    // Si le joueur a assez de jetons pour mettre un big blind
                    if (FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft >= PBigBlind)
                    {
                        FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PLastBet = PBigBlind;
                        FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = (FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft - PBigBlind);
                    }
                    else
                    {
                        FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PLastBet = FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft;
                        FFTabJoueurs[FFIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = 0;
                    }
                    
                };
                Action LFGUIPositionnerDealer = () =>
                {
                    if (FFrmJeu == null)
                        throw new ArgumentNullException("FFrmJeu");

                    // Wierd fix. But if player 1 = first person to speak next hand, that means he's the DEALER.
                    // If player 2 = first person to speak next hand, that means he's the DEALER.
                    // This work for every possible scenario for a heads up game.
                    switch (FFIndPremierJoueurAParlerPostflopProchaineMain)
                    {
                        case CCSB:
                            FFrmJeu.PositionnerDealer(frmJeu.DEALER_POS.SB);
                            break;
                        case CCBB:
                            FFrmJeu.PositionnerDealer(frmJeu.DEALER_POS.BB);
                            break;
                    }
                };
                #endregion

                CLogger.CreateLogContainer(Path.GetDirectoryName(Application.ExecutablePath));
                CLogger.AddLog(new CLog("---- NEW HAND ----"));
                ++PHandCount;

                // L'ordre est important pour les procédures suivantes
                MakeEveryoneAliveAndResetBets();
                SelectionnerPremierJoueurAJouerPostflop.Invoke();
                SelectionnerPremierJoueurAJouerPreflop.Invoke();
                SelectionnerDernierJoueurAJouerSelonFLstJoueursPasFold.Invoke();
                if (FFrmJeu != null)
                    LFGUIPositionnerDealer();
                // Fin de l'ordre important

                AjouterSmallBlind.Invoke();
                AjouterBigBlind.Invoke();

                PPot += (PSmallBlind + PBigBlind);
                PDerniereMise = PBigBlind;
                PIndJoueurActuel = indPremierJoueurAParlerPreflop;

                if (FFrmJeu != null)
                {
                    #region Mise à jour de l'interface graphique
                    #region Local methods
                    Action ShowPlayersCards = (() =>
                    {
                        // Show cards, otherwise they won't be shown in some specific scenarios (for instance, when the bot plays first and he decides to fold)
                        for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)                        
                            FFrmJeu.AfficherCartes(indJoueur, FFTabJoueursCartes[indJoueur]);                        
                    });
                    #endregion
                    FFrmJeu.ResetActions(true);

                    FFrmJeu.AfficherSmallBlind(indPremierJoueurAParlerPreflop);
                    FFrmJeu.AfficherBigBlind(FFIndPremierJoueurAParlerPostflop);

                    FFrmJeu.AfficherNbJetonDunJoueur(indPremierJoueurAParlerPreflop);
                    FFrmJeu.AfficherNbJetonDunJoueur(FFIndPremierJoueurAParlerPostflop);

                    ShowPlayersCards();
                    FFrmJeu.Refresh();
                    #endregion
                }

                Play();                
            }
            else            
                GameStopped();            
        }

        /// <summary>
        /// Jouer une main (celle-ci se fait rappeler à chaque tour de joueur) (se répète tant et aussi longtemps que Jouer()) se fait appelé
        /// </summary>
        protected override sealed void Play()
        {
            #region Local methods
            Action MakeDecision = () =>
            {
                CTableInfosNLHE2Max tableInfos = GetTableInformationsFromCurrentPlayerPointOfView();
                CGame2MaxManualController controller = new CGame2MaxManualController(this);
                CAction decision = FFLstBots[PIndJoueurActuel].GetDecision(tableInfos, controller);

                switch (decision.PAction)
                {
                    case ActionsPossible.Fold:
                        Fold();
                        break;
                    case ActionsPossible.Call:
                        Call();
                        break;
                    case ActionsPossible.Check:
                        Check();
                        break;
                    case ActionsPossible.Bet:
                        Bet(decision.PMise);
                        break;
                    case ActionsPossible.Raise:
                        Raise(decision.PMise);
                        break;
                }
            };
            #endregion
            if (PContinuePlaying)
            {
                UpdateCurrentPlayerAllowedActions();                    
                MakeDecision.Invoke();                            
            }
            else
                GameStopped();
        }

        /// <summary>
        /// Event that is called when the simulator stopped the current game.
        /// </summary>
        protected override sealed void GameStopped()
        {
            // Close form on UI thread
            FFrmJeu?.Invoke(new Action(() => { FFrmJeu?.Close(); }));
            FFrmJeu = null;

            base.GameStopped();
        }

        /// <summary>
        /// Event that is called everytime a player made an action.
        /// </summary>
        /// <param name="_action">Action that the player did</param>
        private void ReceivedAction(CAction _action)
        {
            #region Local methods
            Action LFSaveActionInHistory = () =>
            {
                // Ajoute à la liste des actions du tour en cours l'action du joueur
                CPlayer currentPlayer = FFTabJoueurs[PIndJoueurActuel];

                FFLstActionsMainActuelParJoueur[currentPlayer][(int)PStadeMain].Add(_action);
                FFLstActionsMainActuel.Add(new Tuple<CAction, int>(_action, PIndJoueurActuel));
            };
            Action LFFold = () =>
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(ActionsPossible.Fold))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Fold!");
                else if (!FFLstJoueursPasFold.Contains(PIndJoueurActuel))
                    throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndJoueurActuel");

                FFTabJoueursCartes[PIndJoueurActuel] = "";
                FFLstJoueursPasFold.Remove(PIndJoueurActuel);
            };
            Action LFCheck = () =>
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(ActionsPossible.Check))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Check!");
            };
            Action LFCall = () =>
            {
                #region Local methods
                Func<bool> LFPlayerIsAllIn = new Func<bool>(() =>
                {
                    return (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < (PDerniereMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));
                });
                #endregion
                if (!FLstActionsPossibleJoueurActuel.Contains(ActionsPossible.Call))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Call!");

                // ORDER IS IMPORTANT HERE
                decimal ancienneDerniereMise = FFTabJoueurs[PIndJoueurActuel].PLastBet;
                bool playerIsAllIn = LFPlayerIsAllIn.Invoke();

                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                // Si le joueur est ALL IN
                if (playerIsAllIn)                
                    FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = 0;                
                else                
                    FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - (PDerniereMise - ancienneDerniereMise));                

                PPot = PPot + (FFTabJoueurs[PIndJoueurActuel].PLastBet - ancienneDerniereMise);
            };
            Action LFBet = () =>
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(ActionsPossible.Bet) || (PDerniereMise > 0))
                    throw new InvalidOperationException("Il y a déjà une mise en cours. On ne peut pas miser!");
                else if (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < _action.PMise)
                    throw new InvalidOperationException("Le joueur n'a pas assez de jetons pour effectuer une telle mise.");

                PPot = PPot + _action.PMise;
                PDerniereMise = _action.PMise;
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - _action.PMise);
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);
            };
            Action LFRaise = () =>
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(ActionsPossible.Raise))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Raise!");
                else if ((FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft + FFTabJoueurs[PIndJoueurActuel].PLastBet) < _action.PMise)
                    throw new InvalidOperationException("Le joueur doit avoir le nombre de jetons nécessaire pour effectuer une relance.");
                else if (_action.PMise < (PDerniereMise + PBigBlind))
                    throw new InvalidOperationException("La mise doit être au moins un big blind de plus que la dernière mise.");


                PPot = PPot + (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet);
                PDerniereMise = _action.PMise;
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);
            };
            #endregion

            LFSaveActionInHistory.Invoke();

            #region Execute the action
            switch (_action.PAction)
            {
                case ActionsPossible.Fold:
                    LFFold.Invoke();
                    break;
                case ActionsPossible.Check:
                    LFCheck.Invoke();
                    break;
                case ActionsPossible.Call:
                    LFCall.Invoke();
                    break;
                case ActionsPossible.Bet:
                    LFBet.Invoke();
                    break;
                case ActionsPossible.Raise:
                    LFRaise.Invoke();
                    break;
            }
            #endregion

            if (FFrmJeu != null)
                GUIMettreAJour(_action);
            GameStateChanged();
        }

        /// <summary>
        /// Event that is called everytime that the something changed in the game (example: A player checked.)
        /// </summary>
        private void GameStateChanged()
        {
            #region Local methods
            Func<bool> LFOnePlayerIsAllIn = new Func<bool>(() =>
            {
                return (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft == 0 || FFTabJoueurs[CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel)].PNumberOfChipsLeft == 0 && FFTabJoueurs[PIndJoueurActuel].PLastBet == FFTabJoueurs[CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel)].PLastBet);
            });
            Action LFChangerStadeTour = () =>
            {
                ++PStadeMain;
                PDerniereMise = 0;

                switch (PStadeMain)
                {
                    case ToursPossible.Flop:
                        string carte1 = GetRandomCard().ToString();
                        string carte2 = GetRandomCard().ToString();
                        string carte3 = GetRandomCard().ToString();

                        FFBoard = carte1 + " " + carte2 + " " + carte3;
                        FFrmJeu?.AfficherFlop(carte1, carte2, carte3);
                        break;
                    case ToursPossible.Turn:
                        string carte4 = GetRandomCard().ToString();

                        FFBoard += " " + carte4;
                        FFrmJeu?.AfficherTurn(carte4);
                        break;
                    case ToursPossible.River:
                        string carte5 = GetRandomCard().ToString();

                        FFBoard += " " + carte5;
                        FFrmJeu?.AfficherRiver(carte5);
                        break;
                    default:
                        throw new InvalidOperationException("Stade de tour (flop, turn ou river) invalide.");
                }
            };
            Action LFMainFini = () =>
            {
                int winnerPlayerIndex = 0;
                bool splitPot = false; // Workaround for HU.

                #region Méthodes
                Action Showdown = () =>
                {
                    for (int i = 0; i < FFLstJoueursPasFold.Count; i++)
                    {
                        int ind = FFLstJoueursPasFold[i];
                        FFrmJeu?.AfficherCartes(ind, FFTabJoueursCartes[ind]);
                    }

                    Hand bestHand = new Hand(FFTabJoueursCartes[FFLstJoueursPasFold[0]], FFBoard);
                    winnerPlayerIndex = 0;

                    for (int i = 1; i < FFLstJoueursPasFold.Count; i++)
                    {
                        Hand currentPlayerHand = new Hand(FFTabJoueursCartes[FFLstJoueursPasFold[i]], FFBoard);
                        if (currentPlayerHand >= bestHand)
                        {
                            if (currentPlayerHand == bestHand)
                                splitPot = true;

                            winnerPlayerIndex = i;
                            bestHand = currentPlayerHand;
                        }
                    }
                };
                #endregion

                if (FFLstJoueursPasFold.Count == 1)
                    winnerPlayerIndex = FFLstJoueursPasFold.First();
                else
                    Showdown();

                if (splitPot)
                {
                    #region Reinitialize stacks
                    int firstPlayerIndex = winnerPlayerIndex;
                    int secondPlayerIndex = CListHelper.ElemNextOf(FFLstJoueursPasFold, winnerPlayerIndex);

                    CPlayer firstPlayer = FFTabJoueurs[firstPlayerIndex];
                    CPlayer secondPlayer = FFTabJoueurs[secondPlayerIndex];

                    firstPlayer.PNumberOfChipsLeft = FFStackDepartDebutMain[firstPlayerIndex];
                    secondPlayer.PNumberOfChipsLeft = FFStackDepartDebutMain[secondPlayerIndex];
                    #endregion

                    #region Update interface
                    if (FFrmJeu != null)
                    {
                        FFrmJeu.AfficherGagnerPotDunJoueur(winnerPlayerIndex, (float)firstPlayer.PNumberOfChipsLeft);
                        FFrmJeu.AfficherGagnerPotDunJoueur(CListHelper.ElemNextOf(FFLstJoueursPasFold, winnerPlayerIndex), (float)secondPlayer.PNumberOfChipsLeft);
                    }
                    #endregion
                }
                else
                {
                    int losingPlayerIndex = -1;

                    if (winnerPlayerIndex == 0)
                        losingPlayerIndex = 1;
                    else
                        losingPlayerIndex = 0;

                    CPlayer winningPlayer = FFTabJoueurs[winnerPlayerIndex];
                    CPlayer losingPlayer = FFTabJoueurs[losingPlayerIndex];

                    winningPlayer.PSessionInfo.PNbWins += 1;

                    if (FFStackDepartDebutMain[winnerPlayerIndex] >= FFStackDepartDebutMain[losingPlayerIndex])
                    {
                        winningPlayer.PNumberOfChipsLeft = winningPlayer.PNumberOfChipsLeft + PPot;
                        winningPlayer.PSessionInfo.PNbProfit += (FFStackDepartDebutMain[losingPlayerIndex] - losingPlayer.PNumberOfChipsLeft);
                        losingPlayer.PSessionInfo.PNbProfit -= (FFStackDepartDebutMain[losingPlayerIndex] - losingPlayer.PNumberOfChipsLeft);
                    }
                    // Is the winning player all in?
                    else
                    {
                        if (winningPlayer.PNumberOfChipsLeft == 0)
                        {
                            if (FFLstJoueursPasFold.Count > 1)
                            {
                                winningPlayer.PNumberOfChipsLeft = (FFStackDepartDebutMain[winnerPlayerIndex] * 2);
                                winningPlayer.PSessionInfo.PNbProfit += decimal.Divide((FFStackDepartDebutMain[winnerPlayerIndex] * 2), 2);

                                losingPlayer.PNumberOfChipsLeft = losingPlayer.PNumberOfChipsLeft + (PPot - winningPlayer.PNumberOfChipsLeft);
                                losingPlayer.PSessionInfo.PNbProfit -= decimal.Divide((FFStackDepartDebutMain[winnerPlayerIndex] * 2), 2);
                            }
                            else
                            {
                                winningPlayer.PNumberOfChipsLeft = winningPlayer.PNumberOfChipsLeft + PPot;
                                winningPlayer.PSessionInfo.PNbProfit += (FFStackDepartDebutMain[losingPlayerIndex] - losingPlayer.PNumberOfChipsLeft);
                                losingPlayer.PSessionInfo.PNbProfit -= (FFStackDepartDebutMain[losingPlayerIndex] - losingPlayer.PNumberOfChipsLeft);
                            }
                        }
                        else
                        {
                            winningPlayer.PNumberOfChipsLeft = winningPlayer.PNumberOfChipsLeft + PPot;
                            winningPlayer.PSessionInfo.PNbProfit += decimal.Divide(PPot, 2);
                            losingPlayer.PSessionInfo.PNbProfit -= decimal.Divide(PPot, 2);
                        }
                    }

                    FFrmJeu?.AfficherGagnerPotDunJoueur(winnerPlayerIndex, (float)PPot);
                }

                #region Clear current hand history of players
                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                {
                    FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)ToursPossible.Preflop].Clear();
                    FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)ToursPossible.Flop].Clear();
                    FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)ToursPossible.Turn].Clear();
                    FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)ToursPossible.River].Clear();
                }

                FFLstActionsMainActuel.Clear();
                #endregion

                PPot = 0;

                #region Update interface
                if (FFrmJeu != null)
                {
                    FFrmJeu.Refresh();
                    Thread.Sleep(2000);
                }
                #endregion
            };
            #endregion

            // If everyone folded except one person
            if (FFLstJoueursPasFold.Count == 1)
            {
                LFMainFini.Invoke();
                PlayNewHandAsync();
            }
            // If we are at the end of the street (preflop, flop, turn ou river).
            else if (base.LastPlayerPlayed())
            {
                FFrmJeu?.ResetActions(false);                                                   

                if (PStadeMain == ToursPossible.River)
                {
                    LFMainFini.Invoke();
                    PlayNewHandAsync();
                }
                else
                {
                    if (LFOnePlayerIsAllIn.Invoke())
                    {
                        #region One player is all in and we can be on preflop, flop or the turn.
                        while (PStadeMain != ToursPossible.River)
                        {
                            LFChangerStadeTour.Invoke();

                            if (FFrmJeu != null)
                                Thread.Sleep(1500);
                        }

                        LFMainFini.Invoke();
                        PlayNewHandAsync();
                        #endregion
                    }
                    else
                    {
                        #region Change the current street to another street (example: from preflop to flop)
                        LFChangerStadeTour.Invoke();                        

                        PIndJoueurActuel = FFIndPremierJoueurAParlerPostflop;
                        FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);

                        FFTabJoueurs[0].PLastBet = 0;
                        FFTabJoueurs[1].PLastBet = 0;

                        PlayAsync();
                        #endregion
                    }
                }
            }
            else
            {
                PIndJoueurActuel = GetNextPlayerIndex();
                PlayAsync();
            }
        }
        #endregion
        #region Methods that are related to the players
        private void UpdateCurrentPlayerAllowedActions()
        {
            if (FFLstJoueursPasFold.IndexOf(PIndJoueurActuel) == -1)
                throw new InvalidOperationException("L'indice de joueur actuel ne correspond à aucun joueur qui joue en cours!");

            CPlayer joueurTourActuel = FFTabJoueurs[PIndJoueurActuel];
            CAction derniereAction = null;

            // On prend la dernière action que le joueur actuel a effectué dans le tour actuel (s'il y en a une)
            if (FFLstActionsMainActuelParJoueur[joueurTourActuel][(int)PStadeMain].Count > 0)
                derniereAction = FFLstActionsMainActuelParJoueur[joueurTourActuel][(int)PStadeMain].Last();
            else
                derniereAction = new CAction(ActionsPossible.Aucune);

            FLstActionsPossibleJoueurActuel.Clear();
            FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Fold);

            if (PDerniereMise == 0)
            {
                FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Check);

                if (joueurTourActuel.PNumberOfChipsLeft >= PBigBlind)
                    FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Bet);
            }
            else if (derniereAction.PMise <= PDerniereMise)
            {
                // Si le joueur a déjà mis une mise (situation qui arrivera probablement jamais) ou est big blind (seule situation qui va arriver probablement)
                if (derniereAction.PMise == PDerniereMise || FFTabJoueurs[PIndJoueurActuel].PLastBet == PDerniereMise)
                    FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Check);
                else                
                    FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Call);                

                if (joueurTourActuel.PNumberOfChipsLeft >= (PDerniereMise + PBigBlind))
                {
                    CPlayer playerThatIsAllIn = null;

                    foreach (CPlayer player in FFTabJoueurs)
                        if (player.PNumberOfChipsLeft == 0)
                        {
                            playerThatIsAllIn = player;
                            break;
                        }

                    if (playerThatIsAllIn == null)
                        FLstActionsPossibleJoueurActuel.Add(ActionsPossible.Raise);
                }

            }
        }
        private CTableInfosNLHE2Max GetTableInformationsFromCurrentPlayerPointOfView()
        {
            CCard leftCard = new CCard((CCard.Value)FFTabJoueursCartes[PIndJoueurActuel][0], (CCard.Type)FFTabJoueursCartes[PIndJoueurActuel][1]);
            CCard rightCard = new CCard((CCard.Value)FFTabJoueursCartes[PIndJoueurActuel][3], (CCard.Type)FFTabJoueursCartes[PIndJoueurActuel][4]);

            PossiblePositions heroPosition = PossiblePositions.Unknown;
            PossiblePositions villainPosition = PossiblePositions.Unknown;

            // If hero is first to act, that means he's the BB
            if (FFIndPremierJoueurAParlerPostflop == PIndJoueurActuel)
            {
                heroPosition = PossiblePositions.BB;
                villainPosition = PossiblePositions.BTN;
            }
            else
            {
                heroPosition = PossiblePositions.BTN;
                villainPosition = PossiblePositions.BB;
            }

            CPlayer heroPlayer = FFTabJoueurs[PIndJoueurActuel];
            CPlayer villainPlayer = FFTabJoueurs[CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel)];

            heroPlayer.PPosition = heroPosition;
            villainPlayer.PPosition = villainPosition;

            CTableInfosNLHE2Max tableInfos = new CTableInfosNLHE2Max(PSmallBlind,
                                                                     PBigBlind,
                                                                     PAntes,
                                                                     PPot,
                                                                     heroPlayer,
                                                                     new Tuple<CCard, CCard>(leftCard, rightCard),
                                                                     FFBoard,
                                                                     villainPlayer);
            
            // Ajoute tous les actions qui se sont passé dans la main actuelle
            for (int indAction = 0; indAction < FFLstActionsMainActuel.Count; ++indAction) {                
                CAction currentAction = FFLstActionsMainActuel[indAction].Item1;
                bool isActionDoneByHero = (FFLstActionsMainActuel[indAction].Item2 == PIndJoueurActuel);

                tableInfos.AjouterAction(currentAction, isActionDoneByHero);
            }                

            return tableInfos;
        }
        #endregion    
        #region Méthodes pour les interfaces
        private void GUIMettreAJour(CAction _lastAction)
        {
            if (FFrmJeu == null)
                throw new ArgumentNullException();

            FFrmJeu.MettreAJourChoixDuJoueur(false, false, false, false, false, 0);
            FFrmJeu.AfficherPot();

            switch (_lastAction.PAction)
            {
                case ActionsPossible.Fold:
                    FFrmJeu.AfficherFoldDunJoueur(PIndJoueurActuel);
                    break;
                case ActionsPossible.Check:
                    FFrmJeu.AfficherCheckDunJoueur(PIndJoueurActuel);
                    break;
                case ActionsPossible.Call:
                    FFrmJeu.AfficherCallDunJoueur(PIndJoueurActuel);
                    FFrmJeu.AfficherNbJetonDunJoueur(PIndJoueurActuel);
                    break;
                case ActionsPossible.Bet:
                    FFrmJeu.AfficherMiseDunJoueur(PIndJoueurActuel);
                    FFrmJeu.AfficherNbJetonDunJoueur(PIndJoueurActuel);
                    break;
                case ActionsPossible.Raise:
                    FFrmJeu.AfficherRaiseDunJoueur(PIndJoueurActuel);
                    FFrmJeu.AfficherNbJetonDunJoueur(PIndJoueurActuel);
                    break;
            }

            FFrmJeu.Refresh();

            if (_lastAction.PAction == ActionsPossible.Fold)
                Thread.Sleep(1500);
            else            
                Thread.Sleep(500);
        }
        #endregion
        #region Méthodes qui se fait appeler seulement par une interface
        /// <summary>
        /// Effectuer l'action de fold pour le joueur à qui est le tour de jouer.
        /// </summary>
        public override void Fold()
        {
            ReceivedAction(new CAction(ActionsPossible.Fold));
        }
        /// <summary>
        /// Effectuer l'action de check pour le joueur à qui est le tour de jouer.
        /// </summary>
        public override void Check()
        {
            ReceivedAction(new CAction(ActionsPossible.Check));
        }
        /// <summary>
        /// Effectuer l'action de call pour le joueur à qui est le tour de jouer.
        /// </summary>
        public override void Call()
        {
            #region Local methods
            Func<bool> LFPlayerIsAllIn = new Func<bool>(() =>
            {
                return (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < (PDerniereMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));                
            });
            #endregion

            decimal derniereMise = 0;

            // Si le joueur est ALL IN
            if (LFPlayerIsAllIn.Invoke())            
                derniereMise = (FFTabJoueurs[PIndJoueurActuel].PLastBet + FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft);            
            else
                derniereMise = PDerniereMise;                           

            ReceivedAction(new CAction(ActionsPossible.Call, derniereMise));
        }
        /// <summary>
        /// Effectuer l'action de miser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>
        public override void Bet(decimal _mise)
        {
            ReceivedAction(new CAction(ActionsPossible.Bet, _mise));
        }
        /// <summary>
        /// Effectuer l'action de raiser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>
        public override void Raise(decimal _mise)
        {
            ReceivedAction(new CAction(ActionsPossible.Raise, _mise));
        }
        public CGame2MaxManualController ToCGame2MaxManualController()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
