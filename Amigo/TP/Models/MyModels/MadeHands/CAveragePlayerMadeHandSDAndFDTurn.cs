using Amigo.Models.MyModels.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.MadeHands
{
    public class CAveragePlayerMadeHandSDAndFDTurn : AMadeHand
    {
        public CTurnGameState PGameState { get; }

        public CAveragePlayerMadeHandSDAndFDTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _unifiedCount, _sampleCount)
        {
            PGameState = _turnGameState;
        }
    }
}
