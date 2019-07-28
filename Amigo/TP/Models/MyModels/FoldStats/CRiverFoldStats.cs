using Amigo.Models.MyModels.GameState;
using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.FoldStats
{
    public class CRiverFoldStats : AFoldStats
    {
        public CRiverGameState PGameState { get; }

        public CRiverFoldStats(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, bool _canRaise, long _sampleCount) : base(_boardType, _boardHeat, _canRaise, _sampleCount)
        {
            PGameState = _riverGameState;
        }
    }
}
