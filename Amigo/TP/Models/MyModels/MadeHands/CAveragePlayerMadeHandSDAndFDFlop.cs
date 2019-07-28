using Amigo.Models.MyModels.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.MadeHands
{
    public class CAveragePlayerMadeHandSDAndFDFlop : AMadeHand
    {
        public CFlopGameState PGameState { get; }

        public CAveragePlayerMadeHandSDAndFDFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _unifiedCount, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
