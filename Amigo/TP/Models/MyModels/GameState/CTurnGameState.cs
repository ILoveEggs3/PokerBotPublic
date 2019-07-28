using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Amigo.Models.MyModels.GameState
{
    public class CTurnGameState : AGameState
    {
        public CFlopGameState PGameStateID { get; }

        public CTurnGameState(long _ID, CFlopGameState _flopGameStateID, PokerAction _action, long? _typeBet) : base(_ID, _action, _typeBet)
        {
            PGameStateID = _flopGameStateID;
        }

        public CTurnGameState(CTurnGameState _turnGameState) : base(_turnGameState.PID, _turnGameState.PTypeAction, _turnGameState.PTypeBet)
        {
            PGameStateID = _turnGameState.PGameStateID;
        }

        public static bool operator ==(CTurnGameState _turn1, CTurnGameState _turn2)
        {
            return _turn1?.PID == _turn2?.PID;
        }

        public static bool operator !=(CTurnGameState _turn1, CTurnGameState _turn2)
        {
            return !(_turn1 == _turn2);
        }

        public override bool Equals(object _turn2)
        {
            // Is null?
            if (ReferenceEquals(null, _turn2))
                return false;

            // Is the same object?
            if (ReferenceEquals(this, _turn2))
                return true;

            // Is the same type?
            if (_turn2.GetType() != GetType())
                return false;

            return PID == ((CTurnGameState)_turn2).PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 279410301;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CFlopGameState>.Default.GetHashCode(PGameStateID);
            hashCode = hashCode * -1521134295 + PTypeAction.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<long?>.Default.GetHashCode(PTypeBet);
            return hashCode;
        }

        public CTurnGameState Clone()
        {
            return new CTurnGameState(this);
        }
    }
}
