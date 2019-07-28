using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Shared.Models.Database
{
    public class CRiverGameState
    {
        public long PID { get; }
        public CTurnGameState PTurnGameStateID { get; }
        public ActionsPossible PTypeAction { get; }
        public long? PTypeBet { get; }

        public CRiverGameState(long _ID, CTurnGameState _turnGameStateID, ActionsPossible _typeAction, long? _typeBet)
        {
            PID = _ID;
            PTurnGameStateID = _turnGameStateID;
            PTypeAction = _typeAction;
            PTypeBet = _typeBet;
        }

        public CRiverGameState(CRiverGameState _riverGameState)
        {
            PID = _riverGameState.PID;
            PTurnGameStateID = _riverGameState.PTurnGameStateID.Clone();
            PTypeAction = _riverGameState.PTypeAction;
            PTypeBet = _riverGameState.PTypeBet;
        }

        public static bool operator ==(CRiverGameState _river1, CRiverGameState _river2)
        {
            return _river1?.PID == _river2?.PID;
        }

        public static bool operator !=(CRiverGameState _river1, CRiverGameState _river2)
        {
            return !(_river1 == _river2);
        }

        public override bool Equals(object obj)
        {
            var state = obj as CRiverGameState;
            return state != null &&
                   PID == state.PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 1378316379;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            return hashCode;
        }

        public CRiverGameState Clone()
        {
            return new CRiverGameState(this);
        }
    }
}
