using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Poker.Models;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CAction;
using Shared.Helpers;
using static Shared.Poker.Models.CTableInfos;
using HoldemHand;
using Amigo.Events;

namespace Amigo.Controllers
{
    /// <summary>
    /// Class useful for the Amigo bot
    /// </summary>
    public class CGame2MaxManualController: CGameController
    {
        private bool FFHandFinished;
        public AState PCurrentGameState;

        /// <summary>
        /// Controller used for the hand replayer
        /// </summary>
        /// <param name="_smallBlind">Small bind pour chaque main dans la partie.</param>
        /// <param name="_bigBlind">Big blind pour chaque main dans la partie.</param>
        /// <param name="_antes">Antes pour chaque main de la partie.</param>
        public CGame2MaxManualController(CPlayer _btnPlayer, CPlayer _bbPlayer, double _smallBlind, double _bigBlind, double _antes, Hand _btnPlayerCards, Hand _bbPlayerCards, CBoard _board): base(new List<CPlayer>(2) { _btnPlayer, _bbPlayer }, _smallBlind, _bigBlind, _antes, true)
        {
            if ((object)_btnPlayerCards != null)
            {
                string btnCards = _btnPlayerCards.ToString().Trim();
                string btnCardsInCorrectFormat = btnCards.Substring(0, 2) + " " + btnCards.Substring(2, 2);

                FFTabJoueursCartes[0] = btnCardsInCorrectFormat;
            }
            if ((object)_bbPlayerCards != null)
            {
                string bbCards = _bbPlayerCards.ToString().Trim();
                string bbCardsInCorrectFormat = bbCards.Substring(0, 2) + " " + bbCards.Substring(2, 2);

                FFTabJoueursCartes[1] = bbCardsInCorrectFormat;
            }                

            PBoard = _board.ToString();
            FFHandFinished = false;
            PCurrentGameState = null;
        }

        protected override void Play()
        {
            UpdateCurrentPlayerAllowedActions();
        }

        public string GetBTNCards()
        {
            if (FFTabJoueursCartes[0] != null)
                return FFTabJoueursCartes[0].Replace(" ", "");
            else
                return null;
        }

        public string GetBBCards()
        {
            if (FFTabJoueursCartes[1] != null)
                return FFTabJoueursCartes[1].Replace(" ", "");
            else
                return null;
        }

        public override void PlayNewHand()
        {
            #region Local methods
            void LFReinitialiserLstCartes()
            {
                FFLstCartes.Clear();
            }

            void LFInitialiserJoueurs() 
            {
                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                {
                    if (FFResetStacksEveryHand)
                        FFTabJoueurs[indJoueur].PNumberOfChipsLeft = FFStackDepartDebutPartie[indJoueur];
                }
            }

            void LFReinitialiserStadeMain() 
            {
                PCurrentStreet = Street.Preflop;
            }
            #endregion

            LFReinitialiserLstCartes();
            LFReinitialiserStadeMain();

            LFInitialiserJoueurs();

            if (!FFHandFinished)
            {
                int indPremierJoueurAParlerPreflop = 0;

                #region Local methods
                void SelectionnerPremierJoueurAJouerPostflop()
                {
                    // S'il n'y a aucun joueur à parler postflop de sélectionné
                    if (PIndPremierJoueurAParlerPostflop == -1)
                        PIndPremierJoueurAParlerPostflop = FFLstJoueursPasFold.Last(); // Sélectionne le BB automatiquement                    
                    else
                        PIndPremierJoueurAParlerPostflop = FFIndPremierJoueurAParlerPostflopProchaineMain;

                    FFIndPremierJoueurAParlerPostflopProchaineMain = FFLstJoueursPasFold.ElemNextOf(PIndPremierJoueurAParlerPostflop);
                }
                void SelectionnerPremierJoueurAJouerPreflop()
                {
                    if (!FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    indPremierJoueurAParlerPreflop = CListHelper.ElemNextOf(FFLstJoueursPasFold, PIndPremierJoueurAParlerPostflop);
                }
                void SelectionnerDernierJoueurAJouerSelonFLstJoueursPasFold()
                {
                    if (!FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    FFIndDernierJoueurAParler = PIndPremierJoueurAParlerPostflop;
                }
                void AjouterSmallBlind()
                {
                    if (!FFLstJoueursPasFold.Contains(indPremierJoueurAParlerPreflop) || !FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
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

                }
                void AjouterBigBlind()
                {
                    if (!FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                        throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                    // Si le joueur a assez de jetons pour mettre un big blind
                    if (FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft >= PBigBlind)
                    {
                        FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PLastBet = PBigBlind;
                        FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = (FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft - PBigBlind);
                    }
                    else
                    {
                        FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PLastBet = FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft;
                        FFTabJoueurs[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = 0;
                    }

                }
                #endregion

                ++PHandCount;

                // L'ordre est important pour les procédures suivantes
                MakeEveryoneAliveAndResetBets();
                SelectionnerPremierJoueurAJouerPostflop();
                SelectionnerPremierJoueurAJouerPreflop();
                SelectionnerDernierJoueurAJouerSelonFLstJoueursPasFold();
                // Fin de l'ordre important

                bool indexZeroIsBTN = (indPremierJoueurAParlerPreflop == 0);
                if (indexZeroIsBTN)
                    PCurrentGameState = CStatePreflop.CreateNewGame(0, new List<CPlayer>() { FFTabJoueurs[0], FFTabJoueurs[1] }, PBigBlind);
                else
                    PCurrentGameState = CStatePreflop.CreateNewGame(1, new List<CPlayer>() { FFTabJoueurs[1], FFTabJoueurs[0] }, PBigBlind);

                AjouterSmallBlind();
                AjouterBigBlind();

                PPot += (PSmallBlind + PBigBlind);
                PDerniereMise = PBigBlind;
                PIndJoueurActuel = indPremierJoueurAParlerPreflop;

                RaiseOnNewHandEvent();                
                Play();
            }
            else
                GameStopped();
        }


        public override void StopGame()
        {
            throw new InvalidOperationException();
        }

        protected override void GameStopped()
        {
            throw new NotImplementedException();
        }

        protected void MakeEveryoneAliveAndResetBets()
        {
            FFLstJoueursPasFold.Clear();

            // Tout le monde est vivant.
            for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
            {
                FFLstJoueursPasFold.Add(indJoueur);
                FFTabJoueurs[indJoueur].PLastBet = 0; // Ici est la réinitialisation de la mise

                if (FFTabJoueurs[indJoueur].PNumberOfChipsLeft < FFStackDepartDebutPartie[indJoueur])
                    FFTabJoueurs[indJoueur].PNumberOfChipsLeft = FFStackDepartDebutPartie[indJoueur];

                #region Sauvegarder les stacks de départ des joueurs (utile pour les split pots et les all in pots pour savoir le effective stack
                FFStackDepartDebutMain[indJoueur] = FFTabJoueurs[indJoueur].PNumberOfChipsLeft;
                FFTabJoueurs[indJoueur].PNumberOfChipsAtBeginningHand = FFTabJoueurs[indJoueur].PNumberOfChipsLeft;
                #endregion
            }
        }

        private bool LastPlayerPlayed()
        {
            return !(((FFLstJoueursPasFold.Contains(FFIndDernierJoueurAParler) && PIndJoueurActuel != FFIndDernierJoueurAParler) || (!FFLstJoueursPasFold.Contains(FFIndDernierJoueurAParler) && PIndJoueurActuel != FFLstJoueursPasFold.Last())));
        }

        protected int GetNextPlayerIndex()
        {
            return CListHelper.ElemNextOf(FFLstJoueursPasFold, PIndJoueurActuel);
        }

        private void RaiseOnNewHandEvent()
        {
            var onNewHandEventArgs = new COnNewHandEventArgs(FFIndPremierJoueurAParlerPostflopProchaineMain,
                                                             new Tuple<int, double>(FFIndPremierJoueurAParlerPostflopProchaineMain, PSmallBlind),
                                                             new Tuple<int, double>(PIndPremierJoueurAParlerPostflop, PBigBlind),
                                                             new Dictionary<int, string>(2) { { 0, FFTabJoueurs[0].PName }, { 1, FFTabJoueurs[1].PName } },
                                                             new Dictionary<int, string>(2) { { 0, FFTabJoueursCartes[0] }, { 1, FFTabJoueursCartes[1] } },
                                                             new Dictionary<int, double>(2) { { 0, FFTabJoueurs[0].PNumberOfChipsLeft }, { 1, FFTabJoueurs[1].PNumberOfChipsLeft } });

            RaiseOnNewHandEvent(this, onNewHandEventArgs);
        }

        private void RaiseOnNewStreetEvent()
        {
            if (PCurrentStreet != Street.Flop && PCurrentStreet != Street.Turn && PCurrentStreet != Street.River)
                throw new InvalidOperationException("Invalid board");

            var card1 = PBoard.Substring(0, 2).ToCCarte();
            var card2 = PBoard.Substring(3, 2).ToCCarte();
            var card3 = PBoard.Substring(6, 2).ToCCarte();
            CCard card4 = null;
            CCard card5 = null;

            // If we are atleast "later" than the flop (so turn or river)
            if (PCurrentStreet != Street.Flop)
            {
                card4 = PBoard.Substring(9, 2).ToCCarte();

                if (PCurrentStreet == Street.River)
                    card5 = PBoard.Substring(12, 2).ToCCarte();
            }

            var onNewStreetEventArgs = new COnNewStreetEventArgs(PCurrentStreet, new CBoard(card1, card2, card3, card4, card5));
            RaiseOnNewStreetEvent(this, onNewStreetEventArgs);
        }

        protected override void ReceivedAction(CAction _action)
        {
            if (FFHandFinished)
                throw new InvalidOperationException("The hand is already finished. We cannot do an action! Call PlayNewHand before calling your action.");

            #region Local methods
            void LFSaveActionInHistory()
            {
                // Ajoute à la liste des actions du tour en cours l'action du joueur
                CPlayer currentPlayer = FFTabJoueurs[PIndJoueurActuel];

                FFLstActionsMainActuelParJoueur[currentPlayer][(int)PCurrentStreet].Add(_action);
                FFLstActionsMainActuel.Add(new Tuple<CAction, int, Street>(_action, PIndJoueurActuel, PCurrentStreet));
            }
            void LFFold()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Fold))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Fold!");
                else if (!FFLstJoueursPasFold.Contains(PIndJoueurActuel))
                    throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndJoueurActuel");

                FFTabJoueursCartes[PIndJoueurActuel] = "";
                FFLstJoueursPasFold.Remove(PIndJoueurActuel);
            }
            void LFCheck()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Check))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Check!");
            }
            void LFCall()
            {
                #region Local methods
                Func<bool> LFPlayerIsAllIn = new Func<bool>(() =>
                {
                    return (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < (PDerniereMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));
                });
                #endregion
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Call))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Call!");

                // ORDER IS IMPORTANT HERE
                double ancienneDerniereMise = FFTabJoueurs[PIndJoueurActuel].PLastBet;
                bool playerIsAllIn = LFPlayerIsAllIn();

                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                // Si le joueur est ALL IN
                if (playerIsAllIn)
                    FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = 0;
                else
                    FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - (PDerniereMise - ancienneDerniereMise));

                PPot = PPot + (FFTabJoueurs[PIndJoueurActuel].PLastBet - ancienneDerniereMise);
            }
            void LFBet()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Bet) || (PDerniereMise > 0))
                    throw new InvalidOperationException("Il y a déjà une mise en cours. On ne peut pas miser!");
                else if (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < _action.PMise)
                    throw new InvalidOperationException("Le joueur n'a pas assez de jetons pour effectuer une telle mise.");

                PPot = PPot + _action.PMise;
                PDerniereMise = _action.PMise;
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - _action.PMise);
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);
            }
            void LFRaise()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Raise))
                {
                    Console.WriteLine("Possible hand conflict detected: " + FFTabJoueurs[PIndJoueurActuel].PName.ToString() + " | Other player name: " + FFTabJoueurs[PIndJoueurActuel].PName.ToString() + " | Pot before the raise: " + PPot.ToString());
                }
                //  throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Raise!");
                /* else if ((FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft + FFTabJoueurs[PIndJoueurActuel].PLastBet) < _action.PMise)
                        throw new InvalidOperationException("Le joueur doit avoir le nombre de jetons nécessaire pour effectuer une relance.");
                    else if (_action.PMise < (PDerniereMise + PBigBlind))
                        throw new InvalidOperationException("La mise doit être au moins un big blind de plus que la dernière mise.");*/


                PPot = PPot + (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet);
                PDerniereMise = _action.PMise;
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);
            }
            #endregion

            LFSaveActionInHistory();

            #region Execute the action
            switch (_action.PAction)
            {
                case PokerAction.Fold:
                    LFFold();
                    PCurrentGameState = PCurrentGameState.Fold();
                    break;
                case PokerAction.Check:
                    LFCheck();
                    PCurrentGameState = PCurrentGameState.Check();
                    break;
                case PokerAction.Call:
                    LFCall();
                    PCurrentGameState = PCurrentGameState.Call();
                    break;
                case PokerAction.Bet:
                    LFBet();
                    PCurrentGameState = PCurrentGameState.Bet(_action.PMise);
                    break;
                case PokerAction.Raise:
                    LFRaise();
                    PCurrentGameState = PCurrentGameState.Raise(_action.PMise);
                    break;
                default:
                    throw new InvalidOperationException("Invalid action");
            }
            #endregion

            RaiseOnNewActionEvent(this, new COnNewActionEventArgs(PIndJoueurActuel, PPot, _action, new Dictionary<int, CPlayer>(2) { { 0, FFTabJoueurs[0] }, { 1, FFTabJoueurs[1] } }));
            GameStateChanged();            
        }

        /// <summary>
        /// Event that is called everytime that the something changed in the game (example: A player checked.)
        /// </summary>
        protected override void GameStateChanged()
        {
            #region Local methods
            bool LFOnePlayerIsAllIn()
            {
                return ((FFTabJoueurs[0].PNumberOfChipsLeft == 0) || (FFTabJoueurs[1].PNumberOfChipsLeft == 0));
            }
            void LFChangerStadeTour()
            {
                ++PCurrentStreet;
                PDerniereMise = 0;
                RaiseOnNewStreetEvent();
            }
            void LFMainFini()
            {
                int winnerPlayerIndex = 0;
                bool splitPot = false; // Workaround for HU.
                //List<Tuple<int, double>> _lstPlayersThatWonThePot, Dictionary<int, string> _playerCards
                Dictionary<int, string> playerCards = new Dictionary<int, string>(FFLstJoueursPasFold.Count);
                List<Tuple<int, double>> lstPlayersThatWonThePot = new List<Tuple<int, double>>();

                #region Méthodes
                void Showdown()
                {
                    for (int i = 0; i < FFLstJoueursPasFold.Count; i++)
                    {
                        int ind = FFLstJoueursPasFold[i];

                        playerCards.Add(ind, FFTabJoueursCartes[ind]);
                    }

                    Hand bestHand = new Hand(FFTabJoueursCartes[FFLstJoueursPasFold[0]], PBoard);
                    winnerPlayerIndex = 0;

                    for (int i = 1; i < FFLstJoueursPasFold.Count; i++)
                    {
                        Hand currentPlayerHand = new Hand(FFTabJoueursCartes[FFLstJoueursPasFold[i]], PBoard);
                        if (currentPlayerHand >= bestHand)
                        {
                            if (currentPlayerHand == bestHand)
                                splitPot = true;

                            winnerPlayerIndex = i;
                            bestHand = currentPlayerHand;
                        }
                    }
                }
                #endregion

                if (FFLstJoueursPasFold.Count == 1)
                    winnerPlayerIndex = FFLstJoueursPasFold.First();
                else if (FFTabJoueursCartes[0] != null && FFTabJoueursCartes[1] != null)
                    Showdown();

                if (FFTabJoueursCartes[0] != null && FFTabJoueursCartes[1] != null)
                {
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

                        lstPlayersThatWonThePot.Add(new Tuple<int, double>(winnerPlayerIndex, firstPlayer.PNumberOfChipsLeft));
                        lstPlayersThatWonThePot.Add(new Tuple<int, double>(CListHelper.ElemNextOf(FFLstJoueursPasFold, winnerPlayerIndex), secondPlayer.PNumberOfChipsLeft));
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
                                    winningPlayer.PSessionInfo.PNbProfit += ((FFStackDepartDebutMain[winnerPlayerIndex] * 2) / 2);

                                    losingPlayer.PNumberOfChipsLeft = losingPlayer.PNumberOfChipsLeft + (PPot - winningPlayer.PNumberOfChipsLeft);
                                    losingPlayer.PSessionInfo.PNbProfit -= ((FFStackDepartDebutMain[winnerPlayerIndex] * 2) / 2);
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
                                winningPlayer.PSessionInfo.PNbProfit += (PPot / 2);
                                losingPlayer.PSessionInfo.PNbProfit -= (PPot / 2);
                            }
                        }

                        lstPlayersThatWonThePot.Add(new Tuple<int, double>(winnerPlayerIndex, PPot));
                    }

                    RaiseOnHandFinishedEvent(this, new COnHandFinishedEventArgs(lstPlayersThatWonThePot, playerCards));

                    #region Clear current hand history of players
                    for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                    {
                        FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)Street.Preflop].Clear();
                        FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)Street.Flop].Clear();
                        FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)Street.Turn].Clear();
                        FFLstActionsMainActuelParJoueur[FFTabJoueurs[indJoueur]][(int)Street.River].Clear();
                    }

                    FFLstActionsMainActuel.Clear();
                    #endregion

                    PPot = 0;
                }

                FFHandFinished = true;
            }
            #endregion
            
            // If everyone folded except one person
            if (FFLstJoueursPasFold.Count == 1)            
                LFMainFini();            
            // If we are at the end of the street (preflop, flop, turn ou river).
            else if (LastPlayerPlayed())
            {
                if (PCurrentStreet == Street.River)                
                    LFMainFini();                
                else
                {
                    if (LFOnePlayerIsAllIn())
                    {
                        #region One player is all in and we can be on preflop, flop or the turn.
                        while (PCurrentStreet != Street.River)
                            LFChangerStadeTour();

                        LFMainFini();
                        #endregion
                    }
                    else
                    {
                        #region Change the current street to another street (example: from preflop to flop)
                        LFChangerStadeTour();

                        PIndJoueurActuel = PIndPremierJoueurAParlerPostflop;
                        FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);

                        FFTabJoueurs[0].PLastBet = 0;
                        FFTabJoueurs[1].PLastBet = 0;                        
                        
                        Play();
                        #endregion
                    }
                }
            }
            else
            {
                PIndJoueurActuel = GetNextPlayerIndex();
                Play();
            }
        }

        private void UpdateCurrentPlayerAllowedActions()
        {
            if (FFLstJoueursPasFold.IndexOf(PIndJoueurActuel) == -1)
                throw new InvalidOperationException("L'indice de joueur actuel ne correspond à aucun joueur qui joue en cours!");

            CPlayer joueurTourActuel = FFTabJoueurs[PIndJoueurActuel];
            CAction derniereAction = null;

            // On prend la dernière action que le joueur actuel a effectué dans le tour actuel (s'il y en a une)
            if (FFLstActionsMainActuelParJoueur[joueurTourActuel][(int)PCurrentStreet].Count > 0)
                derniereAction = FFLstActionsMainActuelParJoueur[joueurTourActuel][(int)PCurrentStreet].Last();
            else
                derniereAction = new CAction(PokerAction.None);

            PLstActionsPossibleJoueurActuel.Clear();
            PLstActionsPossibleJoueurActuel.Add(PokerAction.Fold);

            if (PDerniereMise == 0)
            {
                PLstActionsPossibleJoueurActuel.Add(PokerAction.Check);

                if (joueurTourActuel.PNumberOfChipsLeft >= PBigBlind)
                    PLstActionsPossibleJoueurActuel.Add(PokerAction.Bet);
            }
            else if (derniereAction.PMise <= PDerniereMise)
            {
                // Si le joueur a déjà mis une mise (situation qui arrivera probablement jamais) ou est big blind (seule situation qui va arriver probablement)
                if (derniereAction.PMise == PDerniereMise || FFTabJoueurs[PIndJoueurActuel].PLastBet == PDerniereMise)
                    PLstActionsPossibleJoueurActuel.Add(PokerAction.Check);
                else
                    PLstActionsPossibleJoueurActuel.Add(PokerAction.Call);

                if (joueurTourActuel.PNumberOfChipsLeft >= (PDerniereMise + PBigBlind - joueurTourActuel.PLastBet))
                {
                    CPlayer playerThatIsAllIn = null;

                    foreach (CPlayer player in FFTabJoueurs)
                    {
                        if (player.PNumberOfChipsLeft == 0)
                        {
                            playerThatIsAllIn = player;
                            break;
                        }
                    }

                    if (playerThatIsAllIn == null)
                        PLstActionsPossibleJoueurActuel.Add(PokerAction.Raise);
                }
            }
        }
    }
}
