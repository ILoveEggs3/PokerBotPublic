using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CFlopFoldStats
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public bool PCanRaise { get; }
        public long PSampleCount { get; }

        public CFlopFoldStats(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, bool _canRaise, long _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PCanRaise = _canRaise;
            PSampleCount = _sampleCount;
        }
    }
}
