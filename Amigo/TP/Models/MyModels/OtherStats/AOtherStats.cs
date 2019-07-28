using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.OtherStats
{
    public abstract class AOtherStats : AStats
    {
        protected AOtherStats(ushort _boardType, double _boardHeat, long _sampleCount) : base(_boardType, _boardHeat, _sampleCount) { }
    }
}
