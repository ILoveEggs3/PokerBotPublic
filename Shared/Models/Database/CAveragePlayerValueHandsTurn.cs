namespace Shared.Models.Database
{
    public class CAveragePlayerValueHandsTurn
    {
        public CTurnGameState PTurnGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PHandStrength { get; }
        public double PUnifiedCount { get; }
        public long PSampleCount { get; }

        public CAveragePlayerValueHandsTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, double _handStrength, double _unifiedCount, long _sampleCount)
        {
            PTurnGameState = _turnGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PHandStrength = _handStrength;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
