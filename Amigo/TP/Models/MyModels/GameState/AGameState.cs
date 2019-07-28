using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Poker.Models.CAction;

namespace Amigo.Models.MyModels.GameState
{
    public abstract class AGameState
    {
        public long PID { get; }
        public PokerAction PTypeAction { get; }
        public long? PTypeBet { get; }

        protected AGameState(long _PID, PokerAction _typeAction, long? _typeBet)
        {
            PID = _PID;
            PTypeAction = _typeAction;
            PTypeBet = _typeBet;
        }
    }
}
