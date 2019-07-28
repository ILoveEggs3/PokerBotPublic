using Amigo.Models.MyModels.GameState;
using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.FoldStats
{
    public class CFlopFoldStats : AFoldStats
    {
        public CFlopGameState PGameState { get; }

        public CFlopFoldStats(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, bool _canRaise, long _sampleCount) : base(_boardType, _boardHeat, _canRaise, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
