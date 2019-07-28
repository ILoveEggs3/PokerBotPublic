using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Models.MyModels.GameState
{
    public class CFlopGameState : AGameState
    {
        public TypesPot PTypePot { get; }
        public PokerPosition PPosition { get; }

        public CFlopGameState(long _ID, TypesPot _typePot, PokerPosition _position, PokerAction _action, long? _typeBet) : base(_ID, _action, _typeBet)
        {
            PTypePot = _typePot;
            PPosition = _position;
        }

        private CFlopGameState(CFlopGameState _flopGameState) : base(_flopGameState.PID, _flopGameState.PTypeAction, _flopGameState.PTypeBet)
        {
            PTypePot = _flopGameState.PTypePot;
            PPosition = _flopGameState.PPosition;
        }

        public static bool operator ==(CFlopGameState _flop1, CFlopGameState _flop2)
        {
            return _flop1?.PID == _flop2?.PID;
        }

        public static bool operator !=(CFlopGameState _flop1, CFlopGameState _flop2)
        {
            return !(_flop1 == _flop2);
        }

        public override bool Equals(object _flop2)
        {
            // Is null?
            if (ReferenceEquals(null, _flop2))
                return false;

            // Is the same object?
            if (ReferenceEquals(this, _flop2))
                return true;

            // Is the same type?
            if (_flop2.GetType() != GetType())
                return false;

            return PID == ((CFlopGameState)_flop2).PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 991435663;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            hashCode = hashCode * -1521134295 + PTypePot.GetHashCode();
            hashCode = hashCode * -1521134295 + PPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + PTypeAction.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<long?>.Default.GetHashCode(PTypeBet);
            return hashCode;
        }

        public CFlopGameState Clone()
        {
            return new CFlopGameState(this);
        }
    }
}
