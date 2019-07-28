namespace Shared.Models.Database
{
    public class CAveragePlayerValueHandsRiver
    {
        public CRiverGameState PRiverGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PHandStrength { get; }
        public double PUnifiedCount { get; }
        public long PSampleCount { get; }

        public CAveragePlayerValueHandsRiver(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, double _handStrength, double _unifiedCount, long _sampleCount)
        {
            PRiverGameState = _riverGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PHandStrength = _handStrength;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
