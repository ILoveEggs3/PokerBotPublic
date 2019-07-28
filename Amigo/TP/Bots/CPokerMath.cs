using Amigo.Controllers;
using Amigo.Helpers;
using Amigo.Models.MyModels.GameState;
using HoldemHand;
using Shared.Models.Database;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Bots
{
    public static class CPokerMath
    {
        const int POSSIBLE_TURN_CARD_COUNT = 47;
        const int POSSIBLE_RIVER_CARD_COUNT = 46;

        public readonly struct ExtendedState
        {
            public readonly double probability;
            public readonly AState state;
            public readonly ulong boardMask;
            public readonly List<(ulong, double)> vilain_range;
            public readonly ulong? turnBoardMask;

            public ExtendedState(double _probability, AState _state, ulong _boardMask, List<(ulong, double)> _vilainRange, ulong? _turnBoardMask = null)
            {
                probability = _probability;
                state = _state;
                boardMask = _boardMask;
                vilain_range = (_vilainRange != null) ? new List<(ulong, double)>(_vilainRange) : null;
                turnBoardMask = _turnBoardMask;
            }
        }

        public readonly struct ExtendedStateList
        {
            public readonly AState initial_state;
            public readonly List<(AState, List<ExtendedState>)> possible_result_state_list;

            public ExtendedStateList(AState _state, List<(AState, List<ExtendedState>)> _resultingStateList) { initial_state = _state; possible_result_state_list = _resultingStateList; }
        }

        static ulong hero_pockets;
        static readonly object o = new object();


        public static (AState, double) GetEv(AState _gameState, List<(ulong, double)> _villainRange, ulong _boardMask, ulong _heroPocketMaskCards, int _currentPlayerIndex, ulong? _t = null)
        {
            lock (o)
            {
                hero_pockets = _heroPocketMaskCards;
                var qwe = GetEVRecursiveV2(_gameState, _villainRange, _boardMask, _currentPlayerIndex, _t);
                //var qwe = GetEVIterative(_gameState, _villainRange, _boardMask, _currentPlayerIndex, null);

                var maxElem = qwe[0].Item1;
                var maxVal = qwe[0].Item2;
                foreach (var item in qwe)
                {
                    if (maxVal < item.Item2)
                    {
                        maxVal = item.Item2;
                        maxElem = item.Item1;
                    }

                }

                return (maxElem, maxVal);
            }
        }

        /*private static List<(AState, double)> GetEVRecursive(AState _gameState, List<(ulong, double)> _vilainRange, ulong _boardMask)
        {
            var ret = new List<(AState, double)>();
            if (_gameState.PIsFinalState)
            {
                ret.Add((_gameState, GetTerminalStateEV(_gameState, _vilainRange, _boardMask)));
                return ret;
            }
            else
            {
                foreach (var gameState in _gameState.PNextPossibleStateList)
                {
                    double currentEV = 0;
                    var lstGameStateAfterVillainAction = GetVilainActionsProbabilites(gameState, _vilainRange, _boardMask);
                    var bStreetChanged = gameState.PCurrentStreet != _gameState.PCurrentStreet;


                    foreach (var gameStateInfosAfterVillainAction in lstGameStateAfterVillainAction)
                    {
                        var gameStateAfterVillainAction = gameStateInfosAfterVillainAction.Item1;
                        var gameStateProbability = gameStateInfosAfterVillainAction.Item2;
                        var villainRange = gameStateInfosAfterVillainAction.Item3;
                        var nextMoveEV = GetEVRecursive(gameStateAfterVillainAction, villainRange, _boardMask, _boardType).Max((x) => x.Item2);
                        currentEV += nextMoveEV * gameStateProbability;
                    }

                    ret.Add((gameState, currentEV));
                }
            }

            return ret;
        }*/

        private static List<(AState, double)> GetEVRecursiveV2(AState _gameState, List<(ulong, double)> _vilainRange, ulong _boardMask, int _currentIndexPlayer, ulong? _turnBoardMask)
        {
            var ret = new List<(AState, double)>();
            if (_gameState.PIsFinalState)
            {
                ret.Add((_gameState, GetTerminalStateEV(_gameState, _vilainRange, _boardMask)));
                return ret;
            }
            else
            {
                //if (_gameState.PCurrentTurnPlayerIndex != _gameState.PHeroIndex)
                    //throw new Exception("Bad");
                

                var states = _gameState.GetNextStatesExtended(_boardMask, hero_pockets, _vilainRange, _currentIndexPlayer, _turnBoardMask);

                if (states.initial_state != _gameState)
                    throw new Exception("Bad");
                if (states.possible_result_state_list.Any(x => x.Item2.Any(y => !y.state.PIsFinalState && (y.state.PCurrentTurnPlayerIndex != _gameState.PCurrentTurnPlayerIndex))))
                {
                    var qwe = states.possible_result_state_list.Where(x => x.Item2.Any(y => !y.state.PIsFinalState && (y.state.PCurrentTurnPlayerIndex != _gameState.PCurrentTurnPlayerIndex)));
                    var qwe2 = qwe.First().Item2.Where(y => !y.state.PIsFinalState && (y.state.PCurrentTurnPlayerIndex != _gameState.PCurrentTurnPlayerIndex)).ToList();
                    throw new Exception("Bad");
                }

                foreach (var extendedState in states.possible_result_state_list)
                {
                    var currentEv = 0.0d;
                    foreach (var resulting_state in extendedState.Item2)
                    {
                        List<(AState, double)> actionList = null;

                        actionList = GetEVRecursiveV2(resulting_state.state, resulting_state.vilain_range, resulting_state.boardMask, _currentIndexPlayer, resulting_state.turnBoardMask);

                        actionList.Sort((x, y) =>
                        {
                            if (x.Item2 < y.Item2) return 1;
                            if (x.Item2 > y.Item2) return -1;
                            return 0;
                        });



                        if (actionList.Count == 0)
                            continue;


                        currentEv += resulting_state.probability * actionList[0].Item2;

                    }
                    ret.Add((extendedState.Item1, currentEv));
                }
            }

            return ret;
        }


        /*private static List<(AState, double)> GetEVIterative(AState _gameState, List<(ulong, double)> _vilainRange, ulong _boardMask, int _currentIndexPlayer, ulong? _turnBoardMask)
        {
            SortedList<double, ExtendedState> state_list = new SortedList<double, ExtendedState>();

            state_list.Add(0, new ExtendedState(1.0d, _gameState, _boardMask, _vilainRange, _turnBoardMask));

            Dictionary<AState, double> value = new Dictionary<AState, double>();
            Dictionary<AState, AState> rev = new Dictionary<AState, AState>();

            while (state_list.Count > 0)
            {
                var current_node = state_list.First();
                var sample_count = current_node.Key;
                var node = current_node.Value;

                state_list.RemoveAt(0);

                var child_list = node.state.GetNextStatesExtendedV2(node.boardMask, hero_pockets, node.vilain_range, node.state.PCurrentTurnPlayerIndex, node.turnBoardMask);

                foreach (var item in child_list.possible_result_state_list)
                {
                    foreach (var item2 in item.Item2)
                    {
                        if (!value.ContainsKey(item2.state))
                        {
                            value.Add(item2.state, 0);
                            rev.Add(item2.state, item.Item1);
                        }
                        if (item2.state.PIsFinalState)
                        {
                            double val = item2.probability * GetTerminalStateEV(item2.state, item2.vilain_range, item2.boardMask);
                            var st = item2.state;
                            while(value.ContainsKey(st))
                            {
                                value[st] += val;
                                st = rev[st];
                            }

                        }
                        else
                        {
                            state_list.Add(item2.probability, item2);
                        }
                    }
                }
            }

            var df = _gameState.PNextPossibleStateList;

            var ret = new List<(AState, double)>();

            foreach (var item in df)
            {
                ret.Add((item, value[item]));
            }

            return ret;
        }
        */

        private static double GetTerminalStateEV(AState _gameState, List<(ulong, double)> _vilainRange, ulong _boardMask)
        {
            if (!_gameState.PIsFinalState)
                throw new ArgumentException("The hand is not finished!");

            double EV = 0;

            int vilainIndex = _gameState.PHeroIndex == 0 ? 1 : 0;
            var vilainPlayer = _gameState.PPlayerList[vilainIndex];
            var heroPlayer = _gameState.PPlayerList[_gameState.PHeroIndex];

            if (_gameState.GetLastAction(vilainIndex).PAction == PokerAction.Fold)
                EV = vilainPlayer.PNumberOfChipsAtBeginningHand - vilainPlayer.PNumberOfChipsLeft;
            else if (_gameState.GetLastAction(_gameState.PHeroIndex).PAction == PokerAction.Fold)
                EV = heroPlayer.PNumberOfChipsLeft - heroPlayer.PNumberOfChipsAtBeginningHand;
            else if (Hand.BitCount(_boardMask) != 5)
            {
                var boardMaskList = Hand.Hands(_boardMask, hero_pockets, 5).ToList();
                double probability = (1.0 / boardMaskList.Count());

                foreach (var boardMask in boardMaskList)
                    EV += probability * GetTerminalStateEV(_gameState, _vilainRange, boardMask); 
            }
            else
            {
                double reward = Math.Min(vilainPlayer.PNumberOfChipsAtBeginningHand - vilainPlayer.PNumberOfChipsLeft,
                                         heroPlayer.PNumberOfChipsAtBeginningHand - heroPlayer.PNumberOfChipsLeft);
                var cumulativeProbability = 0.0d;
                var nbIterations = 0;
                var heroHs = CDBHelperHandInfos.PLstAllBoardsInfos[_gameState.PHeroIndex][_boardMask][hero_pockets].Item1;
                foreach (var infos in _vilainRange)
                {
                    if ((infos.Item1 & _boardMask) != 0)
                        continue;
                    nbIterations++;
                    cumulativeProbability += infos.Item2;

                    var vilainHs = CDBHelperHandInfos.PLstAllBoardsInfos[_gameState.PHeroIndex][_boardMask][infos.Item1].Item1;
                    var diff = heroHs - vilainHs;
                    var prob = infos.Item2;

                    //hack for equal hands
                    if (diff > 0.008)
                        EV += (prob * reward);
                    else if (diff < -0.008)
                        EV -= (prob * reward);

                }
                // case where the vilain range is empty
                if (nbIterations == 0)
                    EV = (Math.Pow(heroHs, 2) - 0.5d) * reward;
                else
                    EV /= cumulativeProbability;
            }
            return EV;
        }

        public static List<(AState, double, List<(ulong, double)>)> GetVilainActionsProbabilites(AState _gameState, BoardMetaDataFlags _boardType, List<(ulong, double)> _vilainRange, ulong _boardMask, int _currentIndexPlayer, ulong? _turnBoardMask = null)
        {
            if (_gameState.PIsFinalState)
                throw new ArgumentException("The hand is finished!");

            long totalCount = 0;
            var ret = new List<(AState, double, List<(ulong, double)>)>();

            var seenKeys = new HashSet<(Street, long, BoardMetaDataFlags, bool?)>();

            foreach (var gameState in _gameState.PNextPossibleStateList)
            {
                var key = gameState.GetStateKey(gameState.PCurrentStreet, _boardType);

                if (seenKeys.Contains(key))
                    continue;
                seenKeys.Add(key);
                if (CDBHelper.PGameStatesStats.ContainsKey(key))
                    totalCount += CDBHelper.PGameStatesStats[key];
            }

            if (totalCount <= 20)
                return ret;
            seenKeys.Clear();
            foreach (var gameState in _gameState.PNextPossibleStateList)
            {

                var key = gameState.GetStateKey(gameState.PCurrentStreet, _boardType);
                if (seenKeys.Contains(key))
                    continue;
                seenKeys.Add(key);
                var actionProbability = ((double)CDBHelper.PGameStatesStats[key] / totalCount);
                List<(ulong, double)> villainRange = null;

                if (gameState.PCurrentStreet != _gameState.PCurrentStreet)
                {
                    if (gameState.PCurrentStreet == Street.Turn)
                        villainRange = GetRange(((CStateTurn)gameState).PFlopStateID, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);
                    else if (gameState.PCurrentStreet == Street.River)
                        villainRange = GetRange(((CStateRiver) gameState).PTurnStateID, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);
                    else
                        throw new Exception("Invalid street! The street should be on the turn or on the river!");
                }
                else if (gameState.GetLastAction().PAction != PokerAction.Fold)
                    villainRange = GetRange(key.Item2, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);

                ret.Add((gameState, actionProbability, villainRange));
            }

            return ret;
        }

        public static List<(AState, double, List<(ulong, double)>)> GetVilainActionsProbabilitesV2(AState _gameState, BoardMetaDataFlags _boardType, List<(ulong, double)> _vilainRange, ulong _boardMask, int _currentIndexPlayer, ulong? _turnBoardMask = null)
        {
            if (_gameState.PIsFinalState)
                throw new ArgumentException("The hand is finished!");

            long totalCount = 0;
            var ret = new List<(AState, double, List<(ulong, double)>)>();

            foreach (var gameState in _gameState.PNextPossibleStateList)
            {
                var key = gameState.GetStateKey(gameState.PCurrentStreet, _boardType);


                if (CDBHelper.PGameStatesStats.ContainsKey(key))
                    totalCount += CDBHelper.PGameStatesStats[key];
            }

            if (totalCount <= 20)
                return ret;

            foreach (var gameState in _gameState.PNextPossibleStateList)
            {
                var key = gameState.GetStateKey(gameState.PCurrentStreet, _boardType);
                var actionProbability = (double)CDBHelper.PGameStatesStats[key];
                List<(ulong, double)> villainRange = null;

                if (gameState.PCurrentStreet != _gameState.PCurrentStreet)
                {
                    if (gameState.PCurrentStreet == Street.Turn)
                        villainRange = GetRange(((CStateTurn)gameState).PFlopStateID, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);
                    else if (gameState.PCurrentStreet == Street.River)
                        villainRange = GetRange(((CStateRiver)gameState).PTurnStateID, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);
                    else
                        throw new Exception("Invalid street! The street should be on the turn or on the river!");
                }
                else if (gameState.GetLastAction().PAction != PokerAction.Fold)
                    villainRange = GetRange(key.Item2, _vilainRange, gameState, _boardMask, _gameState.PCurrentStreet, _currentIndexPlayer, _turnBoardMask);

                ret.Add((gameState, actionProbability, villainRange));
            }

            return ret;
        }
        // Does not support MT
        public static List<(ulong, double)> GetRange(long _stateID, List<(ulong, double)> _previousVilainRange, AState _gameState, ulong _boardMask, Street _actionStreet, int _currentIndexPlayer, ulong? _turnBoardMask = null)
        {
            var _boardType = CDBHelperHandInfos.PDicAllBoardsByBoardMask[_boardMask].Item2;
            if (_actionStreet == Street.Flop)
                return CBotPokerAmigo.GetRangeFlop(_stateID, _boardType, _previousVilainRange, _boardMask, _gameState.PCurrentTurnPlayerIndex);
            else if (_actionStreet == Street.Turn)
                return CBotPokerAmigo.GetRangeTurn(_stateID, _boardType, _previousVilainRange, _boardMask, _gameState.PCurrentTurnPlayerIndex);
            else
            {
                if (!_turnBoardMask.HasValue)
                    throw new InvalidOperationException("Must have a turn board mask!");

                return CBotPokerAmigo.GetRangeRiver(_stateID, _boardType, _previousVilainRange, _turnBoardMask.Value, _boardMask, _gameState.PCurrentTurnPlayerIndex);
            }
        }

        #region JO
        public static (PokerAction, long?) ParseLastBetSize(IList<(CAction, int, Street)> _actionList, IList<CPlayer> _playersList, TypesPot _filteredPotType, bool _isShortStack, double _pot)
        {
            if (_actionList.Count < 3)
                throw new InvalidOperationException("There should be atleast 3 actions in the list (we should be on the flop)");

            var lastActionFiltered = GetFilteredActionFromLastAction(_actionList).PAction;

            switch (lastActionFiltered)
            {
                case PokerAction.Fold:
                case PokerAction.Check:
                case PokerAction.ReRaise:
                    return (lastActionFiltered, null);
                case PokerAction.Bet:
                    return (lastActionFiltered, (long?)GetClosestBetSizeFromLastAction(_actionList, _isShortStack, _pot));
                case PokerAction.Call:
                    return (lastActionFiltered, (long?)GetClosestBetSizeFromLastCallAction(_actionList, _isShortStack, _pot));
                case PokerAction.CallVsRaise:
                    switch (_filteredPotType)
                    {
                        case TypesPot.TwoBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastCallActionTwoBetPot(_actionList, _filteredPotType, _playersList, _isShortStack));
                        case TypesPot.ThreeBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastCallActionThreeBetPot(_actionList, _filteredPotType, _playersList, _isShortStack));
                        case TypesPot.FourBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastCallActionFourBetPot(_actionList, _filteredPotType, _playersList));
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                case PokerAction.CallVsReRaise:
                    return (lastActionFiltered, (long?)GetClosestReRaiseCallSizeFromLastAction(_actionList, _playersList, _isShortStack));
                case PokerAction.Raise:
                    switch (_filteredPotType)
                    {
                        case TypesPot.TwoBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastActionTwoBetPot(_actionList, _filteredPotType, _playersList, _isShortStack));
                        case TypesPot.ThreeBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastActionThreeBetPot(_actionList, _filteredPotType, _playersList, _isShortStack));
                        case TypesPot.FourBet:
                            return (lastActionFiltered, (long?)GetClosestRaiseSizeFromLastActionFourBetPot(_actionList, _filteredPotType, _playersList));
                        default:
                            throw new InvalidOperationException("Invalid type pot");
                    }
                default:
                    throw new InvalidOperationException("Invalid action");
            }
        }

        private static BetSizePossible GetClosestBetSizeFromLastAction(IList<(CAction, int, Street)> _actionList, bool _isShortStack, double _pot)
        {
            int indLastAction = (_actionList.Count - 1);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            PokerAction lastAction = lastActionObject.PAction;

            if (lastAction != PokerAction.Bet)
                throw new InvalidOperationException("Action is not supported");

            double bet = lastActionObject.PMise;
            double potBeforeTheBet = (_pot - bet);

            if (bet > (potBeforeTheBet * 1.33))
            {
                if (_isShortStack)
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
        private static BetSizePossible GetClosestBetSizeFromLastCallAction(IList<(CAction, int, Street)> _actionList, bool _isShortStack, double _pot)
        {
            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction lastLastActionObject = _actionList[indLastLastAction].Item1;

            if (!(lastActionObject.PAction == PokerAction.Call && lastLastActionObject.PAction == PokerAction.Bet))
                throw new InvalidOperationException("Invalid action. The last action must be a call and the action before the last action must be a bet");

            double callSize = lastActionObject.PMise;
            double potBeforeTheBetAndTheCall = (_pot - lastActionObject.PMise - lastActionObject.PMise);

            if (callSize > (potBeforeTheBetAndTheCall * 1.33))
            {
                if (_isShortStack)
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

        private static RaiseSizePossibleTwoBetPot GetClosestRaiseSizeFromLastActionTwoBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers, bool _isShortStack)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.TwoBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (_isShortStack)
                        return RaiseSizePossibleTwoBetPot.AllInShort;
                    else
                        return RaiseSizePossibleTwoBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn;
            }
        }
        private static RaiseSizePossibleTwoBetPot GetClosestRaiseSizeFromLastCallActionTwoBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers, bool _isShortStack)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.TwoBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            int indLastLastLastAction = (_actionList.Count - 3);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = _actionList[indLastLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (_isShortStack)
                        return RaiseSizePossibleTwoBetPot.AllInShort;
                    else
                        return RaiseSizePossibleTwoBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn;
            }
        }

        private static RaiseSizePossibleThreeBetPot GetClosestRaiseSizeFromLastActionThreeBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers, bool _isShortStack)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.ThreeBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (_isShortStack)
                        return RaiseSizePossibleThreeBetPot.AllInShort;
                    else
                        return RaiseSizePossibleThreeBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn;
            }
        }
        private static RaiseSizePossibleThreeBetPot GetClosestRaiseSizeFromLastCallActionThreeBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers, bool _isShortStack)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.ThreeBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot.");

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            int indLastLastLastAction = (_actionList.Count - 3);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = _actionList[indLastLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                {
                    if (_isShortStack)
                        return RaiseSizePossibleThreeBetPot.AllInShort;
                    else
                        return RaiseSizePossibleThreeBetPot.AllIn;
                }
                else
                    return RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn;
            }
        }

        private static RaiseSizePossibleFourBetPot GetClosestRaiseSizeFromLastActionFourBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.FourBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot." + typePot.ToString());

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                    return RaiseSizePossibleFourBetPot.AllIn;
                else
                    return RaiseSizePossibleFourBetPot.AnySizingExceptAllIn;
            }
        }
        private static RaiseSizePossibleFourBetPot GetClosestRaiseSizeFromLastCallActionFourBetPot(IList<(CAction, int, Street)> _actionList, TypesPot _filteredPotType, IList<CPlayer> _lstPlayers)
        {
            TypesPot typePot = _filteredPotType;

            if (typePot != TypesPot.FourBet)
                throw new InvalidOperationException("The type of the pot is not supported. Call the method associated to the current type of pot." + typePot.ToString());

            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            int indLastLastLastAction = (_actionList.Count - 3);
            CAction lastActionObject = _actionList[indLastAction].Item1;
            CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;
            CAction actionBeforeBeforeLastActionObject = _actionList[indLastLastLastAction].Item1;
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
                CPlayer playerAssociatedToLastAction = _lstPlayers[_actionList[indLastAction].Item2];

                if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
                    return RaiseSizePossibleFourBetPot.AllIn;
                else
                    return RaiseSizePossibleFourBetPot.AnySizingExceptAllIn;
            }
        }

        private static CallSizePossibleVsReRaise GetClosestReRaiseCallSizeFromLastAction(IList<(CAction, int, Street)> _actionList, IList<CPlayer> _lstPlayers, bool _isShortStack)
        {
            int indLastAction = (_actionList.Count - 1);
            int indLastLastAction = (_actionList.Count - 2);
            int indLastLastLastAction = (_actionList.Count - 3);

            var tupleInfos = _actionList[indLastAction];
            CAction lastActionObject = tupleInfos.Item1;
            CAction lastLastActionObject = _actionList[indLastLastAction].Item1;
            CAction lastLastLastActionObject = _actionList[indLastLastLastAction].Item1;

            if (!(lastActionObject.PAction == PokerAction.Call && lastLastActionObject.PAction == PokerAction.Raise && lastLastLastActionObject.PAction == PokerAction.Raise))
                throw new InvalidOperationException("The last action must be a call and the two actions after must be a raise, otherwise it is not a call vs a re-raise!");

            CPlayer playerAssociatedToLastAction = _lstPlayers[tupleInfos.Item2];

            if (playerAssociatedToLastAction.PNumberOfChipsLeft == 0)
            {
                if (_isShortStack)
                    return CallSizePossibleVsReRaise.AllInShort;
                else
                    return CallSizePossibleVsReRaise.AllIn;
            }
            else
                return CallSizePossibleVsReRaise.AnySizingExceptAllIn;
        }

        private static CAction GetFilteredActionFromLastAction(IList<(CAction, int, Street)> _actionList)
        {
            int indLastAction = (_actionList.Count - 1);
            CAction lastActionObject = _actionList[indLastAction].Item1;

            switch (lastActionObject.PAction)
            {
                case PokerAction.Raise:
                    if (_actionList.Count < 3)
                        throw new InvalidOperationException("We can only call this function on flop, turn or river");

                    int indLastLastAction = (_actionList.Count - 2);
                    CAction actionBeforeLastActionObject = _actionList[indLastLastAction].Item1;

                    if (actionBeforeLastActionObject.PAction == PokerAction.Raise)
                        return new CAction(PokerAction.ReRaise);
                    else
                        return lastActionObject;
                case PokerAction.Call:
                    if (_actionList.Count < 3)
                        throw new InvalidOperationException("We can only call this function on flop, turn or river");

                    int indLastLastAction2 = (_actionList.Count - 2);
                    CAction actionBeforeLastActionObject2 = _actionList[indLastLastAction2].Item1;
                    Street validStreet2 = _actionList[indLastAction].Item3;
                    bool actionWasOnSameStreet2 = (_actionList[indLastLastAction2].Item3 == validStreet2);

                    if ((actionBeforeLastActionObject2.PAction == PokerAction.Raise) && actionWasOnSameStreet2)
                    {
                        int indLastLastLastAction = (_actionList.Count - 3);
                        CAction actionBeforeBeforeLastActionObject = _actionList[indLastLastLastAction].Item1;
                        bool actionWasOnSameStreet3 = (_actionList[indLastLastLastAction].Item3 == validStreet2); // Because we just called, so the street just changed.

                        if ((actionBeforeBeforeLastActionObject.PAction == PokerAction.Raise) && actionWasOnSameStreet3)
                            return new CAction(PokerAction.CallVsReRaise, lastActionObject.PMise);
                        else
                            return new CAction(PokerAction.CallVsRaise, lastActionObject.PMise);
                    }
                    else
                        return lastActionObject;
                default:
                    if (lastActionObject.PAction == PokerAction.None)
                        throw new InvalidOperationException("Invalid action");
                    else
                        return lastActionObject;
            }
        }
        #endregion
    }
}
