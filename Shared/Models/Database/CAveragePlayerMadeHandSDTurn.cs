namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandSDTurn
    {
        public CTurnGameState PTurnGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandSDTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, double _unifiedCount, int _sampleCount)
        {
            PTurnGameState = _turnGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
