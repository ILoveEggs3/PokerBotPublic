namespace Shared.Models.Database
{
    public class CAveragePlayerValueHandsFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PHandStrength { get; }
        public double PUnifiedCount { get; }
        public long PSampleCount { get; }

        public CAveragePlayerValueHandsFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, double _handStrength, double _unifiedCount, long _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PHandStrength = _handStrength;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
