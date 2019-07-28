using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CRiverFoldStats
    {
        public CRiverGameState PRiverGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public bool PCanRaise { get; }
        public long PSampleCount { get; }

        public CRiverFoldStats(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, bool _canRaise, long _sampleCount)
        {
            PRiverGameState = _riverGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PCanRaise = _canRaise;
            PSampleCount = _sampleCount;
        }
    }
}
