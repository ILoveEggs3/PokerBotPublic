using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.MadeHands
{
    public abstract class AMadeHand
    {
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public decimal PUnifiedCount { get; }
        public int PSampleCount { get; }

        protected AMadeHand(ushort _boardType, double _boardHeat, decimal _unifiedCount, int _sampleCount)
        {
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
