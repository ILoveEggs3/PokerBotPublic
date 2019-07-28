using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels
{
    public abstract class AStats
    {
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public long PSampleCount { get; }

        protected AStats(ushort _boardType, double _boardHeat, long _sampleCount)
        {
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PSampleCount = _sampleCount;
        }
    }
}
