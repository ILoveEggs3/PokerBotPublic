using Shared.Poker.Models;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Shared.Models.Database
{
    public class CTurnGameState
    {
        public long PID { get; }
        public CFlopGameState PFlopGameStateID { get; }
        public ActionsPossible PTypeAction { get; }
        public long? PTypeBet { get; }

        public CTurnGameState(long _ID, CFlopGameState _flopGameStateID, ActionsPossible _action, long? _typeBet)
        {
            PID = _ID;
            PFlopGameStateID = _flopGameStateID;
            PTypeAction = _action;
            PTypeBet = _typeBet;
        }

        public CTurnGameState(CTurnGameState _turnGameState)
        {
            PID = _turnGameState.PID;
            PFlopGameStateID = _turnGameState.PFlopGameStateID.Clone();
            PTypeAction = _turnGameState.PTypeAction;
            PTypeBet = _turnGameState.PTypeBet;
        }

        public static bool operator ==(CTurnGameState _turn1, CTurnGameState _turn2)
        {
            return _turn1?.PID == _turn2?.PID;
        }

        public static bool operator !=(CTurnGameState _turn1, CTurnGameState _turn2)
        {
            return !(_turn1 == _turn2);
        }

        public override bool Equals(object obj)
        {
            var state = obj as CTurnGameState;
            return state != null &&
                   PID == state.PID;
        }

        public override int GetHashCode()
        {
            var hashCode = 279410301;
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            return hashCode;
        }

        public CTurnGameState Clone()
        {
            return new CTurnGameState(this);
        }
    }
}
