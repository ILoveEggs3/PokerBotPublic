using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CTurnOtherStats
    {
        public CTurnGameState PTurnGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public long PSampleCount { get; }

        public CTurnOtherStats(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, long _sampleCount)
        {
            PTurnGameState = _turnGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PSampleCount = _sampleCount;
        }
    }
}
