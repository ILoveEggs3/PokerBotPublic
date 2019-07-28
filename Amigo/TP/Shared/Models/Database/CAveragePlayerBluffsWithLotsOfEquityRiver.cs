using Amigo.Models.MyModels.GameState;

namespace Shared.Models.Database
{
    public class CAveragePlayerBluffsWithLotsOfEquityRiver
    {
        public CRiverGameState PGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public sbyte PNumberOfOuts { get; } // Because SQLite uses unsigned data types
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerBluffsWithLotsOfEquityRiver(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, sbyte _numberOfOuts, double _unifiedCount, int _sampleCount)
        {
            PGameState = _riverGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PNumberOfOuts = _numberOfOuts;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
