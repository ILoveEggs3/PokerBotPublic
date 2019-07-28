namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandSDFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandSDFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, double _unifiedCount, int _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
