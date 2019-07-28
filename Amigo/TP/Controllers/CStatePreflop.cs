using Amigo.Models.MyModels.GameState;
using Shared.Models.Database;
using Shared.Poker.Helpers;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Controllers
{
    public class CStatePreflop : AState
    {
        protected override Street FFCurrentStreet => Street.Preflop;

        public (Street, long, BoardMetaDataFlags, bool?) GetStateKey(BoardMetaDataFlags _bm)
        {
            throw new Exception("Bad function call");
        }

        protected CStatePreflop(CStatePreflop _state, AStateStreetVariants _variant) : base(_state, _variant) { }

        protected override AState CreateNewFromStreetVariant(AStateStreetVariants _variant)
        {
            var state = new CStatePreflop(this, _variant);
            
            if (_variant.actionList.Last().Item1.PAction == PokerAction.Call)
                return CStateFlop.CreateFromPrefFlop(state, _variant);
            return state;
        }

        public override bool IsFinalState()
        {
            if (GetLastAction().PAction == PokerAction.Fold)
                return true;
            if (GetLastAction().PAction == PokerAction.Call && (Math.Min(PPlayerList[0].PNumberOfChipsLeft, PPlayerList[1].PNumberOfChipsLeft) == 0))
                return true;
            return false;
        }

        public CStatePreflop(List<CPlayer> _playerList, List<(CAction, int, Street)> _actionList, int _heroIndex, int _currentPlayerTurnIndex, double _pot, double _bigBlind)/*, AGameState _state)*/ :
    base(_playerList, _actionList, _heroIndex, _currentPlayerTurnIndex, _pot, _bigBlind)
        { }

        public static CStatePreflop CreateNewGame(int _heroIndex, List<CPlayer> playerList, double _bigBlind)
        {
            var btnPlayer = playerList[BTN_PLAYER_INDEX].Clone();
            var bbPlayer = playerList[BB_PLAYER_INDEX].Clone();

            var stacks = Math.Min(btnPlayer.PNumberOfChipsLeft, bbPlayer.PNumberOfChipsLeft);
            var sb = (_bigBlind / 2);
            sb = Math.Min(sb, stacks);
            var bb = Math.Min(_bigBlind, stacks);

            btnPlayer.PLastBet = sb;
            btnPlayer.PNumberOfChipsLeft = stacks - sb;
            btnPlayer.PPosition = CPlayer.PokerPosition.BTN;

            bbPlayer.PLastBet = bb;
            bbPlayer.PNumberOfChipsLeft = stacks - bb;
            bbPlayer.PPosition = CPlayer.PokerPosition.BB;

            var actionList = new List<(CAction, int, Street)>(); 
            var pot = btnPlayer.PLastBet + bbPlayer.PLastBet;

            return new CStatePreflop(new List<CPlayer>() { btnPlayer, bbPlayer }, actionList, _heroIndex, BTN_PLAYER_INDEX, pot, _bigBlind);
        }

        protected override long LoadGameState()
        {
            throw new Exception("Cant call this now");
        }


        public override (Street, long, BoardMetaDataFlags, bool?) GetStateKey(Street _str, BoardMetaDataFlags _bm)
        {
            throw new NotImplementedException();
        }
    }
}
