namespace Shared.Models.Database
{
    public class CAveragePlayerBluffsWithLotsOfEquityFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public sbyte PNumberOfOuts { get; } // Because SQLite uses unsigned data types
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerBluffsWithLotsOfEquityFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, sbyte _numberOfOuts, double _unifiedCount, int _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PNumberOfOuts = _numberOfOuts;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
