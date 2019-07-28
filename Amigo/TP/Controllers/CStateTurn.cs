using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amigo.Bots;
using Amigo.Helpers;
using Amigo.Models.MyModels.GameState;
using Shared.Models.Database;
using Shared.Poker.Models;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Controllers
{
    public class CStateTurn : AState
    {
        public long PFlopStateID { get; }
        protected override CTableInfos.Street FFCurrentStreet => CTableInfos.Street.Turn;

        public override (Street, long, CBoardModel.BoardMetaDataFlags, bool?) GetStateKey(Street _street, CBoardModel.BoardMetaDataFlags _bm)
        {
            var lastAction = PActionList.Last();

            if (lastAction.Item3 != PCurrentStreet)
                return (lastAction.Item3, PFlopStateID, _bm, null);

            (Street, long, CBoardModel.BoardMetaDataFlags, bool?) key;
            if (lastAction.Item1.PAction == PokerAction.Fold)
            {
                key = (PCurrentStreet, PGameState, _bm, true);
                if (!CDBHelper.PGameStatesStats.ContainsKey(key))
                    key = (PCurrentStreet, PGameState, _bm, false);
            }
            else
                key = (PCurrentStreet, PGameState, _bm, null);

            return key;
        }

        public override bool IsFinalState()
        {
            if (PActionList.Last().Item1.PAction == PokerAction.Fold)
                return true;
            if (PActionList.Last().Item1.PAction == PokerAction.Call && (Math.Min(PPlayerList[0].PNumberOfChipsLeft, PPlayerList[1].PNumberOfChipsLeft) - 0.01) <= 0.01)
                return true;
            return false;
        }

        protected CStateTurn(CStateTurn _state, AStateStreetVariants _variant) : base(_state, _variant)
        {
            PFlopStateID = _state.PFlopStateID;
        }

        protected override AState CreateNewFromStreetVariant(AStateStreetVariants _variant)
        {
            var state = new CStateTurn(this, _variant);
            var actionList = state.PActionList.Where(x => x.Item3 == PCurrentStreet);
            var currentStreetCount = actionList.Count(x => x.Item3 == PCurrentStreet && x.Item1.PAction == PokerAction.Check);
            if (actionList.Last().Item1.PAction == PokerAction.Call || currentStreetCount == 2)
                return CStateRiver.CreateFromTurn(state, _variant);
            return state;
        }

        protected CStateTurn(CStateFlop _state, AStateStreetVariants _variant) : base(_state, _variant)
        {
            PFlopStateID = _state.PGameState;
        }

        public static CStateTurn CreateFromFlop(CStateFlop _state, AStateStreetVariants _variant)
        {
            _variant.currentPlayerTurnIndex = BB_PLAYER_INDEX;
            _variant.playerList[BB_PLAYER_INDEX].PLastBet = _variant.playerList[BTN_PLAYER_INDEX].PLastBet = 0;
            return new CStateTurn(_state, _variant);
        }

        protected override long LoadGameState()
        {
            var qwe = CPokerMath.ParseLastBetSize(PActionList, PPlayerList, PTypeFilteredPot, PIsHandShortStack, PPot);
            var key = (PFlopStateID, qwe.Item1, qwe.Item2);

            if (PIsFinalState && GetLastActionStreet() == CTableInfos.Street.Flop)
                return PFlopStateID;
            else
                return CDBHelper.PDicAllTurnGameStatesByInfos[(key.PFlopStateID, qwe.Item1, qwe.Item2)].PID;
        }

    }
}
