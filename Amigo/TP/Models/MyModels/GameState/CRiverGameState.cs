using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Amigo.Models.MyModels.GameState
{
    public class CRiverGameState : AGameState
    {
        public CTurnGameState PGameStateID { get; }

        public CRiverGameState(long _ID, CTurnGameState _turnGameStateID, PokerAction _typeAction, long? _typeBet) : base(_ID, _typeAction, _typeBet)
        {
            PGameStateID = _turnGameStateID;
        }

        public CRiverGameState(CRiverGameState _riverGameState) : base(_riverGameState.PID, _riverGameState.PTypeAction, _riverGameState.PTypeBet)
        {
            PGameStateID = _riverGameState.PGameStateID;
        }

        public static bool operator ==(CRiverGameState _river1, CRiverGameState _river2)
        {
            return _river1?.PID == _river2?.PID;
        }

        public static bool operator !=(CRiverGameState _river1, CRiverGameState _river2)
        {
            return !(_river1 == _river2);
        }

        public override bool Equals(object _river2)
        {
            // Is null?
            if (ReferenceEquals(null, _river2))
                return false;

            // Is the same object?
            if (ReferenceEquals(this, _river2))
                return true;

            // Is the same type?
            if (_river2.GetType() != GetType())
                return false;

            return PID == ((CRiverGameState)_river2).PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 1378316379;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CTurnGameState>.Default.GetHashCode(PGameStateID);
            hashCode = hashCode * -1521134295 + PTypeAction.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<long?>.Default.GetHashCode(PTypeBet);
            return hashCode;
        }

        public CRiverGameState Clone()
        {
            return new CRiverGameState(this);
        }
    }
}
