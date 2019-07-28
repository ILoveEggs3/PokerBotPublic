using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amigo.Models.MyModels;


namespace Amigo.Models.MyModels.FoldStats
{
    public abstract class AFoldStats : AStats
    {
        public bool PCanRaise { get; }

        protected AFoldStats(ushort _boardType, double _boardHeat, bool _canRaise, long _sampleCount) : base(_boardType, _boardHeat, _sampleCount)
        {
            PCanRaise = _canRaise;
        }
    }
}
