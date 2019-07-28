using Amigo.Helpers;
using Amigo.Models.MyModels.GameState;
using Shared.Helpers;
using Shared.Models.Database;
using Shared.Poker.Helpers;
using Shared.Poker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Controllers
{
    /*public class CGameStateNLHE2Max : AState
    {

        private CFlopGameState FFBTNFlopGameState;
        private CFlopGameState FFBBFlopGameState;
        public CFlopGameState PGameState
        {
            get => (GetVillainPlayerPosition() == PokerPosition.BTN ? FFBTNFlopGameState : FFBBFlopGameState);
        }

        private CTurnGameState FFBTNTurnGameState;
        private CTurnGameState FFBBTurnGameState;
        public CTurnGameState PGameState
        {
            get => (GetVillainPlayerPosition() == PokerPosition.BTN ? FFBTNTurnGameState : FFBBTurnGameState);
        }

        private CRiverGameState FFBTNRiverGameState;
        private CRiverGameState FFBBRiverGameState;
        public CRiverGameState PGameState
        {
            get => (GetVillainPlayerPosition() == PokerPosition.BTN ? FFBTNRiverGameState : FFBBRiverGameState);
        }

        protected override Street FFCurrentStreet => Street.Preflop;

        private CFlopGameState LoadAndGetFlopGameState()
        {
            if (FFActionList[FFActionList.Count - 1].Item3 != Street.Flop)
                return null;

            (TypesPot, PokerPosition, PokerAction, long?) tupleInfos;
            var lastAction = GetFilteredActionFromLastAction();
            long? actionBetType = null;

            switch (lastAction.PAction)
            {
                case PokerAction.Check:
                    break;
                case PokerAction.Bet:
                    actionBetType = (long?)GetClosestBetSizeFromLastAction();
                    break;
                case PokerAction.Call:
                    actionBetType = (long?)GetClosestBetSizeFromLastCallAction();
                    break;
                case PokerAction.CallVsRaise:
                    switch(PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.Raise:
                    switch (PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.ReRaise:
                    break;
                case PokerAction.CallVsReRaise:
                    actionBetType = (long?)GetClosestReRaiseCallSizeFromLastAction();
                    break;                 
                default:
                    throw new InvalidOperationException("Action not supported");
            }

            var villainPosition = GetVillainPlayerPosition();
            tupleInfos = (PTypeFilteredPot, villainPosition, lastAction.PAction, actionBetType);
            var flopGameState = CDBHelper.PDicAllFlopGameStatesByInfos[tupleInfos];

            if (villainPosition == PokerPosition.BTN)
                FFBTNFlopGameState = flopGameState;
            else
                FFBBFlopGameState = flopGameState;

            return flopGameState;
        }

        private CTurnGameState LoadAndGetTurnGameState()
        {
            if (FFActionList[FFActionList.Count - 1].Item3 != Street.Turn)
                return null;

            (CFlopGameState, PokerAction, long?) tupleInfos;
            var lastAction = GetFilteredActionFromLastAction();
            long? actionBetType = null;

            switch (lastAction.PAction)
            {
                case PokerAction.Check:
                    break;
                case PokerAction.Bet:
                    actionBetType = (long?)GetClosestBetSizeFromLastAction();
                    break;
                case PokerAction.Call:
                    actionBetType = (long?)GetClosestBetSizeFromLastCallAction();
                    break;
                case PokerAction.CallVsRaise:
                    switch (PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.Raise:
                    switch (PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.ReRaise:
                    break;
                case PokerAction.CallVsReRaise:
                    actionBetType = (long?)GetClosestReRaiseCallSizeFromLastAction();
                    break;
                default:
                    throw new InvalidOperationException("Action not supported");
            }

            var villainPosition = GetVillainPlayerPosition();

            if (villainPosition == PokerPosition.BTN)
            {
                if (FFBTNFlopGameState.PTypeBet == 135 || FFBTNFlopGameState.PTypeBet == 136)
                    return null;

                tupleInfos = (FFBTNFlopGameState, lastAction.PAction, actionBetType);

                var turnGameState = CDBHelper.PDicAllTurnGameStatesByInfos[tupleInfos];
                FFBTNTurnGameState = turnGameState;

                return turnGameState;
            }
            else
            {
                if (FFBBFlopGameState.PTypeBet == 135 || FFBBFlopGameState.PTypeBet == 136)
                    return null;

                tupleInfos = (FFBBFlopGameState, lastAction.PAction, actionBetType);

                var turnGameState = CDBHelper.PDicAllTurnGameStatesByInfos[tupleInfos];
                FFBBTurnGameState = turnGameState;

                return turnGameState;
            }
        }

        private CRiverGameState LoadAndGetRiverGameState()
        {

            (CTurnGameState, PokerAction, long?) tupleInfos;
            var lastAction = GetFilteredActionFromLastAction();
            long? actionBetType = null;

            switch (lastAction.PAction)
            {
                case PokerAction.Check:
                    break;
                case PokerAction.Bet:
                    actionBetType = (long?)GetClosestBetSizeFromLastAction();
                    break;
                case PokerAction.Call:
                    actionBetType = (long?)GetClosestBetSizeFromLastCallAction();
                    break;
                case PokerAction.CallVsRaise:
                    switch (PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastCallActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.Raise:
                    switch (PTypeFilteredPot)
                    {
                        case TypesPot.TwoBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            actionBetType = (long?)GetClosestRaiseSizeFromLastActionFourBetPot();
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                    break;
                case PokerAction.ReRaise:
                    break;
                case PokerAction.CallVsReRaise:
                    actionBetType = (long?)GetClosestReRaiseCallSizeFromLastAction();
                    break;                            
                default:
                    throw new InvalidOperationException("Action not supported");
            }
            var villainPosition = GetVillainPlayerPosition();

            if (villainPosition == PokerPosition.BTN)
            {
                if (FFBTNTurnGameState.PTypeBet == 135 || FFBTNTurnGameState.PTypeBet == 136)
                    return null;

                tupleInfos = (FFBTNTurnGameState, lastAction.PAction, actionBetType);
                var riverGameState = CDBHelper.PDicAllRiverGameStatesByInfos[tupleInfos];

                FFBTNRiverGameState = riverGameState;
                return riverGameState;
            }
            else
            {
                if (FFBBTurnGameState.PTypeBet == 135 || FFBBTurnGameState.PTypeBet == 136)
                    return null;

                tupleInfos = (FFBBTurnGameState, lastAction.PAction, actionBetType);
                var riverGameState = CDBHelper.PDicAllRiverGameStatesByInfos[tupleInfos];

                FFBBRiverGameState = riverGameState;
                return riverGameState;

            }
        }

        public BetSizePossible GetClosestBetSizeFromLastAction()
        {
            int indLastAction = (FFActionList.Count - 1);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            PokerAction lastAction = lastActionObject.PAction;

            if (lastAction != PokerAction.Bet)
                throw new InvalidOperationException("Action is not supported");

            double bet = lastActionObject.PMise;
            double potBeforeTheBet = (PPot - bet);

            if (bet > (potBeforeTheBet * 1.33))
            {
                if (PIsHandShortStack)
                    return BetSizePossible.AllInShort;
                else
                    return BetSizePossible.AllIn;
            }
            else
            {
                BetSizePossible closestBetSize = BetSizePossible.Percent33;
                double percent33Pot = (potBeforeTheBet * 0.33);
                double percent50Pot = (potBeforeTheBet * 0.50);
                double closestBetSizeDifference = Math.Abs((bet - percent33Pot));

                double otherBetSizeDifference = Math.Abs((bet - percent50Pot));
                if (otherBetSizeDifference < closestBetSizeDifference)
                {
                    closestBetSize = BetSizePossible.Percent50;
                    closestBetSizeDifference = otherBetSizeDifference;

                    double percent72Pot = (potBeforeTheBet * 0.72);
                    otherBetSizeDifference = Math.Abs((bet - percent72Pot));

                    if (otherBetSizeDifference < closestBetSizeDifference)
                    {
                        closestBetSize = BetSizePossible.Percent72;
                        closestBetSizeDifference = otherBetSizeDifference;

                        otherBetSizeDifference = Math.Abs((bet - potBeforeTheBet));

                        if (otherBetSizeDifference < closestBetSizeDifference)
                        {
                            closestBetSize = BetSizePossible.Percent100;
                            closestBetSizeDifference = otherBetSizeDifference;

                            double percent133Pot = (potBeforeTheBet * 1.33);
                            otherBetSizeDifference = Math.Abs((bet - percent133Pot));

                            if (otherBetSizeDifference < closestBetSizeDifference)
                            {
                                closestBetSize = BetSizePossible.Percent133;
                                closestBetSizeDifference = otherBetSizeDifference;
                            }
                        }
                    }
                }

                return closestBetSize;
            }
        }
        public BetSizePossible GetClosestBetSizeFromLastCallAction()
        {
            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction lastLastActionObject = FFActionList[indLastLastAction].Item1;

            if (!(lastActionObject.PAction == PokerAction.Call && lastLastActionObject.PAction == PokerAction.Bet))
                throw new InvalidOperationException("Invalid action. The last action must be a call and the action before the last action must be a bet");

            double callSize = lastActionObject.PMise;
            double potBeforeTheBetAndTheCall = (PPot - lastActionObject.PMise - lastActionObject.PMise);

            if (callSize > (potBeforeTheBetAndTheCall * 1.33))
            {
                if (PIsHandShortStack)
                    return BetSizePossible.AllInShort;
                else
                    return BetSizePossible.AllIn;
            }
            else
            {
                BetSizePossible closestBetSize = BetSizePossible.Percent33;
                double percent33Pot = (potBeforeTheBetAndTheCall * 0.33);
                double percent50Pot = (potBeforeTheBetAndTheCall * 0.50);
                double closestBetSizeDifference = Math.Abs((callSize - percent33Pot));

                double otherBetSizeDifference = Math.Abs((callSize - percent50Pot));
                if (otherBetSizeDifference < closestBetSizeDifference)
                {
                    closestBetSize = BetSizePossible.Percent50;
                    closestBetSizeDifference = otherBetSizeDifference;

                    double percent72Pot = (potBeforeTheBetAndTheCall * 0.72);
                    otherBetSizeDifference = Math.Abs((callSize - percent72Pot));

                    if (otherBetSizeDifference < closestBetSizeDifference)
                    {
                        closestBetSize = BetSizePossible.Percent72;
                        closestBetSizeDifference = otherBetSizeDifference;

                        otherBetSizeDifference = Math.Abs((callSize - potBeforeTheBetAndTheCall));

                        if (otherBetSizeDifference < closestBetSizeDifference)
                        {
                            closestBetSize = BetSizePossible.Percent100;
                            closestBetSizeDifference = otherBetSizeDifference;

                            double percent133Pot = (potBeforeTheBetAndTheCall * 1.33);
                            otherBetSizeDifference = Math.Abs((callSize - percent133Pot));

                            if (otherBetSizeDifference < closestBetSizeDifference)
                            {
                                closestBetSize = BetSizePossible.Percent133;
                                closestBetSizeDifference = otherBetSizeDifference;
                            }
                        }
                    }
                }

                return closestBetSize;
            }
        }

        /// <summary>
        /// Only call this method when it's only 1 raise (2+ raise will not work)
        /// </summary>
        /// <param name="_lastAction"></param>
        /// <returns></returns>
        public RaiseSizePossibleTwoBetPot GetClosestRaiseSizeFromLastActionTwoBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.TwoBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;            
            double raise = lastActionObject.PMise;            

            if (actionBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last action should be bet, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The last action should be raise, not anything else!");

            double oldBetSize = actionBeforeLastActionObject.PMise;

            if (raise <= (oldBetSize * 2.7))
                return RaiseSizePossibleTwoBetPot.TwoPoint7Max;
            else if (raise <= (oldBetSize * 4.5))
                return RaiseSizePossibleTwoBetPot.FourPoint5Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (PIsHandShortStack)
                        return RaiseSizePossibleTwoBetPot.AllInShort;
                    else
                        return RaiseSizePossibleTwoBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn;
            }
        }
        public RaiseSizePossibleTwoBetPot GetClosestRaiseSizeFromLastCallActionTwoBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.TwoBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            int indLastLastLastAction = (FFActionList.Count - 3);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = FFActionList[indLastLastLastAction].Item1;
            double callSizeVsRaise = lastActionObject.PMise;

            if (actionBeforeBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last last action should be bet, not anything else!");
            if (actionBeforeLastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The action before the last action should be raise, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Call)
                throw new InvalidOperationException("The last action should be call, not anything else!");

            double oldBetSize = actionBeforeBeforeLastActionObject.PMise;

            if (callSizeVsRaise <= (oldBetSize * 2.7))
                return RaiseSizePossibleTwoBetPot.TwoPoint7Max;
            else if (callSizeVsRaise <= (oldBetSize * 4.5))
                return RaiseSizePossibleTwoBetPot.FourPoint5Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (PIsHandShortStack)
                        return RaiseSizePossibleTwoBetPot.AllInShort;
                    else
                        return RaiseSizePossibleTwoBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn;
            }
        }

        public RaiseSizePossibleThreeBetPot GetClosestRaiseSizeFromLastActionThreeBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.ThreeBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;
            double raise = lastActionObject.PMise;

            if (actionBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last action should be bet, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The last action should be raise, not anything else!");

            double oldBetSize = actionBeforeLastActionObject.PMise;

            if (raise <= (oldBetSize * 3.5))
                return RaiseSizePossibleThreeBetPot.ThreePoint5Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (PIsHandShortStack)
                        return RaiseSizePossibleThreeBetPot.AllInShort;
                    else
                        return RaiseSizePossibleThreeBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn;
            }
        }
        public RaiseSizePossibleThreeBetPot GetClosestRaiseSizeFromLastCallActionThreeBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.ThreeBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            int indLastLastLastAction = (FFActionList.Count - 3);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = FFActionList[indLastLastLastAction].Item1;
            double callSizeVsRaise = lastActionObject.PMise;

            if (actionBeforeBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last last action should be bet, not anything else!");
            if (actionBeforeLastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The action before the last action should be raise, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Call)
                throw new InvalidOperationException("The last action should be call, not anything else!");

            double oldBetSize = actionBeforeBeforeLastActionObject.PMise;

            if (callSizeVsRaise <= (oldBetSize * 3.5))
                return RaiseSizePossibleThreeBetPot.ThreePoint5Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (PIsHandShortStack)
                        return RaiseSizePossibleThreeBetPot.AllInShort;
                    else
                        return RaiseSizePossibleThreeBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn;
            }
        }

        public RaiseSizePossibleFourBetPot GetClosestRaiseSizeFromLastActionFourBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.FourBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot." + typePot.ToString());

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;
            double raise = lastActionObject.PMise;

            if (actionBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last action should be bet, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The last action should be raise, not anything else!");

            double oldBetSize = actionBeforeLastActionObject.PMise;

            if (raise <= (oldBetSize * 2.7))
                return RaiseSizePossibleFourBetPot.TwoPoint7Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)                
                    return RaiseSizePossibleFourBetPot.AllIn;                
                else
                    return RaiseSizePossibleFourBetPot.AnySizingExceptAllIn;
            }
        }
        public RaiseSizePossibleFourBetPot GetClosestRaiseSizeFromLastCallActionFourBetPot()
        {
            TypesPot typePot = PTypeFilteredPot;

            if (typePot != TypesPot.FourBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot." + typePot.ToString());

            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            int indLastLastLastAction = (FFActionList.Count - 3);
            CAction lastActionObject = FFActionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = FFActionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = FFActionList[indLastLastLastAction].Item1;
            double callSizeVsRaise = lastActionObject.PMise;

            if (actionBeforeBeforeLastActionObject.PAction != PokerAction.Bet)
                throw new InvalidOperationException("The action before the last last action should be bet, not anything else!");
            if (actionBeforeLastActionObject.PAction != PokerAction.Raise)
                throw new InvalidOperationException("The action before the last action should be raise, not anything else!");
            else if (lastActionObject.PAction != PokerAction.Call)
                throw new InvalidOperationException("The last action should be call, not anything else!");

            double oldBetSize = actionBeforeBeforeLastActionObject.PMise;

            if (callSizeVsRaise <= (oldBetSize * 2.7))
                return RaiseSizePossibleFourBetPot.TwoPoint7Max;
            else
            {
                CPlayer playerAssociatedToLastAction = FFPlayerList[FFActionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                    return RaiseSizePossibleFourBetPot.AllIn;
                else
                    return RaiseSizePossibleFourBetPot.AnySizingExceptAllIn;
            }
        }

        public CallSizePossibleVsReRaise GetClosestReRaiseCallSizeFromLastAction()
        {
            int indLastAction = (FFActionList.Count - 1);
            int indLastLastAction = (FFActionList.Count - 2);
            int indLastLastLastAction = (FFActionList.Count - 3);

            var tupleInfos = FFActionList[indLastAction];
            CAction lastActionObject = tupleInfos.Item1;
            CAction lastLastActionObject = FFActionList[indLastLastAction].Item1;
            CAction lastLastLastActionObject = FFActionList[indLastLastLastAction].Item1;

            if (!(lastActionObject.PAction == PokerAction.Call && lastLastActionObject.PAction == PokerAction.Raise && lastLastLastActionObject.PAction == PokerAction.Raise))
                throw new InvalidOperationException("The last action must be a call and the two actions after must be a raise, otherwise it is not a call vs a re-raise!");

            CPlayer playerAssociatedToLastAction = FFPlayerList[tupleInfos.Item2];

            if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
            {
                if (PIsHandShortStack)
                    return CallSizePossibleVsReRaise.AllInShort;
                else
                    return CallSizePossibleVsReRaise.AllIn;
            }
            else
                return CallSizePossibleVsReRaise.AnySizingExceptAllIn;
        }


        public override (AGameState, BoardMetaDataFlags, bool?) GetStateKey(BoardMetaDataFlags _bm)
        {
            var action = GetLastAction(PPreviousTurnPlayerIndex).PAction;

            if (action == PokerAction.Fold)
                return (PGameState, _bm, action == PokerAction.Raise);
            else
                return (PGameState, _bm, null);
        }

        protected CGameStateNLHE2Max(CGameStateNLHE2Max _state, AStateStreetVariants _variant) : base (_state, _variant)
        {

        }

        protected override AState CreateNewFromStreetVariant(AStateStreetVariants _variant)
        {
            return new CGameStateNLHE2Max(this, _variant);
        }


        
        public CGameStateNLHE2Max(CPlayer _btnPlayer, CPlayer _bbPlayer, double _smallBlind, double _bigBlind, double _antes, int _heroIndex = 0, int _previousIndex = 0, int _currentTurnPlayerIndex = 0, double _pot = 0.0d, double _minBet = 0.0d) : base(_heroIndex, _currentTurnPlayerIndex, _previousIndex, _pot, _minBet, null)
        {
            if (_btnPlayer.PNumberOfChipsLeft != _btnPlayer.PNumberOfChipsAtBeginningHand)
                throw new ArgumentException("The number of chips left must be the same number as the number of chips at the beginning of the hand");
            else if (_bbPlayer.PNumberOfChipsLeft != _bbPlayer.PNumberOfChipsAtBeginningHand)
                throw new ArgumentException("The number of chips left must be the same number as the number of chips at the beginning of the hand");

            #region Local methods
            void LFInitialiserJoueurs()
            {
                if (_btnPlayer.PNumberOfChipsAtBeginningHand > _bbPlayer.PNumberOfChipsAtBeginningHand)
                {
                    _btnPlayer.PNumberOfChipsAtBeginningHand = _bbPlayer.PNumberOfChipsAtBeginningHand;
                    _btnPlayer.PNumberOfChipsLeft = _bbPlayer.PNumberOfChipsLeft;
                }
                else if (_bbPlayer.PNumberOfChipsAtBeginningHand > _btnPlayer.PNumberOfChipsAtBeginningHand)
                {
                    _bbPlayer.PNumberOfChipsAtBeginningHand = _btnPlayer.PNumberOfChipsAtBeginningHand;
                    _bbPlayer.PNumberOfChipsLeft = _btnPlayer.PNumberOfChipsLeft;
                }

                // Création des joueurs
                FFPlayerList = new CPlayer[2] { _btnPlayer, _bbPlayer };

                // Création de l'historique des actions des joueurs
                FFActionList = new List<Tuple<CAction, int, Street>>();

                // Création de la liste qui indique les actions possible pour un joueur X
                FLstActionsPossibleJoueurActuel = new HashSet<PokerAction>();
            }
            #endregion

            #region Initialisation des paramètres de la partie
            PSmallBlind = _smallBlind;
            PBigBlind = _bigBlind;
            PAntes = _antes;
            PCurrentTurnPlayerIndex = -1;
            PHandFinished = false;
            FFIsHandShortStack = null;
            FFBTNFlopGameState = null;
            FFBBFlopGameState = null;
            FFBTNTurnGameState = null;
            FFBBTurnGameState = null;
            FFBTNRiverGameState = null;
            FFBBRiverGameState = null;

            FFIndPremierJoueurAParlerPostflop = -1;
            FFIndDernierJoueurAParler = -1;

            // Création de la liste des joueurs qui n'ont pas foldé au cours d'un tour
            FFLstJoueursPasFold = new List<int>();
            #endregion
            LFInitialiserJoueurs();
        }

        private CGameStateNLHE2Max(CGameStateNLHE2Max _gameStateToClone) : base(_gameStateToClone.PHeroIndex, _gameStateToClone.PCurrentTurnPlayerIndex, _gameStateToClone.PPreviousTurnPlayerIndex, _gameStateToClone.PPot, _gameStateToClone.PBigBlind, null)
        {
            CPlayer btnPlayer = (CPlayer)_gameStateToClone.FFPlayerList[0].Clone();
            CPlayer bbPlayer = (CPlayer)_gameStateToClone.FFPlayerList[1].Clone();

            if (_gameStateToClone == null)
                throw new ArgumentNullException("_gameStateToClone");
            else if (btnPlayer.PNumberOfChipsAtBeginningHand != bbPlayer.PNumberOfChipsAtBeginningHand)
                throw new ArgumentException("The number of chips at the beginning of the hand should be the same!");
            
            void InitializeOtherData()
            {
                
                // Création des joueurs
                FFPlayerList = new CPlayer[2] { btnPlayer, bbPlayer };

                // Création de l'historique des actions des joueurs
                FFActionList = new List<Tuple<CAction, int, Street>>(_gameStateToClone.FFActionList.Capacity);
                foreach (var tupleInfos in _gameStateToClone.FFActionList)
                    FFActionList.Add(new Tuple<CAction, int, Street>((CAction)tupleInfos.Item1.Clone(), tupleInfos.Item2, tupleInfos.Item3));

                // Création de la liste qui indique les actions possible pour un joueur X
                FLstActionsPossibleJoueurActuel = new HashSet<PokerAction>();
                foreach (var action in _gameStateToClone.FLstActionsPossibleJoueurActuel)
                    FLstActionsPossibleJoueurActuel.Add(action);

                FFBBFlopGameState = _gameStateToClone.FFBBFlopGameState?.Clone();
                FFBTNFlopGameState = _gameStateToClone.FFBTNFlopGameState?.Clone();

                FFBBTurnGameState = _gameStateToClone.FFBBTurnGameState?.Clone();
                FFBTNTurnGameState = _gameStateToClone.FFBTNTurnGameState?.Clone();

                FFBBRiverGameState = _gameStateToClone.FFBBRiverGameState?.Clone();
                FFBTNRiverGameState = _gameStateToClone.FFBTNRiverGameState?.Clone();
            }

            #region Initialisation des paramètres de la partie
            PSmallBlind = _gameStateToClone.PSmallBlind;
            PBigBlind = _gameStateToClone.PBigBlind;
            PAntes = _gameStateToClone.PAntes;            
            PLastBet = _gameStateToClone.PLastBet;
            PCurrentTurnPlayerIndex = _gameStateToClone.PCurrentTurnPlayerIndex;
            PHandFinished = _gameStateToClone.PHandFinished;
            FFIsHandShortStack = _gameStateToClone.FFIsHandShortStack;
            FFStadeMain = _gameStateToClone.FFCurrentStreet;            

            FFIndPremierJoueurAParlerPostflop = _gameStateToClone.FFIndPremierJoueurAParlerPostflop;
            FFIndDernierJoueurAParler = _gameStateToClone.FFIndDernierJoueurAParler;

            // Création de la liste des joueurs qui n'ont pas foldé au cours d'un tour
            FFLstJoueursPasFold = new List<int>();
            foreach (var playerIndex in _gameStateToClone.FFLstJoueursPasFold)
                FFLstJoueursPasFold.Add(playerIndex);
            #endregion

            InitializeOtherData();
            FFNextPossibleStates = _gameStateToClone.FFNextPossibleStates;
        }
        

        private void Play()
        {
            UpdateCurrentPlayerAllowedActions();
        }

        public void PlayNewHand()
        {

            int indPremierJoueurAParlerPreflop = 0; // Selects the BTN as the first player to act preflop

            MakeEveryoneAliveAndResetBets();

            FFStadeMain = Street.Preflop;
            PIndPremierJoueurAParlerPostflop = 1; // Selects the BB as the first player to act postflop
            FFIndDernierJoueurAParler = 1; // Selects the BB as the last player to act preflop

            #region Local methods
            void AjouterSmallBlind()
            {
                if (!FFLstJoueursPasFold.Contains(indPremierJoueurAParlerPreflop) || !FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                    throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop et indPremierJoueurAParlerPreflop");

                // Si le joueur a assez de jetons pour mettre le small blind
                if (FFPlayerList[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft >= PSmallBlind)
                {
                    FFPlayerList[indPremierJoueurAParlerPreflop].PLastBet = PSmallBlind;
                    FFPlayerList[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft = (FFPlayerList[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft - PSmallBlind);
                }
                else
                {
                    FFPlayerList[indPremierJoueurAParlerPreflop].PLastBet = FFPlayerList[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft;
                    FFPlayerList[indPremierJoueurAParlerPreflop].PNumberOfChipsLeft = 0;
                }

            }
            void AjouterBigBlind()
            {
                if (!FFLstJoueursPasFold.Contains(PIndPremierJoueurAParlerPostflop))
                    throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndPremierJoueurPostflop");

                // Si le joueur a assez de jetons pour mettre un big blind
                if (FFPlayerList[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft >= PBigBlind)
                {
                    FFPlayerList[PIndPremierJoueurAParlerPostflop].PLastBet = PBigBlind;
                    FFPlayerList[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = (FFPlayerList[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft - PBigBlind);
                }
                else
                {
                    FFPlayerList[PIndPremierJoueurAParlerPostflop].PLastBet = FFPlayerList[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft;
                    FFPlayerList[PIndPremierJoueurAParlerPostflop].PNumberOfChipsLeft = 0;
                }

            }
            void MakeEveryoneAliveAndResetBets()
            {
                FFLstJoueursPasFold.Clear();

                // Tout le monde est vivant.
                for (int indJoueur = 0; indJoueur < FFPlayerList.Length; ++indJoueur)
                {
                    FFLstJoueursPasFold.Add(indJoueur);
                    FFPlayerList[indJoueur].PLastBet = 0; // Ici est la réinitialisation de la mise
                }
            }
            #endregion

            AjouterSmallBlind();
            AjouterBigBlind();

            //PPot += (PSmallBlind + PBigBlind);
            PLastBet = PBigBlind;
            PCurrentTurnPlayerIndex = 0;

            Play();
        }

        private void UpdateCurrentPlayerAllowedActions()
        {
            if (FFLstJoueursPasFold.IndexOf(PCurrentTurnPlayerIndex) == -1)
                throw new InvalidOperationException("L'indice de joueur actuel ne correspond à aucun joueur qui joue en cours!");            

            CPlayer joueurTourActuel = FFPlayerList[PCurrentTurnPlayerIndex];

            if (joueurTourActuel.PNumberOfChipsLeft == 0)            
                throw new InvalidOperationException("Cannot call this method when the current player is all in since he is not allowed to act");                            
            else if (PLastBet > joueurTourActuel.PNumberOfChipsAtBeginningHand)
                throw new InvalidOperationException("This class does not support stacks that are not equal. In other words, this should NEVER happen!");

            FLstActionsPossibleJoueurActuel.Clear();
            FLstActionsPossibleJoueurActuel.Add(PokerAction.Fold);
                        
            // On vérifie si:
            //   1 - Le joueur a déjà mis une mise (situation qui arrive dans le cas où il join une table et il décide de payer le fee pour jouer toute suite, le fee équivalent à un big blind)
            //   2 - Le joueur est big blind (situation qui arrive preflop, lorsque BTN limp)
            //   3 - Le joueur adverse n'a pas misé, donc le joueur présent peut checker
            if (joueurTourActuel.PLastBet == PLastBet)
            {
                FLstActionsPossibleJoueurActuel.Add(PokerAction.Check);
                
                if (FFActionList.Count > 0)
                {
                    var indLastAction = (FFActionList.Count - 1);
                    var lastAction = FFActionList[indLastAction].Item1.PAction;
                    var lastActionStreet = FFActionList[indLastAction].Item3;

                    if (lastActionStreet == PCurrentStreet)
                    {
                        if (lastAction == PokerAction.Check)
                            FLstActionsPossibleJoueurActuel.Add(PokerAction.Bet);
                    }
                    else
                        FLstActionsPossibleJoueurActuel.Add(PokerAction.Bet);
                }                
            }                
            else                
                FLstActionsPossibleJoueurActuel.Add(PokerAction.Call); // On fais l'assumption que s'il pouvait bet/raise avant, c'est que le joueur présent pouvait call (on met les deux stacks au même niveau à la création de la partie)
                        
            // Si la dernière mise ne met pas le joueur présent all in, ça veut dire implicitement que le joueur présent peut aller all in, puisque les stacks doivent être égal au début de la partie.
            if ((FFPlayerList[PCurrentTurnPlayerIndex ^ 1].PNumberOfChipsLeft > 0) && !FLstActionsPossibleJoueurActuel.Contains(PokerAction.Bet))
                FLstActionsPossibleJoueurActuel.Add(PokerAction.Raise);                            
        }
        private bool LastPlayerPlayed()
        {
            return (PCurrentTurnPlayerIndex == FFIndDernierJoueurAParler);
        }

        private int GetNextPlayerIndex()
        {
            return CListHelper.ElemNextOf(FFLstJoueursPasFold, PCurrentTurnPlayerIndex);
        }

        /// <summary>
        /// Event that is called everytime that the something changed in the game (example: A player checked.)
        /// </summary>
        private void GameStateChanged()
        {
            #region Local methods
            bool LFOnePlayerIsAllIn()
            {
                return ((FFPlayerList[0].PNumberOfChipsLeft == 0) || (FFPlayerList[1].PNumberOfChipsLeft == 0));
            }
            void LFChangerStadeTour()
            {
                ++FFStadeMain;
                PLastBet = 0;
            }
            #endregion
            // If everyone folded except one person
            if (FFLstJoueursPasFold.Count == 1)            
                PHandFinished = true;                                               
            // If we are at the end of the street (preflop, flop, turn ou river).
            else if (LastPlayerPlayed())
            {
                if (PCurrentStreet == Street.River)
                {
                    PHandFinished = true;
                    LoadAndGetRiverGameState();

                    int oldIndJoueurActuel = PCurrentTurnPlayerIndex;

                    if (PCurrentTurnPlayerIndex == 0)
                        PCurrentTurnPlayerIndex = 1;
                    else
                        PCurrentTurnPlayerIndex = 0;

                    LoadAndGetRiverGameState();

                    PCurrentTurnPlayerIndex = oldIndJoueurActuel;
                }                                   
                else
                {
                    if (LFOnePlayerIsAllIn())
                    {
                        #region One player is all in and we can be on preflop, flop or the turn.
                        PHandFinished = true;
                        switch(PCurrentStreet)
                        {
                            case Street.Flop:
                                LoadAndGetFlopGameState();
                                break;
                            case Street.Turn:
                                LoadAndGetTurnGameState();
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        #region Change the current street to another street (example: from preflop to flop)
                        var oldStreet = PCurrentStreet;

                        LFChangerStadeTour();

                        bool indCurrentPlayerChanged = !(PCurrentTurnPlayerIndex == PIndPremierJoueurAParlerPostflop);                        

                        PCurrentTurnPlayerIndex = PIndPremierJoueurAParlerPostflop;
                        FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PIndPremierJoueurAParlerPostflop);

                        FFPlayerList[0].PLastBet = 0;
                        FFPlayerList[1].PLastBet = 0;

                        #region Update flop game states, turn game states
                        if (!indCurrentPlayerChanged)
                        {
                            if (PCurrentTurnPlayerIndex == 0)
                                PCurrentTurnPlayerIndex = 1;
                            else
                                PCurrentTurnPlayerIndex = 0;

                            switch (oldStreet)
                            {
                                case Street.Flop:
                                    LoadAndGetFlopGameState();
                                    break;
                                case Street.Turn:
                                    LoadAndGetTurnGameState();
                                    break;
                            }

                            PCurrentTurnPlayerIndex = PIndPremierJoueurAParlerPostflop;
                        }
                        else
                        {
                            switch (oldStreet)
                            {
                                case Street.Flop:
                                    LoadAndGetFlopGameState();
                                    break;
                                case Street.Turn:
                                    LoadAndGetTurnGameState();
                                    break;
                            }
                        }
                        #endregion

                        Play();
                        #endregion
                    }
                }
            }
            else
            {
                // ORDER IS IMPORTANT HERE
                PCurrentTurnPlayerIndex = GetNextPlayerIndex();
                
                switch (PCurrentStreet)
                {
                    case Street.Flop:
                        LoadAndGetFlopGameState();                        
                        break;
                    case Street.Turn:
                        LoadAndGetTurnGameState();
                        break;
                    case Street.River:
                        LoadAndGetRiverGameState();
                        break;
                }

                Play();
            }
        }

        /// <summary>
        /// Event that is called everytime a player made an action.
        /// </summary>
        /// <param name="_action">Action that the player did</param>
        private void ReceivedAction(CAction _action)
        {
            #region Local methods
            void LFSaveActionInHistory()
            {
                // Ajoute à la liste des actions du tour en cours l'action du joueur
                CPlayer currentPlayer = FFPlayerList[PCurrentTurnPlayerIndex];

                FFActionList.Add(new Tuple<CAction, int, Street>(_action, PCurrentTurnPlayerIndex, PCurrentStreet));
            }
            void LFFold()
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(PokerAction.Fold))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Fold!");
                else if (!FFLstJoueursPasFold.Contains(PCurrentTurnPlayerIndex))
                    throw new InvalidOperationException("Vous devez affecter une donnée valide à la donnée membre FIndJoueurActuel");

                FFLstJoueursPasFold.Remove(PCurrentTurnPlayerIndex);
            }
            void LFCheck()
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(PokerAction.Check))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Check!");
            }
            void LFCall()
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(PokerAction.Call))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Call!");

                //PPot = PPot + (_action.PMise - FFPlayerList[PCurrentTurnPlayerIndex].PLastBet);
                FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - (_action.PMise - FFPlayerList[PCurrentTurnPlayerIndex].PLastBet));
                FFPlayerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;
            }
            void LFBet()
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(PokerAction.Bet))
                    throw new InvalidOperationException("The current player is not allowed to bet");
                else if (FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft < _action.PMise)
                    throw new InvalidOperationException("The current player does not have enough chips to bet this amount");

                //PPot = PPot + _action.PMise;
                PLastBet = _action.PMise;
                FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - _action.PMise);
                FFPlayerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PCurrentTurnPlayerIndex);
            }
            void LFRaise()
            {
                if (!FLstActionsPossibleJoueurActuel.Contains(PokerAction.Raise))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Raise!");
                 else if ((FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + FFPlayerList[PCurrentTurnPlayerIndex].PLastBet) < _action.PMise)
                     throw new InvalidOperationException("The current player does not have enough chips to raise this amount");

                //PPot = PPot + (_action.PMise - FFPlayerList[PCurrentTurnPlayerIndex].PLastBet);
                PLastBet = _action.PMise;
                FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - (_action.PMise - FFPlayerList[PCurrentTurnPlayerIndex].PLastBet));
                FFPlayerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;

                FFIndDernierJoueurAParler = CListHelper.ElemPrecedent(FFLstJoueursPasFold, PCurrentTurnPlayerIndex);
            }
            #endregion

            LFSaveActionInHistory();

            #region Execute the action
            switch (_action.PAction)
            {
                case PokerAction.Fold:
                    LFFold();
                    break;
                case PokerAction.Check:
                    LFCheck();
                    break;
                case PokerAction.Call:
                    LFCall();
                    break;
                case PokerAction.Bet:
                    LFBet();
                    break;
                case PokerAction.Raise:
                    LFRaise();
                    break;
            }
            #endregion

            GameStateChanged();
        }

        /// <summary>
        /// Effectuer l'action de fold pour le joueur à qui est le tour de jouer.
        /// </summary>
        public void Fold()
        {
            ReceivedAction(new CAction(PokerAction.Fold));
        }
        public CGameStateNLHE2Max CloneAndFold()
        {
            var newGameState = Clone();
            newGameState.Fold();

            return newGameState;
        }

        public void Check()
        {
            ReceivedAction(new CAction(PokerAction.Check));
        }
        public CGameStateNLHE2Max CloneAndCheck()
        {
            var newGameState = Clone();
            newGameState.Check();

            return newGameState;
        }
        /// <summary>
        /// Effectuer l'action de call pour le joueur à qui est le tour de jouer.
        /// </summary>
        public void Call()
        {
            ReceivedAction(new CAction(PokerAction.Call, PLastBet));
        }
        public CGameStateNLHE2Max CloneAndCall()
        {
            var newGameState = Clone();
            newGameState.Call();

            return newGameState;
        }

        public void Bet(double _betSize)
        {
            double betSize = Math.Round(_betSize, 2);

            if (betSize > FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                ReceivedAction(new CAction(PokerAction.Bet, FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft));
            else
                ReceivedAction(new CAction(PokerAction.Bet, betSize));
        }
        /// <summary>
        /// Effectuer l'action de miser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>                  
        public void Bet(BetSizePossible _betSizeType)
        {
            switch(_betSizeType)
            {
                case BetSizePossible.AllIn:
                case BetSizePossible.AllInShort:
                    Bet(FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft);
                    break;
                case BetSizePossible.Percent100:
                    Bet(PPot);
                    break;
                default:
                    if (!Enum.IsDefined(typeof(BetSizePossible), _betSizeType))
                        throw new ArgumentException();

                    double betAmount = (PPot * PDicBetSize[_betSizeType]);

                    Bet(betAmount);
                    break;
            }
        }
        public CGameStateNLHE2Max CloneAndBet(BetSizePossible _betSize)
        {
            var newGameState = Clone();
            newGameState.Bet(_betSize);

            return newGameState;
        }

        /// <summary>
        /// Effectuer l'action de raiser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>
        public void Raise(double _mise)
        {
            double mise = Math.Round(_mise, 2);
            double allInSize = Math.Round(FFPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + FFPlayerList[PCurrentTurnPlayerIndex].PLastBet, 2);

            if (mise > allInSize)
                ReceivedAction(new CAction(PokerAction.Raise, allInSize));
            else
                ReceivedAction(new CAction(PokerAction.Raise, mise));
        }
        public CGameStateNLHE2Max CloneAndRaise(double _raiseSize)
        {
            var newGameState = Clone();
            newGameState.Raise(_raiseSize);

            return newGameState;
        }

        public CGameStateNLHE2Max Clone()
        {
            ++CApplication.toto;

            return new CGameStateNLHE2Max(this);
        }

        protected override List<AState> GetNextPossibleStates(List<AStateStreetVariants> _variantList)
        {
            throw new NotImplementedException();
        }
    }*/
}
