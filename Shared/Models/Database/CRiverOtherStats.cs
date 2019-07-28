using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CRiverOtherStats
    {
        public CRiverGameState PRiverGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public long PSampleCount { get; }

        public CRiverOtherStats(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, long _sampleCount)
        {
            PRiverGameState = _riverGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PSampleCount = _sampleCount;
        }
    }
}
