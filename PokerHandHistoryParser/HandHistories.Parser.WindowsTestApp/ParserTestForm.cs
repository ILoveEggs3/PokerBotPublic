using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using HandHistories.Objects.GameDescription;
using HandHistories.Parser.Parsers.Factory;
using System.Diagnostics;
using HandHistories.Parser.Parsers.FastParser.Base;
using HandHistories.Objects.Cards;
using HoldemHand;
using HandHistories.Objects.Actions;
using HandHistories.Objects.Players;
using System.IO;
using static Shared.Poker.Models.CTableInfos;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CAction;
using Shared.Poker.Models;
using Amigo.Helpers;
using Shared.Models.Database;
using Amigo.Bots;
using static Shared.Models.Database.CBoardModel;
using Shared.Helpers;
using Amigo.Controllers;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace HandHistories.Parser.WindowsTestApp
{
    public partial class ParserTestForm : Form
    {
        const UInt16 METADATA_IS_PAIRED_MASK = 0x1 << 0;
        const UInt16 METADATA_IS_2_PAIRED_MASK = 0x1 << 1;
        const UInt16 METADATA_IS_TRIPS_MASK = 0x1 << 2;
        const UInt16 METADATA_IS_FULL_HOUSE = 0x1 << 3;
        const UInt16 METADATA_IS_QUADS = 0x1 << 4;
        const UInt16 METADATA_IS_STRAIGHT_DRAW = 0x1 << 5;
        const UInt16 METADATA_IS_STRAIGHT = 0x1 << 6;
        const UInt16 METADATA_IS_ONE_CARD_STRAIGHT = 0x1 << 7;
        const UInt16 METADATA_IS_STRAIGHT_COMPLETE = 0x1 << 8;
        const UInt16 METADATA_IS_FLUSH_DRAW = 0x1 << 9;
        const UInt16 METADATA_IS_FLUSH = 0x1 << 10;
        const UInt16 METADATA_IS_ONE_CARD_FLUSH = 0x1 << 11;
        const UInt16 METADATA_IS_FLUSH_COMPLETE = 0x1 << 12;

        public ParserTestForm()
        {
            InitializeComponent();

            listBoxSite.Items.Add(SiteName.BossMedia);
            listBoxSite.Items.Add(SiteName.PokerStars);
            listBoxSite.Items.Add(SiteName.PokerStarsFr);
            listBoxSite.Items.Add(SiteName.PokerStarsIt);
            listBoxSite.Items.Add(SiteName.PokerStarsEs);
            listBoxSite.Items.Add(SiteName.FullTilt);
            listBoxSite.Items.Add(SiteName.PartyPoker);
            listBoxSite.Items.Add(SiteName.IPoker);
            listBoxSite.Items.Add(SiteName.OnGame);
            listBoxSite.Items.Add(SiteName.OnGameFr);
            listBoxSite.Items.Add(SiteName.OnGameIt);
            listBoxSite.Items.Add(SiteName.Pacific);
            listBoxSite.Items.Add(SiteName.Entraction);
            listBoxSite.Items.Add(SiteName.Merge);
            listBoxSite.Items.Add(SiteName.WinningPoker);
            listBoxSite.Items.Add(SiteName.MicroGaming);
        }

        private void buttonParse_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                // Order is important
                CDBHelper.CreateAllGameStatesTableIfNotExist(false);
                CDBHelper.CreateAllGameStatesTableIfNotExist(true);
                CDBHelper.LoadAllGameStates();
                CDBHelperHandInfos.LoadAllBoards();
                // End of important order
                CDBHelper.CreateAllGameStatesOtherStatsTableIfNotExist();
                CDBHelper.CreateAllGameStatesFoldStatsTableIfNotExist();                
                CDBHelper.CreateAveragePlayerPreflopRangesTableIfNotExist();
                CDBHelper.CreateAllAveragePlayerStatsFlopTableIfNotExist();
                CDBHelper.CreateAllAveragePlayerStatsTurnTableIfNotExist();
                CDBHelper.CreateAllAveragePlayerStatsRiverTableIfNotExist();
                CDBHelper.LoadAllFlopGameStatesFoldStats();
                CDBHelper.LoadAllFlopGameStatesOtherStats();
                CDBHelper.LoadAllTurnGameStatesFoldStats();
                CDBHelper.LoadAllTurnGameStatesOtherStats();
                CDBHelper.LoadAllRiverGameStatesFoldStats();
                CDBHelper.LoadAllRiverGameStatesOtherStats();                
                Thread.MemoryBarrier();

                IHandHistoryParserFactory factory = new HandHistoryParserFactoryImpl();
                var handParser = factory.GetFullHandHistoryParser(SiteName.PokerStars);
                int parsedHands = 0;

                try
                {

                    Stopwatch SW = new Stopwatch();
                    HandHistoryParserFastImpl fastParser = handParser as HandHistoryParserFastImpl;
                    IEnumerable<string> allTxtFiles = Directory.EnumerateFiles(@"C:\PokerStarsHands\", "*.txt");

                    foreach (string currentFileName in allTxtFiles)
                    {
                        GC.Collect();

                        #region Initialization
                        // Preflop
                        ConcurrentBag<CAveragePlayerPreflopRange> lstAvgPlayerPreflopRange = new ConcurrentBag<CAveragePlayerPreflopRange>();

                        // Flop
                        ConcurrentBag<CAveragePlayerBluffsFlop> lstAvgPlayerBluffsFlop = new ConcurrentBag<CAveragePlayerBluffsFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsFlop, CDebugGeneralHandInfos>> lstDebugFlop = new ConcurrentBag<Tuple<CAveragePlayerBluffsFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityFlop> lstAvgPlayerLotsEquityFlop = new ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityFlop, CDebugGeneralHandInfos>> lstDebugFlop2 = new ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandFDFlop> lstAvgPlayerMadeHandFDFlop = new ConcurrentBag<CAveragePlayerMadeHandFDFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandFDFlop, CDebugGeneralHandInfos>> lstDebugFlop3 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandFDFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandSDAndFDFlop> lstAvgPlayerMadeHandSDAndFDFlop = new ConcurrentBag<CAveragePlayerMadeHandSDAndFDFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDFlop, CDebugGeneralHandInfos>> lstDebugFlop4 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandSDFlop> lstAvgPlayerMadeHandSDFlop = new ConcurrentBag<CAveragePlayerMadeHandSDFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandSDFlop, CDebugGeneralHandInfos>> lstDebugFlop5 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandSDFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerValueHandsFlop> lstAvgPlayerMadeHandValueHandFlop = new ConcurrentBag<CAveragePlayerValueHandsFlop>();
                        ConcurrentBag<Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>> lstDebugFlop6 = new ConcurrentBag<Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CFlopFoldStats> lstFlopFoldsStats = new ConcurrentBag<CFlopFoldStats>();
                        ConcurrentBag<CFlopOtherStats> lstFlopOtherStats = new ConcurrentBag<CFlopOtherStats>();

                        // Turn
                        ConcurrentBag<CAveragePlayerBluffsTurn> lstAvgPlayerBluffsTurn = new ConcurrentBag<CAveragePlayerBluffsTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsTurn, CDebugGeneralHandInfos>> lstDebugTurn = new ConcurrentBag<Tuple<CAveragePlayerBluffsTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityTurn> lstAvgPlayerLotsEquityTurn = new ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityTurn, CDebugGeneralHandInfos>> lstDebugTurn2 = new ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandFDTurn> lstAvgPlayerMadeHandFDTurn = new ConcurrentBag<CAveragePlayerMadeHandFDTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandFDTurn, CDebugGeneralHandInfos>> lstDebugTurn3 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandFDTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandSDAndFDTurn> lstAvgPlayerMadeHandSDAndFDTurn = new ConcurrentBag<CAveragePlayerMadeHandSDAndFDTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDTurn, CDebugGeneralHandInfos>> lstDebugTurn4 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandSDTurn> lstAvgPlayerMadeHandSDTurn = new ConcurrentBag<CAveragePlayerMadeHandSDTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandSDTurn, CDebugGeneralHandInfos>> lstDebugTurn5 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandSDTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerValueHandsTurn> lstAvgPlayerMadeHandValueHandTurn = new ConcurrentBag<CAveragePlayerValueHandsTurn>();
                        ConcurrentBag<Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>> lstDebugTurn6 = new ConcurrentBag<Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CTurnFoldStats> lstTurnFoldsStats = new ConcurrentBag<CTurnFoldStats>();
                        ConcurrentBag<CTurnOtherStats> lstTurnOtherStats = new ConcurrentBag<CTurnOtherStats>();

                        // River
                        ConcurrentBag<CAveragePlayerBluffsRiver> lstAvgPlayerBluffsRiver = new ConcurrentBag<CAveragePlayerBluffsRiver>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsRiver, CDebugGeneralHandInfos>> lstDebugRiver = new ConcurrentBag<Tuple<CAveragePlayerBluffsRiver, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityRiver> lstAvgPlayerLotsEquityRiver = new ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityRiver>();
                        ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityRiver, CDebugGeneralHandInfos>> lstDebugRiver2 = new ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityRiver, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerMadeHandBlockerRiver> lstAvgPlayerMadeHandBlockerRiver = new ConcurrentBag<CAveragePlayerMadeHandBlockerRiver>();
                        ConcurrentBag<Tuple<CAveragePlayerMadeHandBlockerRiver, CDebugGeneralHandInfos>> lstDebugRiver3 = new ConcurrentBag<Tuple<CAveragePlayerMadeHandBlockerRiver, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CAveragePlayerValueHandsRiver> lstAvgPlayerMadeHandValueHandRiver = new ConcurrentBag<CAveragePlayerValueHandsRiver>();
                        ConcurrentBag<Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>> lstDebugRiver4 = new ConcurrentBag<Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>>();
                        ConcurrentBag<CRiverFoldStats> lstRiverFoldsStats = new ConcurrentBag<CRiverFoldStats>();
                        ConcurrentBag<CRiverOtherStats> lstRiverOtherStats = new ConcurrentBag<CRiverOtherStats>();
                        #endregion

                        string fileText = File.ReadAllText(currentFileName);
                        IEnumerable<string> handHistories = fastParser.SplitUpMultipleHands(fileText);

                        SW.Restart();
                        // We don't want to take the last line
                        Parallel.ForEach<string>(handHistories, ((currentHandHistory) =>
                        {
                            try
                            {
                                #region Code in try
                                var parsedHand = fastParser.ParseFullHandHistory(currentHandHistory, true);

                                void ParseHandFromPlayer(int _playerIndex)
                                {
                                    #region Parse hand from Player X                                    
                                    // Just in case someone showed his card before the river
                                    bool hasHoleCards = parsedHand.Players[_playerIndex].hasHoleCards;
                                    Player currentPlayer = parsedHand.Players[_playerIndex];

                                    BoardCards flopBoard = BoardCards.ForFlop(parsedHand.ComumnityCards[0], parsedHand.ComumnityCards[1], parsedHand.ComumnityCards[2]);
                                    BoardCards turnBoard = null;
                                    BoardCards riverBoard = null;

                                    if (parsedHand.ComumnityCards.Count > 3)
                                    {
                                        turnBoard = BoardCards.ForTurn(parsedHand.ComumnityCards[0], parsedHand.ComumnityCards[1], parsedHand.ComumnityCards[2], parsedHand.ComumnityCards[3]);

                                        if (parsedHand.ComumnityCards.Count == 5)
                                            riverBoard = parsedHand.ComumnityCards;
                                    }

                                    HoleCards holeCards = null;
                                    Hand handConvertedFlop = null;
                                    Hand handConvertedTurn = null;
                                    Hand handConvertedRiver = null;

                                    if (hasHoleCards)
                                    {
                                        holeCards = currentPlayer.HoleCards;
                                        handConvertedFlop = new Hand(holeCards.ToString(), flopBoard.ToString());
                                        handConvertedTurn = new Hand(holeCards.ToString(), turnBoard.ToString());
                                        handConvertedRiver = new Hand(holeCards.ToString(), riverBoard.ToString());
                                    }

                                    Hand handFlopBoardOnly = new Hand() { Board = flopBoard.ToString() };
                                    Hand handTurnBoardOnly = null;
                                    Hand handRiverBoardOnly = null;

                                    if (turnBoard != null)
                                    {
                                        handTurnBoardOnly = new Hand() { Board = turnBoard.ToString() };

                                        if (riverBoard != null)
                                            handRiverBoardOnly = new Hand() { Board = riverBoard.ToString() };
                                    }

                                    bool isButton = (currentPlayer.SeatNumber == parsedHand.DealerButtonPosition);

                                    CGameStateNLHE2Max simulatedGameState = null;

                                    if (isButton)
                                        simulatedGameState = new CGameStateNLHE2Max(new CPlayer(currentPlayer.StartingStack, currentPlayer.PlayerName),
                                                                                    new CPlayer(parsedHand.Players[_playerIndex ^ 1].StartingStack, parsedHand.Players[_playerIndex ^ 1].PlayerName),
                                                                                    parsedHand.GameDescription.Limit.SmallBlind,
                                                                                    parsedHand.GameDescription.Limit.BigBlind,
                                                                                    parsedHand.GameDescription.Limit.Ante);
                                    else
                                        simulatedGameState = new CGameStateNLHE2Max(new CPlayer(parsedHand.Players[_playerIndex ^ 1].StartingStack, parsedHand.Players[_playerIndex ^ 1].PlayerName),
                                                                                    new CPlayer(currentPlayer.StartingStack, currentPlayer.PlayerName),
                                                                                    parsedHand.GameDescription.Limit.SmallBlind,
                                                                                    parsedHand.GameDescription.Limit.BigBlind,
                                                                                    parsedHand.GameDescription.Limit.Ante);

                                    simulatedGameState.PlayNewHand();
                                    #region Settings up infos for preflop                                    
                                    // Verify if the two players has >= 90BB stacks before noting preflop hand
                                    bool noteHandPreflop = ((decimal.Divide(parsedHand.Players[0].StartingStack, parsedHand.GameDescription.Limit.BigBlind) >= 90) &&
                                                            (decimal.Divide(parsedHand.Players[1].StartingStack, parsedHand.GameDescription.Limit.BigBlind) >= 90) &&
                                                            parsedHand.Players[_playerIndex].hasHoleCards);

                                    List<HandAction> lstFilteredActions = new List<HandAction>(parsedHand.HandActions.Count);
                                    foreach (HandAction currentHandAction in parsedHand.HandActions)
                                    {
                                        switch (currentHandAction.HandActionType)
                                        {
                                            case HandActionType.CALL:
                                            case HandActionType.RAISE:
                                            case HandActionType.CHECK:
                                            case HandActionType.BET:
                                            case HandActionType.FOLD:
                                                lstFilteredActions.Add(currentHandAction);
                                                break;
                                        }
                                    }

                                    PossiblePositions playerPosition = PossiblePositions.Unknown;
                                    PossiblePositions villainPosition = PossiblePositions.Unknown;

                                    if (isButton)
                                    {
                                        playerPosition = PossiblePositions.BTN;
                                        villainPosition = PossiblePositions.BB;
                                    }
                                    else
                                    {
                                        playerPosition = PossiblePositions.BB;
                                        villainPosition = PossiblePositions.BTN;
                                    }

                                    #endregion

                                    int currentIndexAction = 0;

                                    HandAction currentAction = null;
                                    if (lstFilteredActions.Count > 0)
                                        currentAction = lstFilteredActions[currentIndexAction];

                                    CFlopGameState lastFlopGameStateFromHero = null;
                                    CFlopGameState lastFlopGameStateFromVillain = null;
                                    CTurnGameState lastTurnGameStateFromHero = null;
                                    CTurnGameState lastTurnGameStateFromVillain = null;

                                    #region Note flop    
                                    decimal potAfterRaise = 0m;
                                    decimal lastRaiseAmount = 0;
                                    decimal nbRaisePreflop = 0;

                                    // Get only actions that are on the flop
                                    while (currentAction != null && currentAction.Street != Street.Flop)
                                    {
                                        if (currentAction.Street == Street.Preflop)
                                        {
                                            decimal amount = Math.Abs(currentAction.Amount);

                                            if (currentAction.HandActionType == HandActionType.RAISE)
                                            {
                                                ++nbRaisePreflop;
                                                lastRaiseAmount = currentAction.Amount;
                                            }

                                            switch (currentAction.HandActionType)
                                            {
                                                case HandActionType.BET:
                                                    throw new Exception("Cannot bet in preflop stage!");
                                                case HandActionType.RAISE:
                                                    simulatedGameState.Raise(amount);
                                                    potAfterRaise = simulatedGameState.PPot;
                                                    break;
                                                case HandActionType.CALL:
                                                    simulatedGameState.Call();
                                                    break;
                                                case HandActionType.CHECK:
                                                    simulatedGameState.Check();
                                                    break;
                                                case HandActionType.FOLD:
                                                    throw new Exception("Cannot analyze a hand that was folded before the flop!");
                                                case HandActionType.UNCALLED_BET:
                                                    break;
                                                default:
                                                    throw new Exception("Never suppose to happen to be here!");
                                            }

                                            if (currentAction.PlayerName == currentPlayer.PlayerName)
                                            {
                                                if (noteHandPreflop)
                                                {
                                                    TypesPot typePotPreflop = simulatedGameState.LoadAndGetTypePot();

                                                    switch (typePotPreflop)
                                                    {
                                                        case TypesPot.Limped:
                                                            noteHandPreflop = true;
                                                            break;
                                                        case TypesPot.RaisedLimped:
                                                            noteHandPreflop = (lastRaiseAmount <= (parsedHand.GameDescription.Limit.BigBlind * 6));
                                                            break;
                                                        case TypesPot.LimpedThreeBet:
                                                            decimal potBeforeRaise = (potAfterRaise - lastRaiseAmount);
                                                            noteHandPreflop = ((potAfterRaise >= (potBeforeRaise * 2m)) && (potAfterRaise <= (potBeforeRaise + (potBeforeRaise * 3.4m))));
                                                            break;
                                                        case TypesPot.LimpedFourBetEtPlus:
                                                            noteHandPreflop = false;
                                                            break;
                                                        case TypesPot.TwoBet:
                                                            noteHandPreflop = (lastRaiseAmount <= (parsedHand.GameDescription.Limit.BigBlind * 4));
                                                            break;
                                                        case TypesPot.ThreeBet:
                                                            decimal potThreeBetBeforeRaise = (potAfterRaise - lastRaiseAmount); // Includes both situations: If the player is calling or if the player is 3betting
                                                            noteHandPreflop = ((potAfterRaise >= (potThreeBetBeforeRaise * 3)) && (potAfterRaise <= (potThreeBetBeforeRaise + (potThreeBetBeforeRaise * 4m))));
                                                            break;
                                                        case TypesPot.FourBet:
                                                            decimal potFourBetBeforeRaise = (potAfterRaise - lastRaiseAmount);
                                                            noteHandPreflop = ((potAfterRaise >= (potFourBetBeforeRaise * 2m)) && (potAfterRaise <= (potFourBetBeforeRaise + (potFourBetBeforeRaise * 3m))));
                                                            break;
                                                        case TypesPot.FiveBetEtPlus:
                                                            noteHandPreflop = (currentAction.IsAllIn);
                                                            break;
                                                        default:
                                                            throw new InvalidOperationException("Invalid pot type");
                                                    }

                                                    if (noteHandPreflop)
                                                        lstAvgPlayerPreflopRange.Add(new CAveragePlayerPreflopRange(typePotPreflop, playerPosition, handConvertedRiver.PocketMask, handConvertedRiver.PocketCards, 1));
                                                }
                                            }

                                            if (++currentIndexAction < lstFilteredActions.Count)
                                                currentAction = lstFilteredActions[currentIndexAction];
                                            else
                                                currentAction = null;
                                        }
                                    }

                                    TypesPot typePotFiltered = simulatedGameState.PTypeFilteredPot;

                                    int lastIndexPreflopAction = (currentIndexAction - 1);
                                    long? GetFilteredTypeBetFromAction(ActionsPossible _action)
                                    {
                                        switch (_action)
                                        {
                                            case ActionsPossible.Bet:
                                                return (long?)simulatedGameState.GetClosestBetSizeFromLastAction();
                                            case ActionsPossible.Call:
                                                return (long?)simulatedGameState.GetClosestBetSizeFromLastCallAction();
                                            case ActionsPossible.Raise:
                                                switch (typePotFiltered)
                                                {
                                                    case TypesPot.TwoBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastActionTwoBetPot();
                                                    case TypesPot.ThreeBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastActionThreeBetPot();
                                                    case TypesPot.FourBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastActionFourBetPot();
                                                    default:
                                                        throw new InvalidOperationException("Invalid type pot");
                                                }
                                            case ActionsPossible.CallVsRaise:
                                                switch (typePotFiltered)
                                                {
                                                    case TypesPot.TwoBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastCallActionTwoBetPot();
                                                    case TypesPot.ThreeBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastCallActionThreeBetPot();
                                                    case TypesPot.FourBet:
                                                        return (long?)simulatedGameState.GetClosestRaiseSizeFromLastCallActionFourBetPot();
                                                    default:
                                                        throw new InvalidOperationException("Invalid type pot");
                                                }
                                            case ActionsPossible.CallVsReRaise:
                                                return (long?)simulatedGameState.GetClosestReRaiseCallSizeFromLastAction();
                                            default:
                                                return null;
                                        }
                                    }

                                    while ((currentAction != null) && (currentAction.Street == Street.Flop) && !simulatedGameState.PHandFinished)
                                    {
                                        bool justFolded = false;
                                        bool? couldHaveRaise = null;
                                        bool? couldHaveChecked = simulatedGameState.GetLstAllowedActionsForCurrentPlayer().Contains(ActionsPossible.Check);

                                        switch (currentAction.HandActionType)
                                        {
                                            case HandActionType.BET:
                                                simulatedGameState.Bet(currentAction.Amount);
                                                break;
                                            case HandActionType.RAISE:
                                                simulatedGameState.Raise(currentAction.Amount);
                                                break;
                                            case HandActionType.CALL:
                                                simulatedGameState.Call();
                                                break;
                                            case HandActionType.CHECK:
                                                simulatedGameState.Check();
                                                break;
                                            case HandActionType.FOLD:
                                                var lstAllowedActions = simulatedGameState.GetLstAllowedActionsForCurrentPlayer();
                                                couldHaveRaise = lstAllowedActions.Contains(ActionsPossible.Raise);
                                                justFolded = true;
                                                break;
                                            default:
                                                throw new Exception("Unable to detect the action!");
                                        }

                                        bool foldedWhenHeCouldOfChecked = (justFolded && (bool)couldHaveChecked);

                                        if ((lstFilteredActions[currentIndexAction].PlayerName == currentPlayer.PlayerName) && (!foldedWhenHeCouldOfChecked))
                                        {
                                            CAction actionConverted = simulatedGameState.GetFilteredActionFromLastAction();
                                            long? typeBet = GetFilteredTypeBetFromAction(actionConverted.PAction);

                                            CFlopGameState flopGameStateGivenTheAction = null;

                                            if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, playerPosition, actionConverted.PAction, null)];
                                            else
                                                flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, playerPosition, actionConverted.PAction, typeBet)];

                                            // Getting the flop game state associated to the current action
                                            if (flopGameStateGivenTheAction != null)
                                            {
                                                ushort boardType = (ushort)CBoardModel.CalculateMetaData(handFlopBoardOnly.BoardMask);
                                                double boardHeat = 0;

                                                if (justFolded)
                                                {
                                                    if (!(bool)couldHaveChecked)
                                                    {
                                                        if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                            flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, villainPosition, actionConverted.PAction, null)];
                                                        else
                                                            flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, villainPosition, actionConverted.PAction, typeBet)];

                                                        var flopFoldStat = new CFlopFoldStats(flopGameStateGivenTheAction, boardType, boardHeat, (bool)couldHaveRaise, 0);

                                                        lstFlopFoldsStats.Add(flopFoldStat);
                                                    }

                                                    simulatedGameState.Fold(); // PHandFinished will be triggered to true, and it will be used later.
                                                }
                                                else
                                                {
                                                    var flopOtherStat = new CFlopOtherStats(flopGameStateGivenTheAction, boardType, 0, 0);

                                                    lstFlopOtherStats.Add(flopOtherStat);

                                                    if (hasHoleCards)
                                                    {
                                                        // If we have the equivalent of a high card (so if we have a bluff)
                                                        if (Hand.GetHandTypeExcludingBoard(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask) == Hand.HandTypes.HighCard)
                                                        {
                                                            #region Bluff hand
                                                            // Insert the flop state to the "to-do" list                                                    
                                                            bool isBDFD = Hand.IsBackdoorFlushDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, 0L);
                                                            bool isBDSD = Hand.IsBackDoorStraightDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask);
                                                            bool isSD = Hand.IsStraightDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, 0L);
                                                            bool isFD = Hand.IsFlushDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, 0L);
                                                            sbyte highestIndex = (sbyte)CBotPokerAmigo.CardIndex(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask);

                                                            decimal uc = GetUnifiedCountForBluffs(isBDFD, isBDSD, isFD, isSD, highestIndex, handConvertedFlop.BoardMask);
                                                            var infos = new CAveragePlayerBluffsFlop(flopGameStateGivenTheAction, boardType, boardHeat, isBDFD, isBDSD, isSD, isFD, highestIndex, uc, 1);
                                                            lstAvgPlayerBluffsFlop.Add(infos);
                                                            lstDebugFlop.Add(new Tuple<CAveragePlayerBluffsFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region Made hand
                                                            double handStrength = Hand.HandStrength(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask);

                                                            if (handStrength >= 0.9)
                                                            {
                                                                double lowerIntervalHS = 0;
                                                                decimal uc = GetUnifiedCountForHS(handStrength, handConvertedFlop.BoardMask, out lowerIntervalHS);

                                                                CAveragePlayerValueHandsFlop newInfos = new CAveragePlayerValueHandsFlop(flopGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);
                                                                lstAvgPlayerMadeHandValueHandFlop.Add(newInfos);
                                                                lstDebugFlop6.Add(new Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>(newInfos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                            }
                                                            else
                                                            {
                                                                sbyte nbOuts = (sbyte)Hand.OutsDiscounted(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, new ulong[0]);

                                                                if (nbOuts >= 10)
                                                                {
                                                                    decimal uc = GetUnifiedCountForOuts(nbOuts, handConvertedFlop.BoardMask);
                                                                    var infos = new CAveragePlayerBluffsWithLotsOfEquityFlop(flopGameStateGivenTheAction, boardType, boardHeat, nbOuts, uc, 1);
                                                                    lstAvgPlayerLotsEquityFlop.Add(infos);
                                                                    lstDebugFlop2.Add(new Tuple<CAveragePlayerBluffsWithLotsOfEquityFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                }
                                                                else if (handStrength >= 0.8)
                                                                {
                                                                    double lowerIntervalHS = 0;
                                                                    decimal uc = GetUnifiedCountForHS(handStrength, handConvertedFlop.BoardMask, out lowerIntervalHS);
                                                                    var infos = new CAveragePlayerValueHandsFlop(flopGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                    lstAvgPlayerMadeHandValueHandFlop.Add(infos);
                                                                    lstDebugFlop6.Add(new Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                }
                                                                else
                                                                {
                                                                    bool isFlushDraw = Hand.IsFlushDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, 0L);
                                                                    bool isStraightDraw = Hand.IsStraightDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, 0L);

                                                                    if (isFlushDraw && isStraightDraw)
                                                                    {
                                                                        var uc = GetUnifiedCountForMadeHandSDFD(handConvertedFlop.BoardMask);
                                                                        var infos = new CAveragePlayerMadeHandSDAndFDFlop(flopGameStateGivenTheAction, boardType, boardHeat, uc, 1);
                                                                        lstAvgPlayerMadeHandSDAndFDFlop.Add(infos);
                                                                        lstDebugFlop4.Add(new Tuple<CAveragePlayerMadeHandSDAndFDFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else if (isFlushDraw)
                                                                    {
                                                                        sbyte indexHighestFlushDraw = (sbyte)CBotPokerAmigo.GetIndexHighestFlushDraw(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask);
                                                                        var uc = GetUnifiedCountForMadeHandFD(handConvertedFlop.BoardMask);
                                                                        var infos = new CAveragePlayerMadeHandFDFlop(flopGameStateGivenTheAction, boardType, boardHeat, indexHighestFlushDraw, uc, 1);
                                                                        lstAvgPlayerMadeHandFDFlop.Add(infos);
                                                                        lstDebugFlop3.Add(new Tuple<CAveragePlayerMadeHandFDFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else if (isStraightDraw)
                                                                    {
                                                                        var uc = GetUnifiedCountForMadeHandSD(handConvertedFlop.BoardMask);
                                                                        var infos = new CAveragePlayerMadeHandSDFlop(flopGameStateGivenTheAction, boardType, boardHeat, uc, 1);
                                                                        lstAvgPlayerMadeHandSDFlop.Add(infos);
                                                                        lstDebugFlop5.Add(new Tuple<CAveragePlayerMadeHandSDFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else
                                                                    {
                                                                        double lowerIntervalHS = 0;
                                                                        decimal uc = GetUnifiedCountForHS(handStrength, handConvertedFlop.BoardMask, out lowerIntervalHS);
                                                                        var infos = new CAveragePlayerValueHandsFlop(flopGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                        lstAvgPlayerMadeHandValueHandFlop.Add(infos);
                                                                        lstDebugFlop6.Add(new Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedFlop.PocketMask, handConvertedFlop.BoardMask, handConvertedFlop.PocketCards, handConvertedFlop.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                    }

                                                }
                                            }
                                            else
                                                throw new Exception("Unable to retrieve information");

                                            lastFlopGameStateFromHero = flopGameStateGivenTheAction;
                                        }
                                        else if (justFolded)
                                            simulatedGameState.Fold();
                                        else
                                        {
                                            CAction actionConverted = simulatedGameState.GetFilteredActionFromLastAction();

                                            long? typeBet = GetFilteredTypeBetFromAction(actionConverted.PAction);

                                            CFlopGameState flopGameStateGivenTheAction = null;

                                            if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, villainPosition, actionConverted.PAction, null)];
                                            else
                                                flopGameStateGivenTheAction = CDBHelper.PDicAllFlopGameStatesByInfos[(typePotFiltered, villainPosition, actionConverted.PAction, typeBet)];

                                            lastFlopGameStateFromVillain = flopGameStateGivenTheAction;
                                        }

                                        if (++currentIndexAction < lstFilteredActions.Count)
                                            currentAction = lstFilteredActions[currentIndexAction];
                                        else
                                            currentAction = null;
                                    }
                                    #endregion

                                    void NoteTurn()
                                    {
                                        #region Note turn
                                        // Get only actions that are on the turn
                                        while ((currentAction != null) && (currentAction.Street != Street.Turn))
                                        {
                                            if (++currentIndexAction < lstFilteredActions.Count)
                                                currentAction = lstFilteredActions[currentIndexAction];
                                            else
                                                currentAction = null;
                                        }

                                        while ((currentAction != null) && (currentAction.Street == Street.Turn) && !simulatedGameState.PHandFinished)
                                        {
                                            bool justFolded = false;
                                            bool? couldHaveRaise = null;
                                            bool? couldHaveChecked = simulatedGameState.GetLstAllowedActionsForCurrentPlayer().Contains(ActionsPossible.Check);

                                            switch (currentAction.HandActionType)
                                            {
                                                case HandActionType.BET:
                                                    simulatedGameState.Bet(currentAction.Amount);
                                                    break;
                                                case HandActionType.RAISE:
                                                    simulatedGameState.Raise(currentAction.Amount);
                                                    break;
                                                case HandActionType.CALL:
                                                    simulatedGameState.Call();
                                                    break;
                                                case HandActionType.CHECK:
                                                    simulatedGameState.Check();
                                                    break;
                                                case HandActionType.FOLD:
                                                    couldHaveRaise = simulatedGameState.GetLstAllowedActionsForCurrentPlayer().Contains(ActionsPossible.Raise);
                                                    justFolded = true;
                                                    break;
                                                default:
                                                    throw new Exception("Unable to detect the action!");
                                            }

                                            bool foldedWhenHeCouldOfChecked = (justFolded && (bool)couldHaveChecked);

                                            if ((lstFilteredActions[currentIndexAction].PlayerName == currentPlayer.PlayerName) && (!foldedWhenHeCouldOfChecked))
                                            {
                                                CAction actionConverted = simulatedGameState.GetFilteredActionFromLastAction();
                                                long? typeBet = GetFilteredTypeBetFromAction(actionConverted.PAction);

                                                // Getting the turn game state associated to the current action
                                                CTurnGameState turnGameStateGivenTheAction = null;

                                                if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                    turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromHero, actionConverted.PAction, null)];
                                                else
                                                    turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromHero, actionConverted.PAction, typeBet)];

                                                if (turnGameStateGivenTheAction != null)
                                                {
                                                    ushort boardType = (ushort)CBoardModel.CalculateMetaData(handTurnBoardOnly.BoardMask);
                                                    double boardHeat = 0;

                                                    if (justFolded)
                                                    {
                                                        if (!(bool)couldHaveChecked)
                                                        {
                                                            if (lastFlopGameStateFromVillain != null)
                                                            {
                                                                if (!lastFlopGameStateFromVillain.PTypeBet.HasValue || (lastFlopGameStateFromVillain.PTypeBet.HasValue && lastFlopGameStateFromVillain.PTypeBet.Value != 135 && lastFlopGameStateFromVillain.PTypeBet.Value != 136))
                                                                {
                                                                    if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                                        turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromVillain, actionConverted.PAction, null)];
                                                                    else
                                                                        turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromVillain, actionConverted.PAction, typeBet)];

                                                                    // var flopFoldStat = new CFlopFoldStats(flopGameStateGivenTheAction, boardType, boardHeat, (bool)couldHaveRaise, 0);
                                                                    var turnFoldStat = new CTurnFoldStats(turnGameStateGivenTheAction, boardType, boardHeat, (bool)couldHaveRaise, 0);

                                                                    lstTurnFoldsStats.Add(turnFoldStat);
                                                                }
                                                            }
                                                        }

                                                        simulatedGameState.Fold(); // PHandFinished will be triggered to true, and it will be used later.
                                                    }
                                                    else
                                                    {
                                                        var turnOtherStat = new CTurnOtherStats(turnGameStateGivenTheAction, boardType, 0, 0);

                                                        lstTurnOtherStats.Add(turnOtherStat);

                                                        if (hasHoleCards)
                                                        {
                                                            // If we have the equivalent of a high card (so if we have a bluff)
                                                            if (Hand.GetHandTypeExcludingBoard(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask) == Hand.HandTypes.HighCard)
                                                            {
                                                                #region Bluff hand
                                                                // Insert the Turn state to the "to-do" list                                                    
                                                                bool isSD = Hand.IsStraightDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);
                                                                bool isFD = Hand.IsFlushDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);
                                                                sbyte highestIndex = (sbyte)CBotPokerAmigo.CardIndex(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask);

                                                                decimal uc = GetUnifiedCountForBluffsTurn(isFD, isSD, highestIndex, handConvertedTurn.BoardMask);
                                                                var infos = new CAveragePlayerBluffsTurn(turnGameStateGivenTheAction, boardType, boardHeat, isSD, isFD, highestIndex, uc, 1);

                                                                lstAvgPlayerBluffsTurn.Add(infos);
                                                                lstDebugTurn.Add(new Tuple<CAveragePlayerBluffsTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                #endregion
                                                            }
                                                            else
                                                            {
                                                                #region Made hand
                                                                double handStrength = Hand.HandStrength(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask);

                                                                if (handStrength >= 0.9)
                                                                {
                                                                    double lowerIntervalHS = 0;
                                                                    decimal uc = GetUnifiedCountForHS(handStrength, handConvertedTurn.BoardMask, out lowerIntervalHS);
                                                                    var infos = new CAveragePlayerValueHandsTurn(turnGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                    lstAvgPlayerMadeHandValueHandTurn.Add(infos);
                                                                    lstDebugTurn6.Add(new Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                }
                                                                else
                                                                {
                                                                    sbyte nbOuts = (sbyte)Hand.OutsDiscounted(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, new ulong[0]);

                                                                    if (nbOuts >= 10)
                                                                    {
                                                                        decimal uc = GetUnifiedCountForOuts(nbOuts, handConvertedTurn.BoardMask);
                                                                        var infos = new CAveragePlayerBluffsWithLotsOfEquityTurn(turnGameStateGivenTheAction, boardType, boardHeat, nbOuts, uc, 1);
                                                                        lstAvgPlayerLotsEquityTurn.Add(infos);
                                                                        lstDebugTurn2.Add(new Tuple<CAveragePlayerBluffsWithLotsOfEquityTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else if (handStrength >= 0.8)
                                                                    {
                                                                        double lowerIntervalHS = 0;
                                                                        decimal uc = GetUnifiedCountForHS(handStrength, handConvertedTurn.BoardMask, out lowerIntervalHS);
                                                                        var infos = new CAveragePlayerValueHandsTurn(turnGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                        lstAvgPlayerMadeHandValueHandTurn.Add(infos);
                                                                        lstDebugTurn6.Add(new Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else
                                                                    {
                                                                        bool isFlushDraw = Hand.IsFlushDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);
                                                                        bool isStraightDraw = Hand.IsStraightDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);

                                                                        if (isFlushDraw && isStraightDraw)
                                                                        {
                                                                            var uc = GetUnifiedCountForMadeHandSDFD(handConvertedTurn.BoardMask);
                                                                            var infos = new CAveragePlayerMadeHandSDAndFDTurn(turnGameStateGivenTheAction, boardType, boardHeat, uc, 1);

                                                                            lstAvgPlayerMadeHandSDAndFDTurn.Add(infos);
                                                                            lstDebugTurn4.Add(new Tuple<CAveragePlayerMadeHandSDAndFDTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                        }
                                                                        else if (isFlushDraw)
                                                                        {
                                                                            sbyte indexHighestFlushDraw = (sbyte)CBotPokerAmigo.GetIndexHighestFlushDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask);
                                                                            var uc = GetUnifiedCountForMadeHandFD(handConvertedTurn.BoardMask);
                                                                            var infos = new CAveragePlayerMadeHandFDTurn(turnGameStateGivenTheAction, boardType, boardHeat, indexHighestFlushDraw, uc, 1);

                                                                            lstAvgPlayerMadeHandFDTurn.Add(infos);
                                                                            lstDebugTurn3.Add(new Tuple<CAveragePlayerMadeHandFDTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                        }
                                                                        else if (isStraightDraw)
                                                                        {
                                                                            var uc = GetUnifiedCountForMadeHandSD(handConvertedTurn.BoardMask);
                                                                            var infos = new CAveragePlayerMadeHandSDTurn(turnGameStateGivenTheAction, boardType, boardHeat, uc, 1);

                                                                            lstAvgPlayerMadeHandSDTurn.Add(infos);
                                                                            lstDebugTurn5.Add(new Tuple<CAveragePlayerMadeHandSDTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                        }
                                                                        else
                                                                        {
                                                                            double lowerIntervalHS = 0;
                                                                            decimal uc = GetUnifiedCountForHS(handStrength, handConvertedTurn.BoardMask, out lowerIntervalHS);
                                                                            var infos = new CAveragePlayerValueHandsTurn(turnGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                            lstAvgPlayerMadeHandValueHandTurn.Add(infos);
                                                                            lstDebugTurn6.Add(new Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, handConvertedTurn.PocketCards, handConvertedTurn.Board, parsedHand.FullHandHistoryText)));
                                                                        }
                                                                    }
                                                                }
                                                                #endregion
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                    throw new Exception("Cannot get turn game state!");

                                                lastTurnGameStateFromHero = turnGameStateGivenTheAction;
                                            }
                                            else if (justFolded)
                                                simulatedGameState.Fold();
                                            else
                                            {
                                                CAction actionConverted = simulatedGameState.GetFilteredActionFromLastAction();
                                                long? typeBet = GetFilteredTypeBetFromAction(actionConverted.PAction);

                                                CTurnGameState turnGameStateGivenTheAction = null;

                                                if (lastFlopGameStateFromVillain != null)
                                                {
                                                    if (!lastFlopGameStateFromVillain.PTypeBet.HasValue || (lastFlopGameStateFromVillain.PTypeBet.HasValue && lastFlopGameStateFromVillain.PTypeBet.Value != 135 && lastFlopGameStateFromVillain.PTypeBet.Value != 136))
                                                    {
                                                        if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                            turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromVillain, actionConverted.PAction, null)];
                                                        else
                                                            turnGameStateGivenTheAction = CDBHelper.PDicAllTurnGameStatesByInfos[(lastFlopGameStateFromVillain, actionConverted.PAction, typeBet)];

                                                        lastTurnGameStateFromVillain = turnGameStateGivenTheAction;
                                                    }
                                                }
                                            }

                                            if (++currentIndexAction < lstFilteredActions.Count)
                                                currentAction = lstFilteredActions[currentIndexAction];
                                            else
                                                currentAction = null;
                                        }
                                        #endregion
                                    }
                                    void NoteRiver()
                                    {
                                        #region Note river
                                        // Get only actions that are on the river
                                        while ((currentAction != null) && (currentAction.Street != Street.River))
                                        {
                                            if (++currentIndexAction < lstFilteredActions.Count)
                                                currentAction = lstFilteredActions[currentIndexAction];
                                            else
                                                currentAction = null;
                                        }

                                        while ((currentAction != null) && (currentAction.Street == Street.River) && !simulatedGameState.PHandFinished)
                                        {
                                            bool justFolded = false;
                                            bool? couldHaveRaise = null;
                                            bool? couldHaveChecked = simulatedGameState.GetLstAllowedActionsForCurrentPlayer().Contains(ActionsPossible.Check);

                                            switch (currentAction.HandActionType)
                                            {
                                                case HandActionType.BET:
                                                    simulatedGameState.Bet(currentAction.Amount);
                                                    break;
                                                case HandActionType.RAISE:
                                                    simulatedGameState.Raise(currentAction.Amount);
                                                    break;
                                                case HandActionType.CALL:
                                                    simulatedGameState.Call();
                                                    break;
                                                case HandActionType.CHECK:
                                                    simulatedGameState.Check();
                                                    break;
                                                case HandActionType.FOLD:
                                                    couldHaveRaise = simulatedGameState.GetLstAllowedActionsForCurrentPlayer().Contains(ActionsPossible.Raise);
                                                    justFolded = true;
                                                    break;
                                                default:
                                                    throw new Exception("Unable to detect the action!");
                                            }

                                            bool foldedWhenHeCouldOfChecked = (justFolded && (bool)couldHaveChecked);

                                            if ((lstFilteredActions[currentIndexAction].PlayerName == currentPlayer.PlayerName) && (!foldedWhenHeCouldOfChecked))
                                            {
                                                CAction actionConverted = simulatedGameState.GetFilteredActionFromLastAction();
                                                long? typeBet = GetFilteredTypeBetFromAction(actionConverted.PAction);

                                                // Getting the river game state associated to the current action
                                                CRiverGameState riverGameStateGivenTheAction = null;

                                                if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                    riverGameStateGivenTheAction = CDBHelper.PDicAllRiverGameStatesByInfos[(lastTurnGameStateFromHero, actionConverted.PAction, null)];
                                                else
                                                    riverGameStateGivenTheAction = CDBHelper.PDicAllRiverGameStatesByInfos[(lastTurnGameStateFromHero, actionConverted.PAction, typeBet)];

                                                if (riverGameStateGivenTheAction != null)
                                                {
                                                    BoardMetaDataFlags boardTypeFlags = CalculateMetaData(handRiverBoardOnly.BoardMask);
                                                    ushort boardType = (ushort)boardTypeFlags;
                                                    double boardHeat = 0;

                                                    if (justFolded)
                                                    {
                                                        if (!(bool)couldHaveChecked)
                                                        {
                                                            if (lastFlopGameStateFromVillain != null)
                                                            {
                                                                if (!lastFlopGameStateFromVillain.PTypeBet.HasValue || (lastFlopGameStateFromVillain.PTypeBet.HasValue && lastFlopGameStateFromVillain.PTypeBet.Value != 135 && lastFlopGameStateFromVillain.PTypeBet.Value != 136))
                                                                {
                                                                    if (lastTurnGameStateFromVillain != null)
                                                                    {
                                                                        if (!lastTurnGameStateFromVillain.PTypeBet.HasValue || (lastTurnGameStateFromVillain.PTypeBet.HasValue && lastTurnGameStateFromVillain.PTypeBet.Value != 135 && lastTurnGameStateFromVillain.PTypeBet.Value != 136))
                                                                        {
                                                                            if (actionConverted.PAction == ActionsPossible.Check || actionConverted.PAction == ActionsPossible.ReRaise)
                                                                                riverGameStateGivenTheAction = CDBHelper.PDicAllRiverGameStatesByInfos[(lastTurnGameStateFromVillain, actionConverted.PAction, null)];
                                                                            else
                                                                                riverGameStateGivenTheAction = CDBHelper.PDicAllRiverGameStatesByInfos[(lastTurnGameStateFromVillain, actionConverted.PAction, typeBet)];

                                                                            var riverFoldStat = new CRiverFoldStats(riverGameStateGivenTheAction, boardType, boardHeat, (bool)couldHaveRaise, 0);

                                                                            lstRiverFoldsStats.Add(riverFoldStat);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        simulatedGameState.Fold(); // PHandFinished will be triggered to true, and it will be used later.
                                                    }
                                                    else
                                                    {
                                                        var riverOtherStat = new CRiverOtherStats(riverGameStateGivenTheAction, boardType, 0, 0);

                                                        lstRiverOtherStats.Add(riverOtherStat);

                                                        if (hasHoleCards)
                                                        {
                                                            // If it's the equivalent of a high card
                                                            if (Hand.GetHandTypeExcludingBoard(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask) == Hand.HandTypes.HighCard)
                                                            {
                                                                #region Bluff hand
                                                                // Insert the river state to the "to-do" list. Inserts the straight draws and flush draws that was there on the turn/flop, that missed on the river.                                                    
                                                                bool isSD = Hand.IsStraightDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);
                                                                bool isFD = Hand.IsFlushDraw(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, 0L);
                                                                sbyte highestIndex = (sbyte)CBotPokerAmigo.CardIndex(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask);

                                                                decimal uc = GetUnifiedCountForBluffsRiver(isFD, isSD, highestIndex, handConvertedTurn.BoardMask, handConvertedRiver.BoardMask);
                                                                var infos = new CAveragePlayerBluffsRiver(riverGameStateGivenTheAction, boardType, boardHeat, isSD, isFD, highestIndex, uc, 1);

                                                                lstAvgPlayerBluffsRiver.Add(infos);
                                                                lstDebugRiver.Add(new Tuple<CAveragePlayerBluffsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                #endregion
                                                            }
                                                            else
                                                            {
                                                                #region Made hand
                                                                sbyte nbOutsOnTurn = (sbyte)Hand.OutsDiscounted(handConvertedTurn.PocketMask, handConvertedTurn.BoardMask, new ulong[0]);
                                                                double handStrength = Hand.HandStrength(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask);

                                                                if (nbOutsOnTurn >= 10)
                                                                {
                                                                    if (handStrength >= 0.9)
                                                                    {
                                                                        double lowerIntervalHS = 0;
                                                                        decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                        var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                        lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                        lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                    else
                                                                    {
                                                                        decimal uc = GetUnifiedCountForOutsRiver(nbOutsOnTurn, handConvertedTurn.BoardMask, handConvertedRiver.BoardMask);
                                                                        // Most likely a bluff with lots of equity (on the turn) hand, that he did not gave up river
                                                                        var infos = new CAveragePlayerBluffsWithLotsOfEquityRiver(riverGameStateGivenTheAction, boardType, boardHeat, nbOutsOnTurn, uc, 1);

                                                                        lstAvgPlayerLotsEquityRiver.Add(infos);
                                                                        lstDebugRiver2.Add(new Tuple<CAveragePlayerBluffsWithLotsOfEquityRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                    }
                                                                }
                                                                else if (handStrength >= 0.8)
                                                                {
                                                                    double lowerIntervalHS = 0;
                                                                    decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                    var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                    lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                    lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                }
                                                                else
                                                                {
                                                                    if (CBotPokerAmigo.IsNutHandHigherThanTrips(handConvertedRiver.BoardMask))
                                                                    {
                                                                        if (Hand.GetHandTypeExcludingBoard(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask) < Hand.HandTypes.Flush)
                                                                        {
                                                                            bool isTwoPair = Convert.ToBoolean((boardTypeFlags & BoardMetaDataFlags.TwoPaired));
                                                                            bool isTrips = Convert.ToBoolean((boardTypeFlags & BoardMetaDataFlags.Trips));
                                                                            bool isFullHouse = Convert.ToBoolean((boardTypeFlags & BoardMetaDataFlags.FullHouse));
                                                                            bool isQuads = Convert.ToBoolean((boardTypeFlags & BoardMetaDataFlags.Quads));
                                                                            bool isStraightFlush = Convert.ToBoolean((boardTypeFlags & BoardMetaDataFlags.StraightFlushComplete));

                                                                            if (!isTwoPair && !isTrips && !isFullHouse && !isQuads && !isStraightFlush)
                                                                            {
                                                                                List<Tuple<ulong, double>> lstBlockerRange = CBotPokerAmigo.GetBlockerRangeFromPocket(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask);

                                                                                if (lstBlockerRange.Count > 0)
                                                                                {
                                                                                    double blockerRatio = lstBlockerRange[0].Item2;

                                                                                    if (blockerRatio > 0 && blockerRatio < 1)
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            double handRatioInBlockerRange = CBotPokerAmigo.GetHandRatioFromBlockerRange(lstBlockerRange, handConvertedRiver.PocketMask);

                                                                                            if (handRatioInBlockerRange >= 0 && handRatioInBlockerRange <= 1)
                                                                                            {
                                                                                                var infos = new CAveragePlayerMadeHandBlockerRiver(riverGameStateGivenTheAction, boardType, boardHeat, blockerRatio, handRatioInBlockerRange, 1);

                                                                                                lstAvgPlayerMadeHandBlockerRiver.Add(infos);
                                                                                                lstDebugRiver3.Add(new Tuple<CAveragePlayerMadeHandBlockerRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                double lowerIntervalHS = 0;
                                                                                                decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                                                var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                                                lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                                                lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                                            }
                                                                                        }
                                                                                        catch (Exception)
                                                                                        {
                                                                                            Console.WriteLine("fucking board: " + handConvertedRiver.Board + " fucking cards: " + handConvertedRiver.PocketCards);
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        double lowerIntervalHS = 0;
                                                                                        decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                                        var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                                        lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                                        lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    double lowerIntervalHS = 0;
                                                                                    decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                                    var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                                    lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                                    lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                double lowerIntervalHS = 0;
                                                                                decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                                var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                                lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                                lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            double lowerIntervalHS = 0;
                                                                            decimal uc = GetUnifiedCountForHS(handStrength, handConvertedRiver.BoardMask, out lowerIntervalHS);
                                                                            var infos = new CAveragePlayerValueHandsRiver(riverGameStateGivenTheAction, boardType, boardHeat, lowerIntervalHS, uc, 1);

                                                                            lstAvgPlayerMadeHandValueHandRiver.Add(infos);
                                                                            lstDebugRiver4.Add(new Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>(infos, new CDebugGeneralHandInfos(handConvertedRiver.PocketMask, handConvertedRiver.BoardMask, handConvertedRiver.PocketCards, handConvertedRiver.Board, parsedHand.FullHandHistoryText)));
                                                                        }
                                                                    }
                                                                }
                                                                #endregion
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                    throw new Exception("Cannot retrieve river game state!");
                                            }
                                            else if (justFolded)
                                                simulatedGameState.Fold();

                                            if (++currentIndexAction < lstFilteredActions.Count)
                                                currentAction = lstFilteredActions[currentIndexAction];
                                            else
                                                currentAction = null;
                                        }
                                        #endregion
                                    }

                                    if (!simulatedGameState.PHandFinished && (currentIndexAction < lstFilteredActions.Count))
                                    {
                                        if (lastFlopGameStateFromHero.PTypeBet.HasValue)
                                        {
                                            if (lastFlopGameStateFromHero.PTypeBet.Value != 135 && lastFlopGameStateFromHero.PTypeBet.Value != 136)
                                            {
                                                NoteTurn();

                                                if (!simulatedGameState.PHandFinished && (currentIndexAction < lstFilteredActions.Count))
                                                {
                                                    if (lastTurnGameStateFromHero.PTypeBet.HasValue)
                                                    {
                                                        if (lastTurnGameStateFromHero.PTypeBet.Value != 135 && lastTurnGameStateFromHero.PTypeBet.Value != 136)
                                                            NoteRiver();
                                                    }
                                                    else
                                                        NoteRiver();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            NoteTurn();

                                            if (!simulatedGameState.PHandFinished && (currentIndexAction < lstFilteredActions.Count))
                                            {
                                                if (lastTurnGameStateFromHero.PTypeBet.HasValue)
                                                {
                                                    if (lastTurnGameStateFromHero.PTypeBet.Value != 135 && lastTurnGameStateFromHero.PTypeBet.Value != 136)
                                                        NoteRiver();
                                                }
                                                else
                                                    NoteRiver();
                                            }
                                        }
                                    }

                                    #endregion
                                }

                                if (parsedHand.Players[0].StartingStack >= decimal.Multiply(parsedHand.GameDescription.Limit.BigBlind, 2) &&
                                    parsedHand.Players[1].StartingStack >= decimal.Multiply(parsedHand.GameDescription.Limit.BigBlind, 2))
                                {
                                    if (parsedHand.ComumnityCards.Count >= 3)
                                    {
                                        ParseHandFromPlayer(0);
                                        ParseHandFromPlayer(1);
                                    }
                                }

                                ++parsedHands;
                                #endregion
                            }
                            catch (Exception e5)
                            {
                                 Console.WriteLine(e5.Message + "\r\n" + e5.StackTrace);
                            }
                        }));

                        #region Preflop
                        CDBHelper.InsertAveragePlayerPreflopRange(lstAvgPlayerPreflopRange);
                        #endregion

                        #region Flop
                        CDBHelper.InsertAveragePlayerBluffsFlop(lstAvgPlayerBluffsFlop);
                        CDBHelper.InsertAveragePlayerBluffsFlopDebug(lstDebugFlop);

                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityFlop(lstAvgPlayerLotsEquityFlop);
                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityDebugFlop(lstDebugFlop2);

                        CDBHelper.InsertAveragePlayerMadeHandFDFlop(lstAvgPlayerMadeHandFDFlop);
                        CDBHelper.InsertAveragePlayerMadeHandFDDebugFlop(lstDebugFlop3);

                        CDBHelper.InsertAveragePlayerMadeHandSDAndFDFlop(lstAvgPlayerMadeHandSDAndFDFlop);
                        CDBHelper.InsertAveragePlayerMadeHandSDAndFDDebugFlop(lstDebugFlop4);

                        CDBHelper.InsertAveragePlayerMadeHandSDFlop(lstAvgPlayerMadeHandSDFlop);
                        CDBHelper.InsertAveragePlayerMadeHandSDDebugFlop(lstDebugFlop5);

                        CDBHelper.InsertAveragePlayerValueHandsFlop(lstAvgPlayerMadeHandValueHandFlop);
                        CDBHelper.InsertAveragePlayerValueHandsDebugFlop(lstDebugFlop6);

                        CDBHelper.InsertFlopFoldStats(lstFlopFoldsStats);
                        CDBHelper.InsertFlopOtherStats(lstFlopOtherStats);
                        #endregion

                        #region Turn
                        CDBHelper.InsertAveragePlayerBluffsTurn(lstAvgPlayerBluffsTurn);
                        CDBHelper.InsertAveragePlayerBluffsTurnDebug(lstDebugTurn);

                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityTurn(lstAvgPlayerLotsEquityTurn);
                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityTurnDebug(lstDebugTurn2);

                        CDBHelper.InsertAveragePlayerMadeHandFDTurn(lstAvgPlayerMadeHandFDTurn);
                        CDBHelper.InsertAveragePlayerMadeHandFDTurnDebug(lstDebugTurn3);

                        CDBHelper.InsertAveragePlayerMadeHandSDAndFDTurn(lstAvgPlayerMadeHandSDAndFDTurn);
                        CDBHelper.InsertAveragePlayerMadeHandSDAndFDTurnDebug(lstDebugTurn4);

                        CDBHelper.InsertAveragePlayerMadeHandSDTurn(lstAvgPlayerMadeHandSDTurn);
                        CDBHelper.InsertAveragePlayerMadeHandSDTurnDebug(lstDebugTurn5);

                        CDBHelper.InsertAveragePlayerValueHandsTurn(lstAvgPlayerMadeHandValueHandTurn);
                        CDBHelper.InsertAveragePlayerValueHandsTurnDebug(lstDebugTurn6);

                        CDBHelper.InsertTurnFoldStats(lstTurnFoldsStats);
                        CDBHelper.InsertTurnOtherStats(lstTurnOtherStats);
                        #endregion

                        #region River
                        CDBHelper.InsertAveragePlayerBluffsRiver(lstAvgPlayerBluffsRiver);
                        CDBHelper.InsertAveragePlayerBluffsRiverDebug(lstDebugRiver);

                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityRiver(lstAvgPlayerLotsEquityRiver);
                        CDBHelper.InsertAveragePlayerBluffsWithALotsOfEquityDebugRiver(lstDebugRiver2);

                        CDBHelper.InsertAveragePlayerMadeHandsBlockersRiver(lstAvgPlayerMadeHandBlockerRiver);
                        CDBHelper.InsertAveragePlayerMadeHandsBlockersDebugRiver(lstDebugRiver3);

                        CDBHelper.InsertAveragePlayerValueHandsRiver(lstAvgPlayerMadeHandValueHandRiver);
                        CDBHelper.InsertAveragePlayerValueHandsDebugRiver(lstDebugRiver4);

                        CDBHelper.InsertRiverFoldStats(lstRiverFoldsStats);
                        CDBHelper.InsertRiverOtherStats(lstRiverOtherStats);
                        #endregion
                        
                        SW.Stop();
                        Console.WriteLine("Parsed " + parsedHands + " hands." + Math.Round(SW.Elapsed.TotalMilliseconds, 2) + "ms");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message + "\r\n" + ex.StackTrace, "Error");
                }
            });
        }

        public decimal GetUnifiedCountForOuts(sbyte _numberOfOuts, ulong _boardMask)
        {
            decimal unifiedCount = 0;

            foreach (var pocketMask in Hand.Hands(0L, _boardMask, 2))
            {
                if (Hand.GetHandTypeExcludingBoard(pocketMask, _boardMask) != Hand.HandTypes.HighCard)
                {
                    int numberOfOuts = Hand.OutsDiscounted(pocketMask, _boardMask, new ulong[0]);

                    if (numberOfOuts == _numberOfOuts && (Hand.HandStrength(pocketMask, _boardMask) < 0.9))                    
                        ++unifiedCount;                    
                }
            }
            
            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForOutsRiver(sbyte _numberOfOuts, ulong _turnBoardMask, ulong _riverBoardMask)
        {
            decimal unifiedCount = 0;

            foreach (var pocketMask in Hand.Hands(0L, _riverBoardMask, 2))
            {
                if (Hand.GetHandTypeExcludingBoard(pocketMask, _riverBoardMask) != Hand.HandTypes.HighCard)
                {
                    int numberOfOuts = Hand.OutsDiscounted(pocketMask, _turnBoardMask, new ulong[0]);

                    if (numberOfOuts == _numberOfOuts && (Hand.HandStrength(pocketMask, _riverBoardMask) < 0.9))
                        ++unifiedCount;
                }
            }

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForMadeHandSD(ulong _boardMask)
        {
            decimal unifiedCount = 0;

            foreach (var pocketMask in Hand.Hands(0L, _boardMask, 2))
            {
                if (Hand.GetHandTypeExcludingBoard(pocketMask, _boardMask) != Hand.HandTypes.HighCard)
                {
                    int numberOfOuts = Hand.OutsDiscounted(pocketMask, _boardMask, new ulong[0]);
                    double hs = Hand.HandStrength(pocketMask, _boardMask);
                    bool isStraightDraw = Hand.IsStraightDraw(pocketMask, _boardMask, 0L);
                    bool isFlushDraw = Hand.IsFlushDraw(pocketMask, _boardMask, 0L);

                    if (isStraightDraw && (hs < 0.8) && !isFlushDraw && (numberOfOuts < 10))                    
                        ++unifiedCount;                    
                }
            }

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForMadeHandFD(ulong _boardMask)
        {
            decimal unifiedCount = 0;

            foreach (var pocketMask in Hand.Hands(0L, _boardMask, 2))
            {
                if (Hand.GetHandTypeExcludingBoard(pocketMask, _boardMask) != Hand.HandTypes.HighCard)
                {
                    int numberOfOuts = Hand.OutsDiscounted(pocketMask, _boardMask, new ulong[0]);
                    double hs = Hand.HandStrength(pocketMask, _boardMask);
                    bool isStraightDraw = Hand.IsStraightDraw(pocketMask, _boardMask, 0L);
                    bool isFlushDraw = Hand.IsFlushDraw(pocketMask, _boardMask, 0L);

                    if ((numberOfOuts < 10) && (hs < 0.8) && isFlushDraw && !isStraightDraw)
                        ++unifiedCount;                                            
                }
            }

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForMadeHandSDFD(ulong _boardMask)
        {
            decimal unifiedCount = 0;

            foreach (var pocketMask in Hand.Hands(0L, _boardMask, 2))
            {
                if (Hand.GetHandTypeExcludingBoard(pocketMask, _boardMask) != Hand.HandTypes.HighCard)
                {
                    int numberOfOuts = Hand.OutsDiscounted(pocketMask, _boardMask, new ulong[0]);
                    double hs = Hand.HandStrength(pocketMask, _boardMask);
                    bool isStraightDraw = Hand.IsStraightDraw(pocketMask, _boardMask, 0L);
                    bool isFlushDraw = Hand.IsFlushDraw(pocketMask, _boardMask, 0L);

                    if ((numberOfOuts < 10) && (hs < 0.8) && isStraightDraw && isFlushDraw)
                        ++unifiedCount;
                }              
            }

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForHS(double _handStrength, ulong _boardMask, out double _lowerInterval)
        {
            double index = 0;
            decimal unifiedCount = 0;
            bool HSHigherOrEqualThan80 = (_handStrength >= 0.8);

            if (HSHigherOrEqualThan80)
                index = ((double)(((int)(_handStrength * 50)) * 2) / 100);
            else
                index = ((double)(((int)(_handStrength * 20)) * 5) / 100);

            foreach (var hand in Hand.Hands(0L, _boardMask, 2))
            {
                double currentHS = Hand.HandStrength(hand, _boardMask);
                bool currentHSHigherOrEqualThan80 = (_handStrength >= 0.8);

                if (HSHigherOrEqualThan80 == currentHSHigherOrEqualThan80)
                {
                    if (HSHigherOrEqualThan80)
                    {
                        if (((double)(((int)(currentHS * 50)) * 2) / 100) == index)
                            ++unifiedCount;
                    }
                    else
                    {
                        if (((double)(((int)(currentHS * 20)) * 5) / 100) == index)
                            ++unifiedCount;
                    }                        
                }
            }

            _lowerInterval = index;

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForBluffs(bool _isBDFD, bool _isBDSD, bool _FD, bool _SD, long _indexHighestCard, ulong _boardMask)
        {
            decimal unifiedCount = 0;

            void AddAllHandsThatAreBothFDAndSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                        if (isFD)
                        {
                            bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                            if (isSD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreFDFromHighestCard()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                            if (isFD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                        if (isSD)
                        {
                            if (_indexHighestCard == 0)
                            {
                                if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                                    ++unifiedCount;
                            }
                            else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreBDSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (_indexHighestCard == 0)
                        {
                            if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                            {
                                bool isBDSD = Hand.IsBackDoorStraightDraw(hand, _boardMask);

                                if (isBDSD)
                                    ++unifiedCount;
                            }
                        }
                        else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                        {
                            bool isBDSD = Hand.IsBackDoorStraightDraw(hand, _boardMask);

                            if (isBDSD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreBDFD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                        {
                            bool isBDFD = Hand.IsBackdoorFlushDraw(hand, _boardMask, 0L);

                            if (isBDFD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreBothBDFDAndBDSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (_indexHighestCard == 0)
                        {
                            if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                            {
                                bool isBDFD = Hand.IsBackdoorFlushDraw(hand, _boardMask, 0L);

                                if (isBDFD)
                                {
                                    bool isBDSD = Hand.IsBackDoorStraightDraw(hand, _boardMask);

                                    if (isBDSD)
                                        ++unifiedCount;
                                }
                            }
                        }
                        else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                        {
                            bool isBDFD = Hand.IsBackdoorFlushDraw(hand, _boardMask, 0L);

                            if (isBDFD)
                            {
                                bool isBDSD = Hand.IsBackDoorStraightDraw(hand, _boardMask);

                                if (isBDSD)
                                    ++unifiedCount;
                            }
                        }
                    }
                }
            }
            void AddAllHandsThatAreBothSDAndBDFD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                        if (isSD)
                        {
                            if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                            {
                                bool isBDFD = Hand.IsBackdoorFlushDraw(hand, _boardMask, 0L);

                                if (isBDFD)
                                    ++unifiedCount;
                            }
                        }
                    }
                }
            }
            void AddAllHandsThatHasNoEquity()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                            if (!isFD)
                            {
                                bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                                if (!isSD)
                                {
                                    if (_indexHighestCard == 0)
                                    {
                                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                                            ++unifiedCount;
                                    }
                                    else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                                        ++unifiedCount;
                                }
                            }
                        }
                    }
                }
            }

            if (_FD)
            {
                // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                if (_SD)
                    AddAllHandsThatAreBothFDAndSD();
                // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand
                else
                    AddAllHandsThatAreFDFromHighestCard();
            }
            // His hand was a straight draw, but not a flush draw
            else if (_SD)
            {
                // Could only be doing the action with straight draws + BDFD, so we only include hands that are str8 draws + BDFD in this case
                if (_isBDFD)
                    AddAllHandsThatAreBothSDAndBDFD();
                // In this case, villain probably do the action with all of his straight draws
                else
                    AddAllHandsThatAreSD();
            }
            else if (_isBDFD)
            {
                if (_isBDSD)
                    AddAllHandsThatAreBothBDFDAndBDSD();
                else
                    AddAllHandsThatAreBDFD();
            }
            else if (_isBDSD)
                AddAllHandsThatAreBDSD();
            // At this point the hand was: 1 - Not a FD. 2- Not a SD. 3- Not a BDFD. 4- Not a BDSD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
            else
                AddAllHandsThatHasNoEquity();

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForBluffsTurn(bool _FD, bool _SD, long _indexHighestCard, ulong _boardMask)
        {
            decimal unifiedCount = 0;

            void AddAllHandsThatAreBothFDAndSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                        if (isFD)
                        {
                            bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                            if (isSD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreFDFromHighestCard()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                            if (isFD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                        if (isSD)
                        {
                            if (_indexHighestCard == 0)
                            {
                                if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                                    ++unifiedCount;
                            }
                            else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                                ++unifiedCount;
                        }
                    }            
                }
            }
            void AddAllHandsThatHasNoEquity()
            {
                foreach (var hand in Hand.Hands(0L, _boardMask, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMask, 0L);

                            if (!isFD)
                            {
                                bool isSD = Hand.IsStraightDraw(hand, _boardMask, 0L);

                                if (!isSD)
                                {
                                    if (_indexHighestCard == 0)
                                    {
                                        if (CBotPokerAmigo.CardIndex(hand, _boardMask) == 0)
                                            ++unifiedCount;
                                    }
                                    else if (CBotPokerAmigo.CardIndex(hand, _boardMask) != 0)
                                        ++unifiedCount;
                                }
                            }
                        }
                    }
                }
            }

            if (_FD)
            {
                // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                if (_SD)
                    AddAllHandsThatAreBothFDAndSD();
                // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand
                else
                    AddAllHandsThatAreFDFromHighestCard();
            }
            // His hand was a straight draw, but not a flush draw
            else if (_SD)
            {
                // In this case, villain probably do the action with all of his straight draws
                AddAllHandsThatAreSD();
            }
            // At this point the hand was: 1 - Not a FD. 2- Not a SD. 3- Not a BDFD. 4- Not a BDSD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
            else
                AddAllHandsThatHasNoEquity();

            return decimal.Divide(1, unifiedCount);
        }

        public decimal GetUnifiedCountForBluffsRiver(bool _FD, bool _SD, long _indexHighestCard, ulong _boardMaskTurn, ulong _boardMaskRiver)
        {
            decimal unifiedCount = 0;

            void AddAllHandsThatAreBothFDAndSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMaskRiver, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMaskRiver) == Hand.HandTypes.HighCard)
                    {
                        bool isFD = Hand.IsFlushDraw(hand, _boardMaskTurn, 0L);

                        if (isFD)
                        {
                            bool isSD = Hand.IsStraightDraw(hand, _boardMaskTurn, 0L);

                            if (isSD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreFDFromHighestCard()
            {
                foreach (var hand in Hand.Hands(0L, _boardMaskRiver, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMaskRiver) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMaskTurn, 0L);

                            if (isFD)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatAreSD()
            {
                foreach (var hand in Hand.Hands(0L, _boardMaskRiver, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMaskRiver) == Hand.HandTypes.HighCard)
                    {
                        bool isSD = Hand.IsStraightDraw(hand, _boardMaskTurn, 0L);

                        if (isSD)
                        {
                            if (_indexHighestCard == 0)
                            {
                                if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) == 0)
                                    ++unifiedCount;
                            }
                            else if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) != 0)
                                ++unifiedCount;
                        }
                    }
                }
            }
            void AddAllHandsThatHasNoEquity()
            {
                foreach (var hand in Hand.Hands(0L, _boardMaskRiver, 2))
                {
                    if (Hand.GetHandTypeExcludingBoard(hand, _boardMaskRiver) == Hand.HandTypes.HighCard)
                    {
                        if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) == _indexHighestCard)
                        {
                            bool isFD = Hand.IsFlushDraw(hand, _boardMaskTurn, 0L);

                            if (!isFD)
                            {
                                bool isSD = Hand.IsStraightDraw(hand, _boardMaskTurn, 0L);

                                if (!isSD)
                                {
                                    if (_indexHighestCard == 0)
                                    {
                                        if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) == 0)
                                            ++unifiedCount;
                                    }
                                    else if (CBotPokerAmigo.CardIndex(hand, _boardMaskRiver) != 0)
                                        ++unifiedCount;
                                }
                            }
                        }
                    }
                }
            }

            if (_FD)
            {
                // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                if (_SD)
                    AddAllHandsThatAreBothFDAndSD();
                // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand
                else
                    AddAllHandsThatAreFDFromHighestCard();
            }
            // His hand was a straight draw, but not a flush draw
            else if (_SD)
            {
                // In this case, villain probably do the action with all of his straight draws
                AddAllHandsThatAreSD();
            }
            // At this point the hand was: 1 - Not a FD. 2- Not a SD. 3- Not a BDFD. 4- Not a BDSD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
            else
                AddAllHandsThatHasNoEquity();

            return decimal.Divide(1, unifiedCount);
        }
    }
}
