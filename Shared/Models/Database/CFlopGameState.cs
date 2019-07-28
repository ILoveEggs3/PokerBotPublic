using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Shared.Models.Database
{
    public class CFlopGameState
    {
        public long PID { get; }
        public TypesPot PTypePot { get; }
        public PossiblePositions PPosition { get; }
        public ActionsPossible PTypeAction { get; }
        public long? PTypeBet { get; }

        public CFlopGameState(long _ID, TypesPot _typePot, PossiblePositions _position, ActionsPossible _action, long? _typeBet)
        {
            PID = _ID;
            PTypePot = _typePot;
            PPosition = _position;
            PTypeAction = _action;
            PTypeBet = _typeBet;
        }

        private CFlopGameState(CFlopGameState _flopGameState)
        {
            PID = _flopGameState.PID;
            PTypePot = _flopGameState.PTypePot;
            PPosition = _flopGameState.PPosition;
            PTypeAction = _flopGameState.PTypeAction;
            PTypeBet = _flopGameState.PTypeBet;
        }

        public static bool operator ==(CFlopGameState _flop1, CFlopGameState _flop2)
        {
            return _flop1?.PID == _flop2?.PID;
        }

        public static bool operator !=(CFlopGameState _flop1, CFlopGameState _flop2)
        {
            return !(_flop1 == _flop2);
        }

        public override bool Equals(object obj)
        {
            var state = obj as CFlopGameState;
            return state != null &&
                   PID == state.PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 991435663;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            return hashCode;
        }

        public CFlopGameState Clone()
        {
            return new CFlopGameState(this);
        }
    }
}
