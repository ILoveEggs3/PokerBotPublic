using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amigo.Bots;
using Amigo.Models.MyModels.GameState;
using Shared.Poker.Helpers;
using Shared.Poker.Models;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;
using static Amigo.Bots.CPokerMath;
using HoldemHand;
using Shared.Models.Database;
using Amigo.Helpers;
using static Amigo.Helpers.CDBHelper;

namespace Amigo.Controllers
{
    public abstract class AState : IState
    {

        public static readonly Dictionary<TypesPot, TypesPot> PDicFilteredTypesPot = new Dictionary<TypesPot, TypesPot>() { { TypesPot.OneBet, TypesPot.OneBet },
                                                                                                                            { TypesPot.TwoBet, TypesPot.TwoBet },
                                                                                                                            { TypesPot.ThreeBet, TypesPot.ThreeBet },
                                                                                                                            { TypesPot.FourBet, TypesPot.FourBet },
                                                                                                                            { TypesPot.FiveBetEtPlus, TypesPot.FourBet },
                                                                                                                            { TypesPot.Limped, TypesPot.TwoBet },
                                                                                                                            { TypesPot.RaisedLimped, TypesPot.TwoBet },
                                                                                                                            { TypesPot.LimpedThreeBet, TypesPot.FourBet },
                                                                                                                            { TypesPot.LimpedFourBetEtPlus, TypesPot.FourBet } };

        public const int BB_PLAYER_INDEX = 1;
        public const int BTN_PLAYER_INDEX = 0;

        public struct AStateStreetVariants
        {
            public List<(CAction, int, Street)> actionList;
            public List<CPlayer> playerList;
            public int currentPlayerTurnIndex;
            public double pot;
            public long? gs;
        }

        private readonly List<CPlayer> FFPlayerList;
        private readonly List<(CAction, int, Street)> FFActionList;
        private Lazy<HashSet<PokerAction>> FFCurrentPlayerPossibleActionSet { get; }
        private Lazy<TypesPot> FFTypeFilteredPot { get; }
        private Lazy<bool> FFIsFinalState { get; }
        private Lazy<List<AState>> FFNextPossibleStateList { get; }
        private Lazy<TypesPot> FFTypePot { get; }
        private Lazy<long> FFGameState { get; }
        public long PGameState => FFGameState.Value;
        public double PPot { get; }
        public double PBigBlind { get; }
        public int PHeroIndex { get; }
        public int PCurrentTurnPlayerIndex { get; }
        public IList<(CAction, int, Street)> PActionList => FFActionList.AsReadOnly();
        public IList<CPlayer> PPlayerList => FFPlayerList.AsReadOnly();

        protected AState(List<CPlayer> _playerList, List<(CAction, int, Street)> _actionList, int _heroIndex, int _currentPlayerTurnIndex, double _pot, double _bigBlind)
        {
            FFPlayerList = new List<CPlayer>() { _playerList[0].Clone(), _playerList[1].Clone() };
            FFActionList = new List<(CAction, int, Street)>();
            FFActionList.AddRange(_actionList);
            FFCurrentPlayerPossibleActionSet = new Lazy<HashSet<PokerAction>>(() => GetUpdatedCurrentPlayerAllowedActions());
            FFIsFinalState = new Lazy<bool>(() => IsFinalState());
            FFNextPossibleStateList = new Lazy<List<AState>>(() => GetNextPossibleStates());
            FFTypePot = new Lazy<TypesPot>(() => GetTypePot());
            FFTypeFilteredPot = new Lazy<TypesPot>(() => GetFilteredTypePot());
            FFGameState = new Lazy<long>(() => LoadGameState());
            PHeroIndex = _heroIndex;
            PCurrentTurnPlayerIndex = _currentPlayerTurnIndex;
            PPot = _pot;
            PBigBlind = _bigBlind;
        }

        protected AState(AState _state, AStateStreetVariants _variant)
        {
            FFNextPossibleStateList = new Lazy<List<AState>>(() => GetNextPossibleStates());
            FFTypePot = new Lazy<TypesPot>(() => GetTypePot());
            FFTypeFilteredPot = new Lazy<TypesPot>(() => GetFilteredTypePot());
            FFCurrentPlayerPossibleActionSet = new Lazy<HashSet<PokerAction>>(() => GetUpdatedCurrentPlayerAllowedActions());
            FFIsFinalState = new Lazy<bool>(() => IsFinalState());
            if (_variant.gs == null)
            {
                FFGameState = new Lazy<long>(() => LoadGameState());
            }
            else
            {
                FFGameState = new Lazy<long>(() => (long)_variant.gs);
            }
            PHeroIndex = _state.PHeroIndex;
            PBigBlind = _state.PBigBlind;

            FFActionList = _variant.actionList;
            FFPlayerList = _variant.playerList;
            PCurrentTurnPlayerIndex = _variant.currentPlayerTurnIndex;
            PPot = _variant.pot;
        }

        protected HashSet<PokerAction> PCurrentPlayerPossibleActionSet { get { return FFCurrentPlayerPossibleActionSet.Value; } }
        public int PFirstPlayerIndexPreflop { get { return BTN_PLAYER_INDEX; } }
        public int PFirstPlayerIndexPostFlop { get { return BB_PLAYER_INDEX; } }
        public int PVilainIndex { get { return PHeroIndex == 1 ? 0 : 1; } }
        public int PPreviousTurnPlayerIndex { get { return PCurrentTurnPlayerIndex == 1 ? 0 : 1; } }
        public double PLastBet { get { return GetLastAction(PPreviousTurnPlayerIndex).PMise; } }
        public List<AState> PNextPossibleStateList { get { return FFNextPossibleStateList.Value; } }
        public TypesPot PTypeFilteredPot { get { return FFTypeFilteredPot.Value; } }
        public Street PCurrentStreet { get { return FFCurrentStreet; } }
        public TypesPot PTypePot { get { return FFTypePot.Value; } }
        public bool PIsFinalState { get { return FFIsFinalState.Value; } }

        protected abstract AState CreateNewFromStreetVariant(AStateStreetVariants _variant);
        public abstract bool IsFinalState();
        protected abstract Street FFCurrentStreet { get; }
        public bool PIsHandShortStack { get => (Math.Min(PPlayerList[1].PNumberOfChipsAtBeginningHand, PPlayerList[0].PNumberOfChipsAtBeginningHand).ToBB(PBigBlind) < 60); }

        public CAction GetLastAction(int _playerIndex)
        {
            return PActionList.Last(x => x.Item2 == _playerIndex).Item1;
        }
        public CAction GetLastAction()
        {
            return GetLastAction(PPreviousTurnPlayerIndex);
        }
        public Street GetLastActionStreet()
        {
            return PActionList.Last().Item3;
        }
        public CPlayer GetBBPlayer()
        {
            return PPlayerList[1];
        }
        public CPlayer GetHeroPlayer()
        {
            return PPlayerList[PHeroIndex];
        }
        public CPlayer GetVilainPlayer()
        {
            return PPlayerList[PVilainIndex];
        }

        public PokerPosition GetCurrentPlayerPosition()
        {
            if (PCurrentTurnPlayerIndex == PFirstPlayerIndexPostFlop)
                return PokerPosition.BB;
            else
                return PokerPosition.BTN;
        }
        public PokerPosition GetVilainPlayerPosition()
        {
            // If hero is BB, that means villain is BTN and vice-versa.
            if (PCurrentTurnPlayerIndex == PFirstPlayerIndexPostFlop)
                return PokerPosition.BTN;
            else
                return PokerPosition.BB;
        }

        private HashSet<PokerAction> GetUpdatedCurrentPlayerAllowedActions()
        {
            CPlayer joueurTourActuel = PPlayerList[PCurrentTurnPlayerIndex];
            var possibleActionSet = new HashSet<PokerAction>();
            if ((joueurTourActuel.PNumberOfChipsLeft - 0.01) <= 0)
                return possibleActionSet;

            possibleActionSet.Add(PokerAction.Fold);

            // CASES: PREFLOP, POSTFLOP
            // CASE: PREFLOP
            //      CurrentPlayer is BB and PreviousPlayer limped
            // CASE: POSTFLOP
            //      PreviousPlayer checked
            if ((Math.Abs(joueurTourActuel.PLastBet - PPlayerList[PPreviousTurnPlayerIndex].PLastBet) - 0.01) <= 0)
            {
                possibleActionSet.Add(PokerAction.Check);
                possibleActionSet.Remove(PokerAction.Fold);

                if (PActionList.Count > 0)
                {
                    var indLastAction = (PActionList.Count - 1);
                    var lastAction = PActionList[indLastAction].Item1.PAction;
                    var lastActionStreet = PActionList[indLastAction].Item3;

                    if (lastActionStreet == PCurrentStreet)
                    {
                        if (lastAction == PokerAction.Check)
                            possibleActionSet.Add(PokerAction.Bet);
                    }
                    else
                        possibleActionSet.Add(PokerAction.Bet);
                }
            }
            else
                possibleActionSet.Add(PokerAction.Call); // On fais l'assumption que s'il pouvait bet/raise avant, c'est que le joueur présent pouvait call (on met les deux stacks au même niveau à la création de la partie)

            // Si la dernière mise ne met pas le joueur présent all in, ça veut dire implicitement que le joueur présent peut aller all in, puisque les stacks doivent être égal au début de la partie.
            if (((PPlayerList[PCurrentTurnPlayerIndex ^ 1].PNumberOfChipsLeft + 0.01) > 0) && !possibleActionSet.Contains(PokerAction.Bet))
                possibleActionSet.Add(PokerAction.Raise);
            return possibleActionSet;
        }
        private AStateStreetVariants ReceivedAction(CAction _action)
        {
            var actionList = new List<(CAction, int, Street)>();
            actionList.AddRange(PActionList);
            var playerList = new List<CPlayer>();
            foreach (var player in PPlayerList) { playerList.Add(player.Clone()); }

            var pot = PPot;

            #region Local methods
            void LFFold()
            {
                if (!PCurrentPlayerPossibleActionSet.Contains(PokerAction.Fold))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Fold!");
            }
            void LFCheck()
            {
                if (!PCurrentPlayerPossibleActionSet.Contains(PokerAction.Check))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Check!");
            }
            void LFCall()
            {
                if (!PCurrentPlayerPossibleActionSet.Contains(PokerAction.Call))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Call!");

                pot = PPot + (_action.PMise - PPlayerList[PCurrentTurnPlayerIndex].PLastBet);
                playerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - (_action.PMise - PPlayerList[PCurrentTurnPlayerIndex].PLastBet));
                playerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;
            }
            void LFBet()
            {
                if (!PCurrentPlayerPossibleActionSet.Contains(PokerAction.Bet))
                    throw new InvalidOperationException("The current player is not allowed to bet");
                else if (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft < _action.PMise)
                    throw new InvalidOperationException("The current player does not have enough chips to bet this amount");

                pot = PPot + _action.PMise;
                playerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - _action.PMise);
                playerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;
            }
            void LFRaise()
            {
                if (!PCurrentPlayerPossibleActionSet.Contains(PokerAction.Raise))
                    throw new InvalidOperationException("Le joueur n'a pas le droit de faire un Raise!");
                else if (((PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet) - _action.PMise) < -0.01)
                    throw new InvalidOperationException("The current player does not have enough chips to raise this amount");

                pot = PPot + (_action.PMise - PPlayerList[PCurrentTurnPlayerIndex].PLastBet);
                playerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft = (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft - (_action.PMise - PPlayerList[PCurrentTurnPlayerIndex].PLastBet));
                playerList[PCurrentTurnPlayerIndex].PLastBet = _action.PMise;
            }
            #endregion

            actionList.Add((_action, PCurrentTurnPlayerIndex, PCurrentStreet));

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
            long? gs = null;
            if (_action.PAction == PokerAction.Fold && PCurrentStreet != Street.Preflop)
                gs = PGameState;

            return new AStateStreetVariants() { playerList = playerList, actionList = actionList, currentPlayerTurnIndex = PPreviousTurnPlayerIndex, pot = pot, gs = gs  };

        }
        public List<PokerAction> GetLstAllowedActionsForCurrentPlayer()
        {
            if (!PIsFinalState)
            {
                if (PCurrentPlayerPossibleActionSet.Contains(PokerAction.Check))
                {
                    var lstAllowedActionsCurrentPlayer = PCurrentPlayerPossibleActionSet.ToList();

                    lstAllowedActionsCurrentPlayer.Remove(PokerAction.Fold);

                    return lstAllowedActionsCurrentPlayer;
                }
                else
                    return PCurrentPlayerPossibleActionSet.ToList();
            }
            else
                return new List<PokerAction>();
        }

        public CAction GetExtendedActionFromLastAction()
        {
            var lastAction = PActionList.Last();

            if ((lastAction.Item1.PAction & (PokerAction.Fold | PokerAction.Check | PokerAction.Bet)) != 0)
                return lastAction.Item1;
            else
            {
                var raiseCount = PActionList.Reverse().TakeWhile(x => x.Item3 == lastAction.Item3).LongCount(x => x.Item1.PAction == PokerAction.Raise);

                if (lastAction.Item1.PAction == PokerAction.Call)
                {
                    if (raiseCount == 0)
                        return new CAction(PokerAction.Call, lastAction.Item1.PMise);
                    if (raiseCount == 1)
                        return new CAction(PokerAction.CallVsRaise, lastAction.Item1.PMise);
                    else
                        return new CAction(PokerAction.CallVsReRaise, lastAction.Item1.PMise);
                }
                else
                {
                    if (raiseCount == 1)
                        return new CAction(PokerAction.Raise, lastAction.Item1.PMise);
                    else
                        return new CAction(PokerAction.ReRaise, lastAction.Item1.PMise);
                }
            }
        }

        public ExtendedStateList GetNextStatesExtended(ulong _boardMask, ulong _pocketMask, List<(ulong, double)> _vilainRange, int _currentIndexPlayer, ulong? _turnBoardMask)
        {
            var extended_state_list = new List<(AState, List<ExtendedState>)>();

            var initialPlayerIndex = PCurrentTurnPlayerIndex;
            var initialStreet = PCurrentStreet;

            var seenKeys = new HashSet<(Street, long, BoardMetaDataFlags, bool?)>();
            foreach (var stateAfterAction in GetNextPossibleStates())
            {
                var action_result_states = new List<ExtendedState>();
                if (stateAfterAction.PIsFinalState)
                {
                    action_result_states.Add(new ExtendedState(1.0d, stateAfterAction, _boardMask, _vilainRange));
                }
                else
                {
                    if (stateAfterAction.PCurrentStreet != initialStreet)
                    {

                        var deadCards = (_boardMask | _pocketMask);
                        var qwe = Hand.Hands(0, deadCards, 1).GroupBy(x => CDBHelperHandInfos.PDicAllBoardsByBoardMask[(x | _boardMask)].Item2).ToList();

                        foreach (var new_street_card in qwe)
                        {

                            var card = new_street_card.First();
                            var new_street_mask = (_boardMask | card);
                            var board_meta_data = CalculateMetaData(new_street_mask);
                            var key = stateAfterAction.GetStateKey(stateAfterAction.PCurrentStreet, board_meta_data);
                            if (seenKeys.Contains(key))
                                continue;
                            seenKeys.Add(key);
                            var probability = 1.0d;
                            var vilain_range = CBotPokerAmigo.UpdateRange(_vilainRange, card);
                            if (PCurrentStreet == Street.Turn)
                                _turnBoardMask = new_street_mask;
                            if (stateAfterAction.PCurrentTurnPlayerIndex == initialPlayerIndex)
                            {
                                action_result_states.Add(new ExtendedState(probability, stateAfterAction, new_street_mask, vilain_range, _turnBoardMask));
                            }
                            else
                            {
                                var vilainActionPossibleActions = GetVilainActionsProbabilites(stateAfterAction, board_meta_data, vilain_range, new_street_mask, _currentIndexPlayer, _turnBoardMask);
                                foreach (var state_after_vilain in vilainActionPossibleActions)
                                {
                                    var probability_leaf = probability * state_after_vilain.Item2;

                                    action_result_states.Add(new ExtendedState(probability_leaf, state_after_vilain.Item1, new_street_mask, state_after_vilain.Item3, _turnBoardMask));
                                }
                            }
                        }
                    }
                    else
                    {
                        var board_meta_data = CalculateMetaData(_boardMask);
                        var key = stateAfterAction.GetStateKey(stateAfterAction.PCurrentStreet, board_meta_data);
                        if (seenKeys.Contains(key))
                            continue;
                        seenKeys.Add(key);
                        var vilainActionPossibleActions = GetVilainActionsProbabilites(stateAfterAction, board_meta_data, _vilainRange, _boardMask, _currentIndexPlayer, _turnBoardMask);
                        foreach (var state_after_vilain in vilainActionPossibleActions)
                        {
                            var probability = state_after_vilain.Item2;

                            if (state_after_vilain.Item1.PCurrentStreet != initialStreet)
                            {

                                var deadCards = (_boardMask | _pocketMask);
                                var qwe = Hand.Hands(0, deadCards, 1).GroupBy(x => CDBHelperHandInfos.PDicAllBoardsByBoardMask[(x | _boardMask)].Item2).ToList();

                                foreach (var new_street_card in qwe) 
                                {
                                    var card = new_street_card.First();
                                    var probability_leaf = probability;
                                    var new_street_mask = (_boardMask | card);
                                    var vilain_range = CBotPokerAmigo.UpdateRange(state_after_vilain.Item3, card);
                                    if (state_after_vilain.Item1.PCurrentStreet == Street.River)
                                        _turnBoardMask = new_street_mask;

                                    if ((state_after_vilain.Item1.PCurrentTurnPlayerIndex != initialPlayerIndex) && !state_after_vilain.Item1.PIsFinalState)
                                    {
                                        
                                        var board_meta_data2 = CalculateMetaData(new_street_mask);
                                        var vilainActionPossibleActions2 = GetVilainActionsProbabilites(state_after_vilain.Item1, board_meta_data2, vilain_range, new_street_mask, _currentIndexPlayer, _turnBoardMask);
                                        foreach (var vilainSecondAction in vilainActionPossibleActions2)
                                        {
                                            var probabilityVilainAction2 = vilainSecondAction.Item2 * probability_leaf;
                                            action_result_states.Add(new ExtendedState(probabilityVilainAction2, vilainSecondAction.Item1, new_street_mask, vilainSecondAction.Item3, _turnBoardMask));
                                        }
                                    }
                                    else
                                    {
                                        action_result_states.Add(new ExtendedState(probability_leaf, state_after_vilain.Item1, new_street_mask, vilain_range, _turnBoardMask));
                                    }
                                }
                            }
                            else
                            {
                                action_result_states.Add(new ExtendedState(probability, state_after_vilain.Item1, _boardMask, state_after_vilain.Item3, _turnBoardMask));
                            }
                        }
                    }
                }
                extended_state_list.Add((stateAfterAction, action_result_states));
            }

            return new ExtendedStateList(this, extended_state_list);
        }

        /*public ExtendedStateList GetNextStatesExtendedV2(ulong _boardMask, ulong _pocketMask, List<(ulong, double)> _vilainRange, int _currentIndexPlayer, ulong? _turnBoardMask)
        {
            var extended_state_list = new List<(AState, List<ExtendedState>)>();

            var initialPlayerIndex = PCurrentTurnPlayerIndex;
            var initialStreet = PCurrentStreet;
            var q = CalculateMetaData(_boardMask);
            foreach (var state_with_sample_count in GetNextPossibleStates(q))
            {
                var stateAfterAction = state_with_sample_count.Item1;
                var state_count = state_with_sample_count.Item2;
                var action_result_states = new List<ExtendedState>();
                if (stateAfterAction.PIsFinalState)
                {
                    action_result_states.Add(new ExtendedState(state_count, stateAfterAction, _boardMask, _vilainRange));
                }
                else
                {
                    if (stateAfterAction.PCurrentStreet != initialStreet)
                    {

                        var deadCards = (_boardMask | _pocketMask);
                        var qwe = Hand.Hands(0, deadCards, 1).GroupBy(x => CDBHelperHandInfos.PDicAllBoardsByBoardMask[(x | _boardMask)].Item2).ToList();

                        foreach (var new_street_card in qwe)
                        {
                            var card = new_street_card.First();
                            var new_street_mask = (_boardMask | card);
                            var board_meta_data = CalculateMetaData(new_street_mask);
                            var probability = state_count;
                            var vilain_range = CBotPokerAmigo.UpdateRange(_vilainRange, card);
                            if (PCurrentStreet == Street.Turn)
                                _turnBoardMask = new_street_mask;
                            if (stateAfterAction.PCurrentTurnPlayerIndex == initialPlayerIndex)
                            {
                                action_result_states.Add(new ExtendedState(probability, stateAfterAction, new_street_mask, vilain_range, _turnBoardMask));
                            }
                            else
                            {
                                var vilainActionPossibleActions = GetVilainActionsProbabilitesV2(stateAfterAction, board_meta_data, vilain_range, new_street_mask, _currentIndexPlayer, _turnBoardMask);
                                foreach (var state_after_vilain in vilainActionPossibleActions)
                                {
                                    var probability_leaf = state_after_vilain.Item2;

                                    action_result_states.Add(new ExtendedState(probability_leaf, state_after_vilain.Item1, new_street_mask, state_after_vilain.Item3, _turnBoardMask));
                                }
                            }
                        }
                    }
                    else
                    {
                        var board_meta_data = CalculateMetaData(_boardMask);
                        var vilainActionPossibleActions = GetVilainActionsProbabilitesV2(stateAfterAction, board_meta_data, _vilainRange, _boardMask, _currentIndexPlayer, _turnBoardMask);
                        foreach (var state_after_vilain in vilainActionPossibleActions)
                        {
                            var probability = state_after_vilain.Item2;

                            if (state_after_vilain.Item1.PCurrentStreet != initialStreet)
                            {

                                var deadCards = (_boardMask | _pocketMask);
                                var qwe = Hand.Hands(0, deadCards, 1).GroupBy(x => CDBHelperHandInfos.PDicAllBoardsByBoardMask[(x | _boardMask)].Item2).ToList();

                                foreach (var new_street_card in qwe)
                                {
                                    var card = new_street_card.First();
                                    var probability_leaf = probability;
                                    var new_street_mask = (_boardMask | card);
                                    var vilain_range = CBotPokerAmigo.UpdateRange(state_after_vilain.Item3, card);
                                    if (state_after_vilain.Item1.PCurrentStreet == Street.River)
                                        _turnBoardMask = new_street_mask;

                                    if ((state_after_vilain.Item1.PCurrentTurnPlayerIndex != initialPlayerIndex) && !state_after_vilain.Item1.PIsFinalState)
                                    {

                                        var board_meta_data2 = CalculateMetaData(new_street_mask);
                                        var vilainActionPossibleActions2 = GetVilainActionsProbabilitesV2(state_after_vilain.Item1, board_meta_data2, vilain_range, new_street_mask, _currentIndexPlayer, _turnBoardMask);
                                        foreach (var vilainSecondAction in vilainActionPossibleActions2)
                                        {
                                            var probabilityVilainAction2 = vilainSecondAction.Item2;
                                            action_result_states.Add(new ExtendedState(probabilityVilainAction2, vilainSecondAction.Item1, new_street_mask, vilainSecondAction.Item3, _turnBoardMask));
                                        }
                                    }
                                    else
                                    {
                                        action_result_states.Add(new ExtendedState(probability_leaf, state_after_vilain.Item1, new_street_mask, vilain_range, _turnBoardMask));
                                    }
                                }
                            }
                            else
                            {
                                action_result_states.Add(new ExtendedState(probability, state_after_vilain.Item1, _boardMask, state_after_vilain.Item3, _turnBoardMask));
                            }
                        }
                    }
                }
                extended_state_list.Add((stateAfterAction, action_result_states));
            }

            return new ExtendedStateList(this, extended_state_list);
        }*/

        public AState Fold()
        {
            return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Fold)));
        }
        public AState Check()
        {
            return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Check)));
        }
        public AState Call()
        {
            return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Call, GetLastAction(PPreviousTurnPlayerIndex).PMise)));
        }
        public AState Bet(double _betSize)
        {
            double betSize = Math.Round(_betSize, 2);

            if (betSize > PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Bet, PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)));
            else
                return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Bet, betSize)));
        }
        public AState Bet(BetSizePossible _betSizeType)
        {
            var betAmount = 0.0d;
            switch (_betSizeType)
            {
                case BetSizePossible.AllIn:
                case BetSizePossible.AllInShort:
                    betAmount = PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft;
                    break;
                case BetSizePossible.Percent100:
                    betAmount = PPot;
                    break;
                default:
                    if (!Enum.IsDefined(typeof(BetSizePossible), _betSizeType))
                        throw new ArgumentException();

                    betAmount = (PPot * PDicBetSize[_betSizeType]);
                    break;
            }
            return Bet(betAmount);
        }
        public AState Raise(double _mise)
        {
            double mise = Math.Round(_mise, 2);
            double allInSize = Math.Round(PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet, 2);

            mise = Math.Min(mise, allInSize);

            return CreateNewFromStreetVariant(ReceivedAction(new CAction(PokerAction.Raise, mise)));
        }

        public TypesPot GetTypePot()
        {
            TypesPot typePot;
            var actions = PActionList.TakeWhile(x => x.Item3 == Street.Preflop).ToList();
            var nbRaise = actions.LongCount(x => x.Item1.PAction == PokerAction.Raise);

            if (nbRaise == 0)
                if (actions.Count == 1)
                    typePot = TypesPot.Limped;
                else
                    typePot = TypesPot.OneBet;
            else if (nbRaise == 1)
                typePot = TypesPot.TwoBet;
            else if (nbRaise == 2)
                typePot = TypesPot.ThreeBet;
            else if (nbRaise == 3)
                typePot = TypesPot.FourBet;
            else
                typePot = TypesPot.FiveBetEtPlus;
            return typePot;
        }
        private TypesPot GetFilteredTypePot()
        {
            return PDicFilteredTypesPot[PTypePot];
        }
        protected abstract long LoadGameState();

        protected virtual bool IsActionValid(CAction _action)
        {
            var lastAction = GetLastAction(PPreviousTurnPlayerIndex);
            var impossiblePreviousActionInvariants = PokerAction.Fold | PokerAction.Call | PokerAction.Check;
            var impossiblePreviousAction = PokerAction.None;
            var betSize = 0.0d;

            switch (lastAction.PAction)
            {
                case PokerAction.Fold:
                case PokerAction.Call:
                    break;
                case PokerAction.Bet:
                    impossiblePreviousAction = PokerAction.Bet | PokerAction.Raise;
                    betSize = _action.PMise;
                    break;
                case PokerAction.Check:
                    impossiblePreviousAction = PokerAction.Bet | PokerAction.Raise;
                    break;
                case PokerAction.Raise:
                    impossiblePreviousAction = ~PokerAction.Bet;
                    betSize = _action.PMise - PPlayerList[PCurrentTurnPlayerIndex].PLastBet;
                    break;
                default:
                    return false;
            }

            if (((lastAction.PAction & (impossiblePreviousAction | impossiblePreviousActionInvariants)) != 0) ||
                (betSize > PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft))
                return false;
            return true;
        }

        protected List<AState> GetNextPossibleStates()
        {
            //var lstAllowedActions = GetLstAllowedActionsForCurrentPlayer();
            var ret = new List<AState>();
            var currentStreetActions = PActionList.Reverse().TakeWhile(x => x.Item3 == PCurrentStreet).ToList();
            if (PIsFinalState)
                return ret;


            if ((currentStreetActions.Count > 0) && (currentStreetActions.First().Item1.PAction != PokerAction.Check))
            {
                ret.Add(Fold());
                ret.Add(Call());
                if ((PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 5.0d) < (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet))
                    ret.Add(Raise(PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 2.5d));
                if ((PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 6.6d) < (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet))
                    ret.Add(Raise(PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 3.3d));
                if ((PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 8.1d) < (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet))
                    ret.Add(Raise(PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 4.1d));
                //if ((PPlayerList[PPreviousTurnPlayerIndex].PLastBet * 10.0d) < (PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet))
                //ret.Add(Raise((PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft + PPlayerList[PCurrentTurnPlayerIndex].PLastBet)));
            }


            if ((currentStreetActions.Count == 0) || (currentStreetActions.First().Item1.PAction == PokerAction.Check))
            {
                ret.Add(Check());
                var betSize = CAction.PDicBetSize[CAction.BetSizePossible.Percent50] * PPot;
                if (betSize < PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                    ret.Add(Bet(BetSizePossible.Percent33));
                betSize = CAction.PDicBetSize[CAction.BetSizePossible.Percent72] * PPot;
                if (betSize < PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                    ret.Add(Bet(BetSizePossible.Percent50));
                betSize = CAction.PDicBetSize[CAction.BetSizePossible.Percent100] * PPot;
                if (betSize < PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                    ret.Add(Bet(BetSizePossible.Percent72));
                betSize = CAction.PDicBetSize[CAction.BetSizePossible.Percent133] * PPot;
                if (betSize < PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                    ret.Add(Bet(BetSizePossible.Percent100));
                betSize = 1.5d * PPot;
                if (betSize > PPlayerList[PCurrentTurnPlayerIndex].PNumberOfChipsLeft)
                    ret.Add(Bet(BetSizePossible.AllIn));
            }

            return ret;
        }



        public abstract (Street, long, BoardMetaDataFlags, bool?) GetStateKey(Street _str, BoardMetaDataFlags _bm);

    }
}
