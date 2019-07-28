using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandFDFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public sbyte PIndexHighestCardExcludingBoardOfFlushCard { get; }
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandFDFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, sbyte _indexHighestCardExcludingBoardOfFlushCard, double _unifiedCount, int _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PIndexHighestCardExcludingBoardOfFlushCard = _indexHighestCardExcludingBoardOfFlushCard;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
