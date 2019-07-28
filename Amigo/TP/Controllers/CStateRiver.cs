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
    public class CStateRiver : AState
    {
        public long PTurnStateID { get; }
        protected override CTableInfos.Street FFCurrentStreet => CTableInfos.Street.River;

        public override (Street, long, CBoardModel.BoardMetaDataFlags, bool?) GetStateKey(Street _street, CBoardModel.BoardMetaDataFlags _bm)
        {
            var lastAction = PActionList.Last();

            if (lastAction.Item3 != PCurrentStreet)
                return (lastAction.Item3, PTurnStateID, _bm, null);

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
            if (PActionList.Last().Item3 == CTableInfos.Street.River)
            {
                if (GetLastAction().PAction == PokerAction.Call)
                    return true;
                if ((GetLastAction(PCurrentTurnPlayerIndex).PAction == PokerAction.Check) && (GetLastAction(PPreviousTurnPlayerIndex).PAction == PokerAction.Check))
                    return true;
            }

            return false;
        }

        protected CStateRiver(CStateRiver _state, AStateStreetVariants _variant) : base(_state, _variant)
        {
            PTurnStateID = _state.PTurnStateID;
        }

        protected CStateRiver(CStateTurn _state, AStateStreetVariants _variant) : base(_state, _variant)
        {
            PTurnStateID = _state.PGameState;
        }

        public static CStateRiver CreateFromTurn(CStateTurn _state, AStateStreetVariants _variant)
        {
            _variant.currentPlayerTurnIndex = BB_PLAYER_INDEX;
            _variant.playerList[BB_PLAYER_INDEX].PLastBet = _variant.playerList[BTN_PLAYER_INDEX].PLastBet = 0;
            return new CStateRiver(_state, _variant);
        }

        protected override AState CreateNewFromStreetVariant(AStateStreetVariants _variant)
        {
            return new CStateRiver(this, _variant);
        }

        protected override long LoadGameState()
        {
            var qwe = CPokerMath.ParseLastBetSize(PActionList, PPlayerList, PTypeFilteredPot, PIsHandShortStack, PPot);
            var key = (PTurnStateID, qwe.Item1, qwe.Item2);

            if (PIsFinalState && GetLastActionStreet() == CTableInfos.Street.Turn)
                return PTurnStateID;
            else
                return CDBHelper.PDicAllRiverGameStatesByInfos[(key.PTurnStateID, qwe.Item1, qwe.Item2)].PID;
        }

    }
}
