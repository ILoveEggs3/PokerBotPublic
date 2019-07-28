using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.AveragePlayerValueHands
{

    public abstract class AAveragePlayerValueHands
    {
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PHandStrength { get; }
        public decimal PUnifiedCount { get; }
        public long PSampleCount { get; }

        protected AAveragePlayerValueHands(ushort _boardType, double _boardHeat, double _handStrength, decimal _unifiedCount, long _sampleCount)
        {
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PHandStrength = _handStrength;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
