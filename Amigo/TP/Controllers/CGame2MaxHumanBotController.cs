using System;
using System.Collections.Generic;
using System.Linq;
using Amigo.Bots;
using Shared.Poker.Models;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using Shared.Helpers;
using static Shared.Poker.Models.CTableInfos;
using System.IO;
using System.Windows.Forms;
using Shared.Models;
using Amigo.Interfaces;
using Amigo.Events;
using HoldemHand;
using Amigo.Helpers;
using System.Threading.Tasks;

namespace Amigo.Controllers
{
    public class CGame2MaxHumanBotController : CGameController, IGameHumanVsBotController
    {
        private static object lockObject = new object();

        #region Events
        public event EventHandler<COnWaitingForHumanActionEventArgs> POnWaitingForHumanAction;
        #endregion

        #region Données membres, accesseurs, constantes et énumérés

        private int FFHumanPlayerIndex;
        private CBotPoker FFBot;
        private AState FFCurrentGameState;

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
        public CGame2MaxHumanBotController(CPlayer _btnHumanPlayer, (CPlayer, CBotPoker) _bbBotPlayer, double _smallBlind, double _bigBlind, double _antes, bool _resetStackEveryHand) : base(new List<CPlayer>(2) { _btnHumanPlayer, _bbBotPlayer.Item1 }, _smallBlind, _bigBlind, _antes, _resetStackEveryHand)
        {
            if (_btnHumanPlayer.PNumberOfChipsLeft != _btnHumanPlayer.PNumberOfChipsAtBeginningHand)
                throw new ArgumentException("The number of chips left must be the same number as the number of chips at the beginning of the hand");
            else if (_bbBotPlayer.Item1.PNumberOfChipsLeft != _bbBotPlayer.Item1.PNumberOfChipsAtBeginningHand)
                throw new ArgumentException("The number of chips left must be the same number as the number of chips at the beginning of the hand");

            FFHumanPlayerIndex = 0;
            FFBot = _bbBotPlayer.Item2;

            if (_btnHumanPlayer.PNumberOfChipsAtBeginningHand > _bbBotPlayer.Item1.PNumberOfChipsAtBeginningHand)
            {
                _btnHumanPlayer.PNumberOfChipsAtBeginningHand = _bbBotPlayer.Item1.PNumberOfChipsAtBeginningHand;
                _btnHumanPlayer.PNumberOfChipsLeft = _bbBotPlayer.Item1.PNumberOfChipsLeft;
            }
            else if (_bbBotPlayer.Item1.PNumberOfChipsAtBeginningHand > _btnHumanPlayer.PNumberOfChipsAtBeginningHand)
            {
                _bbBotPlayer.Item1.PNumberOfChipsAtBeginningHand = _btnHumanPlayer.PNumberOfChipsAtBeginningHand;
                _bbBotPlayer.Item1.PNumberOfChipsLeft = _btnHumanPlayer.PNumberOfChipsLeft;
            }

            // Création des joueurs
            FFTabJoueurs = new CPlayer[2] { _btnHumanPlayer, _bbBotPlayer.Item1 };
        }

        public override void PlayNewHand()
        {
            #region Local methods
            void LFReinitialiserLstCartes()
            {
                FFLstCartes.Clear();
                #region Création de la liste des cartes
                FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Clubs));
                FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Spades));
                FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Hearts));
                FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Diamonds));
                FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Clubs));
                #endregion
            }

            void LFInitialiserJoueurs()
            {
                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                {
                    if (FFResetStacksEveryHand)
                        FFTabJoueurs[indJoueur].PNumberOfChipsLeft = FFStackDepartDebutPartie[indJoueur];

                    #region Distribuer des cartes aux joueurs
                    DistribuerCartes(indJoueur, 2);
                    #endregion
                }
            }

            void LFReinitialiserStadeMain()
            {
                PBoard = "";
                PCurrentStreet = Street.Preflop;
            }
            #endregion

            MakeEveryoneAliveAndResetBets();
            LFReinitialiserLstCartes();
            LFReinitialiserStadeMain();

            LFInitialiserJoueurs();

            if (PContinuePlaying)
            {
                int indPremierJoueurAParlerPreflop = 0;

                #region Local methods
                void SelectionnerPremierJoueurAJouerPostflop()
                {
                    // S'il n'y a aucun joueur à parler postflop de sélectionné
                    if (PIndPremierJoueurAParlerPostflop == -1)
                        PIndPremierJoueurAParlerPostflop = 1; // Sélectionne le BB automatiquement                    
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
                SelectionnerPremierJoueurAJouerPostflop();
                SelectionnerPremierJoueurAJouerPreflop();
                SelectionnerDernierJoueurAJouerSelonFLstJoueursPasFold();
                // Fin de l'ordre important

                CDBHelperHandInfos.PLstAllBoardsInfos.Clear();
                CDBHelperHandInfos.PLstAllBoardsInfos.Add(new Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, Hand.HandTypes)>>(100));
                CDBHelperHandInfos.PLstAllBoardsInfos.Add(new Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, Hand.HandTypes)>>(100));

                bool indexZeroIsBTN = (indPremierJoueurAParlerPreflop == 0);
                if (indexZeroIsBTN)
                    FFCurrentGameState = CStatePreflop.CreateNewGame(0, new List<CPlayer>() { FFTabJoueurs[0], FFTabJoueurs[1] }, PBigBlind);
                else
                    FFCurrentGameState = CStatePreflop.CreateNewGame(1, new List<CPlayer>() { FFTabJoueurs[1], FFTabJoueurs[0] }, PBigBlind);


                FFBot.CreateNewHand();

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
            PContinuePlaying = false;
        }

        protected override void GameStopped()
        {
            //PEventGameStopped(this);
        }

        protected void RaiseOnWaitingForHumanActionEvent(object sender, COnWaitingForHumanActionEventArgs e)
        {
            POnWaitingForHumanAction?.Invoke(sender, e);
        }

        private bool LastPlayerPlayed()
        {
            return (PIndJoueurActuel == FFIndDernierJoueurAParler);
        }

        protected int GetNextPlayerIndex()
        {
            return CListHelper.ElemNextOf(FFLstJoueursPasFold, PIndJoueurActuel);
        }

        private void RaiseOnNewHandEvent()
        {
            int botPlayerIndex = (FFHumanPlayerIndex == 0) ? 1 : 0;

            var onNewHandEventArgs = new COnNewHandEventArgs(FFIndPremierJoueurAParlerPostflopProchaineMain,
                                                             new Tuple<int, double>(FFIndPremierJoueurAParlerPostflopProchaineMain, PSmallBlind),
                                                             new Tuple<int, double>(PIndPremierJoueurAParlerPostflop, PBigBlind),
                                                             new Dictionary<int, string>(2) { { 0, FFTabJoueurs[0].PName }, { 1, FFTabJoueurs[1].PName } },
                                                             new Dictionary<int, string>(2) { { FFHumanPlayerIndex, FFTabJoueursCartes[FFHumanPlayerIndex] }, { botPlayerIndex, "" } },
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
                FFTabJoueurs[indJoueur].PNumberOfChipsAtBeginningHand = FFStackDepartDebutPartie[indJoueur];
                #endregion
            }
        }

        /// <summary>
        /// Event that is called everytime a player made an action.
        /// </summary>
        /// <param name="_action">Action that the player did</param>
        protected override void ReceivedAction(CAction _action)
        {
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
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Call))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Call!");

                PPot = PPot + (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet);
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - (_action.PMise - FFTabJoueurs[PIndJoueurActuel].PLastBet));
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;
            }
            void LFBet()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Bet))
                    throw new InvalidOperationException("The current player is not allowed to bet");
                else if (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft < _action.PMise)
                    throw new InvalidOperationException("The current player does not have enough chips to bet this amount");

                PPot = PPot + _action.PMise;
                PDerniereMise = _action.PMise;
                FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft = (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft - _action.PMise);
                FFTabJoueurs[PIndJoueurActuel].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndJoueurActuel);
            }
            void LFRaise()
            {
                if (!PLstActionsPossibleJoueurActuel.Contains(PokerAction.Raise))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Raise!");
                else if ((FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft + FFTabJoueurs[PIndJoueurActuel].PLastBet) < _action.PMise)
                    throw new InvalidOperationException("The current player does not have enough chips to raise this amount");

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
                    FFCurrentGameState.Fold();
                    break;
                case PokerAction.Check:
                    LFCheck();
                    FFCurrentGameState.Check();
                    break;
                case PokerAction.Call:
                    LFCall();
                    FFCurrentGameState.Call();
                    break;
                case PokerAction.Bet:
                    LFBet();
                    FFCurrentGameState.Bet(_action.PMise);
                    break;
                case PokerAction.Raise:
                    LFRaise();
                    FFCurrentGameState.Raise(_action.PMise);
                    break;
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

                switch (PCurrentStreet)
                {
                    case Street.Flop:
                        string carte1 = GetRandomCard().ToString();
                        string carte2 = GetRandomCard().ToString();
                        string carte3 = GetRandomCard().ToString();

                        PBoard = carte1 + " " + carte2 + " " + carte3;

                        int botPlayerIndex = (FFHumanPlayerIndex == 0) ? 1 : 0;                        
                        string botCards = FFTabJoueursCartes[botPlayerIndex].Replace(" ", "");

                        CDBHelperHandInfos.LoadBoardInfosAsync(Hand.ParseHand((string)string.Concat(carte1, carte2, carte3).Clone()), Hand.ParseHand((string)botCards.Clone()), botPlayerIndex);
                        break;
                    case Street.Turn:
                        string carte4 = GetRandomCard().ToString();

                        PBoard += " " + carte4;
                        break;
                    case Street.River:
                        string carte5 = GetRandomCard().ToString();

                        PBoard += " " + carte5;
                        break;
                    default:
                        throw new InvalidOperationException("Stade de tour (flop, turn ou river) invalide.");
                }

                RaiseOnNewStreetEvent();
            }
            void LFMainFini()
            {
                int winnerPlayerIndex = 0;
                bool splitPot = false; // Workaround for HU.
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
                Task.Delay(3000).Wait();

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
            #endregion

            // If everyone folded except one person
            if (FFLstJoueursPasFold.Count == 1)
            {
                LFMainFini();
                PlayNewHandAsync();
            }
            // If we are at the end of the street (preflop, flop, turn ou river).
            else if (LastPlayerPlayed())
            {
                if (PCurrentStreet == Street.River)
                {
                    LFMainFini();
                    PlayNewHandAsync();
                }
                else
                {
                    if (LFOnePlayerIsAllIn())
                    {
                        #region One player is all in and we can be on preflop, flop or the turn.
                        while (PCurrentStreet != Street.River)
                            LFChangerStadeTour();

                        LFMainFini();
                        PlayNewHandAsync();
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

        #region Common methods
        /// <summary>
        /// Jouer une main (celle-ci se fait rappeler à chaque tour de joueur) (se répète tant et aussi longtemps que Jouer()) se fait appelé
        /// </summary>
        protected override sealed void Play()
        {
            #region Local methods
            void MakeDecision()
            {
                if (PIndJoueurActuel == FFHumanPlayerIndex)
                {
                    int botPlayerIndex = (FFHumanPlayerIndex == 0) ? 1 : 0;
                    RaiseOnWaitingForHumanActionEvent(this, new COnWaitingForHumanActionEventArgs(PPot, FFTabJoueurs[FFHumanPlayerIndex].PNumberOfChipsLeft, FFTabJoueurs[botPlayerIndex].PLastBet, PLstActionsPossibleJoueurActuel));
                }
                else
                {
                    Hand botCards = null;

                    if (!string.IsNullOrEmpty(PBoard))
                        botCards = new Hand(FFTabJoueursCartes[PIndJoueurActuel], PBoard);
                    else
                        botCards = new Hand() { PocketCards = FFTabJoueursCartes[PIndJoueurActuel] };

                    int botPlayerIndex = -1;

                    if (FFCurrentGameState.PActionList.Count > 0)
                    {
                        if (PIndJoueurActuel == FFCurrentGameState.PActionList[0].Item2)
                            botPlayerIndex = (PIndJoueurActuel ^ 1);
                        else
                            botPlayerIndex = PIndJoueurActuel;
                    }

                    FFBot.GetDecisionAsync(FFCurrentGameState, botCards, botPlayerIndex).ContinueWith((decision) =>
                    {
                        switch (decision.Result.PAction)
                        {
                            case PokerAction.Fold:
                                Fold();
                                break;
                            case PokerAction.Call:
                                Call();
                                break;
                            case PokerAction.Check:
                                Check();
                                break;
                            case PokerAction.Bet:
                                Bet(decision.Result.PMise);
                                break;
                            case PokerAction.Raise:
                                Raise(decision.Result.PMise);
                                break;
                        }
                    });
                }
            }
            #endregion
            if (PContinuePlaying)
            {
                UpdateCurrentPlayerAllowedActions();
                MakeDecision();
            }
            else
                GameStopped();
        }
        #endregion
        #region Methods that are related to the players
        private void UpdateCurrentPlayerAllowedActions()
        {
            if (FFLstJoueursPasFold.IndexOf(PIndJoueurActuel) == -1)
                throw new InvalidOperationException("L'indice de joueur actuel ne correspond à aucun joueur qui joue en cours!");

            CPlayer joueurTourActuel = FFTabJoueurs[PIndJoueurActuel];

            if (joueurTourActuel.PNumberOfChipsLeft == 0)
                throw new InvalidOperationException("Cannot call this method when the current player is all in since he is not allowed to act");
            else if (PDerniereMise > joueurTourActuel.PNumberOfChipsAtBeginningHand)
                throw new InvalidOperationException("This class does not support stacks that are not equal. In other words, this should NEVER happen!");

            PLstActionsPossibleJoueurActuel.Clear();
            PLstActionsPossibleJoueurActuel.Add(PokerAction.Fold);

            // On vérifie si:
            //   1 - Le joueur a déjà mis une mise (situation qui arrive dans le cas où il join une table et il décide de payer le fee pour jouer toute suite, le fee équivalent à un big blind)
            //   2 - Le joueur est big blind (situation qui arrive preflop, lorsque BTN limp)
            //   3 - Le joueur adverse n'a pas misé, donc le joueur présent peut checker
            if (joueurTourActuel.PLastBet == PDerniereMise)
            {
                PLstActionsPossibleJoueurActuel.Add(PokerAction.Check);

                if (FFLstActionsMainActuel.Count > 0)
                {
                    var indLastAction = (FFLstActionsMainActuel.Count - 1);
                    var lastAction = FFLstActionsMainActuel[indLastAction].Item1.PAction;
                    var lastActionStreet = FFLstActionsMainActuel[indLastAction].Item3;

                    if (lastActionStreet == PCurrentStreet)
                    {
                        if (lastAction == PokerAction.Check)
                            PLstActionsPossibleJoueurActuel.Add(PokerAction.Bet);
                    }
                    else
                        PLstActionsPossibleJoueurActuel.Add(PokerAction.Bet);
                }
            }
            else
                PLstActionsPossibleJoueurActuel.Add(PokerAction.Call); // On fais l'assumption que s'il pouvait bet/raise avant, c'est que le joueur présent pouvait call (on met les deux stacks au même niveau à la création de la partie)

            // Si la dernière mise ne met pas le joueur présent all in, ça veut dire implicitement que le joueur présent peut aller all in, puisque les stacks doivent être égal au début de la partie.
            if ((FFTabJoueurs[PIndJoueurActuel ^ 1].PNumberOfChipsLeft > 0) && !PLstActionsPossibleJoueurActuel.Contains(PokerAction.Bet))
                PLstActionsPossibleJoueurActuel.Add(PokerAction.Raise);
        }
        #endregion    
    }
}
