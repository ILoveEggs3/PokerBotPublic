using Amigo.Bots;
using Amigo.Events;
using Amigo.Helpers;
using Amigo.Interfaces;
using Amigo.Views;
using HandHistories.Objects.Actions;
using HandHistories.Objects.Hand;
using HoldemHand;
using Shared.Helpers;
using Shared.Models.Database;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Controllers
{
    public class CHandReplayerController: IGameReplayerController
    {
        public event EventHandler<COnNewHandEventArgs> POnNewHand;
        public event EventHandler<COnNewActionEventArgs> POnNewAction;
        public event EventHandler<COnNewStreetEventArgs> POnNewStreet;
        public event EventHandler<COnHandFinishedEventArgs> POnHandFinished;
        public event EventHandler<COnNewRangeReceivedEventArgs> POnNewRangeReceived;

        private int FFCurrentIndexHandAction;
        private int FFPlayerThatWeWatchIndex;
        private bool FFAlreadyUpdatedRangeOnFlop;
        private bool FFAlreadyUpdatedRangeOnTurn;
        private bool FFAlreadyUpdatedRangeOnRiver;
        private HandHistory FFHandHistory;
        private Hand FFBTNHand;
        private Hand FFBBHand;
        private CBoard FFBoard;
        private CBoard FFBoardWithFlopOnly;
        private CBoard FFBoardWithTurnOnly;
        private CGame2MaxManualController FFSimulator;

        private List<HandAction> FFLstHandFilteredActions;
        private List<(ulong, double)> FFLstCurrentRange;        

        public CHandReplayerController()
        {
            FFLstHandFilteredActions = new List<HandAction>();
            FFSimulator = null;
            FFCurrentIndexHandAction = 0;
            FFLstCurrentRange = null;
            FFAlreadyUpdatedRangeOnFlop = false;
            FFAlreadyUpdatedRangeOnTurn = false;
            FFAlreadyUpdatedRangeOnRiver = false;
            FFBTNHand = null;
            FFBBHand = null;
            FFPlayerThatWeWatchIndex = -1;
        }

        public void ParseNewHand(HandHistory _handHistory)
        {
            void FilterActions()
            {
                foreach (HandAction currentHandAction in FFHandHistory.HandActions)
                {
                    switch (currentHandAction.HandActionType)
                    {
                        case HandActionType.CALL:
                        case HandActionType.RAISE:
                        case HandActionType.CHECK:
                        case HandActionType.BET:
                        case HandActionType.FOLD:
                            FFLstHandFilteredActions.Add(currentHandAction);
                            break;
                    }
                }
            }

            FFLstHandFilteredActions.Clear();
            FFCurrentIndexHandAction = 0;            
            FFHandHistory = _handHistory;
            FFLstCurrentRange = null;
            FFAlreadyUpdatedRangeOnFlop = false;
            FFAlreadyUpdatedRangeOnTurn = false;
            FFAlreadyUpdatedRangeOnRiver = false;
            FFBTNHand = null;
            FFBBHand = null;
            FFPlayerThatWeWatchIndex = -1;

            CDBHelperHandInfos.PLstAllBoardsInfos.Clear();
            CDBHelperHandInfos.PLstAllBoardsInfos.Add(new Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, Hand.HandTypes)>>(100));
            CDBHelperHandInfos.PLstAllBoardsInfos.Add(new Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, Hand.HandTypes)>>(100));

            FilterActions();

            CPlayer btnPlayer = null;
            CPlayer bbPlayer = null;

            Hand btnPlayerCards = null;
            Hand bbPlayerCards = null;

            if (FFHandHistory.Players[0].SeatNumber == FFHandHistory.DealerButtonPosition)
            {
                btnPlayer = new CPlayer(FFHandHistory.Players[0].StartingStack, FFHandHistory.Players[0].PlayerName);

                if (FFHandHistory.Players[0].hasHoleCards)
                    btnPlayerCards = new Hand() { PocketCards = FFHandHistory.Players[0].HoleCards.ToString() };

                bbPlayer = new CPlayer(FFHandHistory.Players[1].StartingStack, FFHandHistory.Players[1].PlayerName);

                if (FFHandHistory.Players[1].hasHoleCards)
                    bbPlayerCards = new Hand() { PocketCards = FFHandHistory.Players[1].HoleCards.ToString() };
            }
            else
            {
                btnPlayer = new CPlayer(FFHandHistory.Players[1].StartingStack, FFHandHistory.Players[1].PlayerName);

                if (FFHandHistory.Players[1].hasHoleCards)
                    btnPlayerCards = new Hand() { PocketCards = FFHandHistory.Players[1].HoleCards.ToString() };

                bbPlayer = new CPlayer(FFHandHistory.Players[0].StartingStack, FFHandHistory.Players[0].PlayerName);

                if (FFHandHistory.Players[0].hasHoleCards)
                    bbPlayerCards = new Hand() { PocketCards = FFHandHistory.Players[0].HoleCards.ToString() };
            }

            CCard card1 = null;
            CCard card2 = null;
            CCard card3 = null;
            CCard card4 = null;
            CCard card5 = null;

            if (FFHandHistory.ComumnityCards[0] != null)
            {
                card1 = FFHandHistory.ComumnityCards[0].ToString().ToCCarte();
                card2 = FFHandHistory.ComumnityCards[1].ToString().ToCCarte();
                card3 = FFHandHistory.ComumnityCards[2].ToString().ToCCarte();

                if (FFHandHistory.ComumnityCards[3] != null)
                {
                    card4 = FFHandHistory.ComumnityCards[3].ToString().ToCCarte();

                    if (FFHandHistory.ComumnityCards[4] != null)
                        card5 = FFHandHistory.ComumnityCards[4].ToString().ToCCarte();
                }                    
            }
            
            FFBoard = new CBoard(card1, card2, card3, card4, card5);
            FFBoardWithFlopOnly = new CBoard(card1, card2, card3);
            FFBoardWithTurnOnly = new CBoard(card1, card2, card3, card4);

            if ((object)btnPlayerCards != null)
                FFPlayerThatWeWatchIndex = 0;
            else if ((object)bbPlayerCards != null)
                FFPlayerThatWeWatchIndex = 1;

            FFSimulator = new CGame2MaxManualController(btnPlayer, 
                                                        bbPlayer, 
                                                        FFHandHistory.GameDescription.Limit.SmallBlind, 
                                                        FFHandHistory.GameDescription.Limit.BigBlind, 
                                                        FFHandHistory.GameDescription.Limit.Ante,
                                                        btnPlayerCards,
                                                        bbPlayerCards,
                                                        FFBoard);
            FFSimulator.POnNewHand += OnNewHand;
            FFSimulator.POnNewAction += OnNewAction;
            FFSimulator.POnNewStreet += OnNewStreet;
            FFSimulator.POnHandFinished += OnHandFinished;
            FFSimulator.PlayNewHand();
        }

        public void Forward()
        {
            if (FFHandHistory == null)
                throw new InvalidOperationException("There is no hand to analyze!");
            else if (FFCurrentIndexHandAction > FFLstHandFilteredActions.Count - 1)
                throw new InvalidOperationException("The current hand to analyze is already finished!");

            var lastActionStreet = FFSimulator.PCurrentStreet;
            var lastActionFromVillain = FFLstHandFilteredActions[FFCurrentIndexHandAction].HandActionType;
            bool doNotPass = false;
            switch (lastActionFromVillain)
            {
                case HandActionType.CHECK:
                    FFSimulator.Check();
                    break;
                case HandActionType.CALL:
                    doNotPass = (FFPlayerThatWeWatchIndex == 1);
                    FFSimulator.Call();                    
                    break;
                case HandActionType.BET:
                    FFSimulator.Bet(FFLstHandFilteredActions[FFCurrentIndexHandAction].Amount);
                    break;
                case HandActionType.RAISE:
                    FFSimulator.Raise(FFLstHandFilteredActions[FFCurrentIndexHandAction].Amount);
                    break;
                case HandActionType.FOLD:
                    FFSimulator.Fold();
                    break;
                default:
                    throw new InvalidOperationException("Invalid action!");
            }
            var currentGameState = FFSimulator.PCurrentGameState;

            if (currentGameState.PCurrentTurnPlayerIndex == FFPlayerThatWeWatchIndex)
            {
                // If we're from BTN
                if (FFPlayerThatWeWatchIndex == 0)
                {
                    switch (lastActionStreet)
                    {
                        case Street.Flop:
                            #region Flop
                            var villainPosition = PokerPosition.BB;
                            CBoardModel boardModel = new CBoardModel(FFBoardWithFlopOnly.PMask);

                            if (((object)FFBTNHand == null))
                            {
                                string BTNCards = FFSimulator.GetBTNCards();

                                if (BTNCards != null)
                                    FFBTNHand = new Hand() { PocketCards = BTNCards, BoardMask = FFBoardWithFlopOnly.PMask };
                                else
                                    throw new InvalidOperationException("Unable to read hero's card!");
                            }

                            if (!FFAlreadyUpdatedRangeOnFlop)
                            {                                
                                CDBHelperHandInfos.LoadBoardInfosAsync(boardModel.PBoardMask, FFBTNHand.PocketMask, FFPlayerThatWeWatchIndex);
                                FFLstCurrentRange = CBotPokerAmigo.GetRangePreflop(currentGameState.PTypePot, villainPosition, FFBTNHand.PocketMask, FFBoardWithFlopOnly.PMask);
                                FFAlreadyUpdatedRangeOnFlop = true;
                            }
                            
                            FFLstCurrentRange = CBotPokerAmigo.GetRangeFlop(currentGameState.PGameState, boardModel.CalculateMetaData(), FFLstCurrentRange, boardModel.PBoardMask, FFPlayerThatWeWatchIndex);

                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms3 = new Stopwatch();
                            ms3.Start();
                            var flopBestGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoardWithFlopOnly.PMask, FFBTNHand.PocketMask, FFPlayerThatWeWatchIndex);
                            ms3.Stop();
                            Console.WriteLine(ms3.ElapsedMilliseconds + " MS");
                            Console.WriteLine(flopBestGameState.Item1.GetLastAction().PAction.ToString());
                            Console.WriteLine(flopBestGameState.Item1.GetLastAction().PMise.ToString());                                                            
                            #endregion
                            break;
                        case Street.Turn:
                            #region Turn
                            CBoardModel boardModelTurn = new CBoardModel(FFBoardWithTurnOnly.PMask);

                            if ((object)FFBTNHand != null)
                                FFBTNHand.BoardMask = FFBoardWithTurnOnly.PMask;

                            if (!FFAlreadyUpdatedRangeOnTurn)
                            {
                                FFLstCurrentRange = CBotPokerAmigo.UpdateRange(FFLstCurrentRange.ToList(), FFBoardWithTurnOnly.PBoardList[3].PMask);
                                FFAlreadyUpdatedRangeOnTurn = true;
                            }

                            FFLstCurrentRange = CBotPokerAmigo.GetRangeTurn(currentGameState.PGameState, boardModelTurn.CalculateMetaData(), FFLstCurrentRange, boardModelTurn.PBoardMask, FFPlayerThatWeWatchIndex);

                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms = new Stopwatch();
                            ms.Start();
                            var bestGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoardWithTurnOnly.PMask, FFBTNHand.PocketMask, FFPlayerThatWeWatchIndex);
                            ms.Stop();
                            Console.WriteLine(ms.ElapsedMilliseconds + " MS");

                            Console.WriteLine(bestGameState.Item1.GetLastAction().PAction.ToString());
                            Console.WriteLine(bestGameState.Item1.GetLastAction().PMise.ToString());
                            #endregion
                            break;
                        case Street.River:
                            #region River
                            CBoardModel boardModelRiver = new CBoardModel(FFBoard.PMask);

                            if ((object)FFBTNHand != null)
                                FFBTNHand.BoardMask = FFBoard.PMask;

                            if (!FFAlreadyUpdatedRangeOnRiver)
                            {
                                FFLstCurrentRange = CBotPokerAmigo.UpdateRange(FFLstCurrentRange, FFBoard.PBoardList[4].PMask);
                                FFAlreadyUpdatedRangeOnRiver = true;
                            }

                            FFLstCurrentRange = CBotPokerAmigo.GetRangeRiver(currentGameState.PGameState, boardModelRiver.CalculateMetaData(), FFLstCurrentRange, FFBoardWithTurnOnly.PMask, boardModelRiver.PBoardMask, FFPlayerThatWeWatchIndex);

                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms2 = new Stopwatch();
                            ms2.Start();
                            var bestRiverGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoard.PMask, FFBTNHand.PocketMask, FFPlayerThatWeWatchIndex);
                            ms2.Stop();
                            Console.WriteLine(ms2.ElapsedMilliseconds + " MS");

                            #endregion
                            break;
                    }
                }
                else
                {
                    switch (currentGameState.PCurrentStreet)
                    {
                        case Street.Flop:
                            #region Flop
                            var villainPosition = PokerPosition.BTN;
                            CBoardModel boardModel = new CBoardModel(FFBoardWithFlopOnly.PMask);

                            if ((object)FFBBHand == null)
                            {
                                string BBCards = FFSimulator.GetBBCards();

                                if (BBCards != null)
                                    FFBBHand = new Hand() { PocketCards = BBCards, BoardMask = FFBoardWithFlopOnly.PMask };
                                else
                                    throw new InvalidOperationException("Unable to read hero's card!");
                            }

                            if (!FFAlreadyUpdatedRangeOnFlop)
                            {
                                CDBHelperHandInfos.LoadBoardInfosAsync(FFBoardWithFlopOnly.PMask, FFBBHand.PocketMask, FFPlayerThatWeWatchIndex);
                                FFLstCurrentRange = CBotPokerAmigo.GetRangePreflop(currentGameState.PTypePot, villainPosition, FFBBHand.PocketMask, FFBoardWithFlopOnly.PMask);
                                FFAlreadyUpdatedRangeOnFlop = true;
                            }
                            else
                                FFLstCurrentRange = CBotPokerAmigo.GetRangeFlop(currentGameState.PGameState, boardModel.CalculateMetaData(), FFLstCurrentRange, boardModel.PBoardMask, FFPlayerThatWeWatchIndex);
                            
                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms3 = new Stopwatch();
                            ms3.Start();

                            if (currentGameState.PTypeFilteredPot == TypesPot.TwoBet)
                            {
                                if (currentGameState.GetLastActionStreet() != Street.Preflop)
                                {                                    
                                    var flopBestGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoardWithFlopOnly.PMask, FFBBHand.PocketMask, FFPlayerThatWeWatchIndex);
                                    ms3.Stop();
                                    Console.WriteLine(ms3.ElapsedMilliseconds + " MS");
                                    Console.WriteLine(flopBestGameState.Item1.GetLastAction().PAction.ToString());
                                    Console.WriteLine(flopBestGameState.Item1.GetLastAction().PMise.ToString());
                                }
                            }

                            // int mama = 0;
                            #endregion
                            break;
                        case Street.Turn:
                            #region Turn
                            CBoardModel boardModelTurn = new CBoardModel(FFBoardWithTurnOnly.PMask);

                            if ((object)FFBBHand != null)
                                FFBBHand.BoardMask = FFBoardWithTurnOnly.PMask;

                            if (!FFAlreadyUpdatedRangeOnTurn)
                            {
                                // If BTN checked back
                                if (currentGameState.GetLastAction().PAction == PokerAction.Check)
                                {
                                    CBoardModel flopBoardModel = new CBoardModel(FFBoardWithFlopOnly.PMask);                                    

                                    FFLstCurrentRange = CBotPokerAmigo.GetRangeFlop(currentGameState.PGameState, flopBoardModel.CalculateMetaData(), FFLstCurrentRange, flopBoardModel.PBoardMask, FFPlayerThatWeWatchIndex);
                                }

                                FFLstCurrentRange = CBotPokerAmigo.UpdateRange(FFLstCurrentRange, FFBoardWithTurnOnly.PBoardList[3].PMask);
                                FFAlreadyUpdatedRangeOnTurn = true;
                            }
                            else                                                            
                                FFLstCurrentRange = CBotPokerAmigo.GetRangeTurn(currentGameState.PGameState, boardModelTurn.CalculateMetaData(), FFLstCurrentRange, boardModelTurn.PBoardMask, FFPlayerThatWeWatchIndex);                          

                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms = new Stopwatch();
                            ms.Start();
                            var bestGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoardWithTurnOnly.PMask, FFBBHand.PocketMask, FFPlayerThatWeWatchIndex);
                            ms.Stop();
                            Console.WriteLine(ms.ElapsedMilliseconds + " MS");

                            Console.WriteLine(bestGameState.Item1.GetLastAction().PAction.ToString());                            
                            Console.WriteLine(bestGameState.Item1.GetLastAction().PMise.ToString());
                            #endregion
                            break;
                        case Street.River:
                            #region River      
                            CBoardModel boardModelRiver = new CBoardModel(FFBoard.PMask);

                            if ((object)FFBBHand != null)
                                FFBBHand.BoardMask = FFBoard.PMask;

                            if (!FFAlreadyUpdatedRangeOnRiver)
                            {                                
                                // If BTN checked back
                                if (currentGameState.GetLastAction().PAction == PokerAction.Check)
                                {
                                    CBoardModel boardModelTurn2 = new CBoardModel(FFBoardWithTurnOnly.PMask);

                                    var qwe = (CStateRiver)currentGameState;

                                    FFLstCurrentRange = CBotPokerAmigo.GetRangeTurn(qwe.PTurnStateID, boardModelTurn2.CalculateMetaData(), FFLstCurrentRange, boardModelTurn2.PBoardMask, FFPlayerThatWeWatchIndex);
                                }

                                FFLstCurrentRange = CBotPokerAmigo.UpdateRange(FFLstCurrentRange, FFBoard.PBoardList[4].PMask);
                                FFAlreadyUpdatedRangeOnRiver = true;
                            }
                            else                                                            
                                FFLstCurrentRange = CBotPokerAmigo.GetRangeRiver(currentGameState.PGameState, boardModelRiver.CalculateMetaData(), FFLstCurrentRange, FFBoardWithTurnOnly.PMask, boardModelRiver.PBoardMask, FFPlayerThatWeWatchIndex);                            

                            POnNewRangeReceived(this, new COnNewRangeReceivedEventArgs(FFLstCurrentRange));

                            Stopwatch ms2 = new Stopwatch();
                            ms2.Start();
                            var bestRiverGameState = CPokerMath.GetEv(currentGameState, FFLstCurrentRange, FFBoard.PMask, FFBBHand.PocketMask, FFPlayerThatWeWatchIndex);
                            ms2.Stop();
                            Console.WriteLine(ms2.ElapsedMilliseconds + " MS");

                            #endregion
                            break;
                    }
                }
            }

            ++FFCurrentIndexHandAction;
        }

        public void OnNewHand(object sender, COnNewHandEventArgs e)
        {
            POnNewHand(sender, e);
        }

        public void OnNewAction(object sender, COnNewActionEventArgs e)
        {
            POnNewAction(sender, e);
        }

        public void OnNewStreet(object sender, COnNewStreetEventArgs e)
        {
            POnNewStreet(sender, e);
        }

        public void OnHandFinished(object sender, COnHandFinishedEventArgs e)
        {
            POnHandFinished(sender, e);
        }
    }
}
