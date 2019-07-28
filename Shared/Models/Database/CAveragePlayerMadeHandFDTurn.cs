namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandFDTurn
    {
        public CTurnGameState PTurnGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public sbyte PIndexHighestCardExcludingBoardOfFlushCard { get; }
        public double PUnifiedCount { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandFDTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, sbyte _indexHighestCardExcludingBoardOfFlushCard, double _unifiedCount, int _sampleCount)
        {
            PTurnGameState = _turnGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PIndexHighestCardExcludingBoardOfFlushCard = _indexHighestCardExcludingBoardOfFlushCard;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
