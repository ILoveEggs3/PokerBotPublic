using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CFlopOtherStats
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public long PSampleCount { get; }

        public CFlopOtherStats(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, long _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PSampleCount = _sampleCount;
        }
    }
}
