namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandBlockerTurn
    {
        public CTurnGameState PTurnGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PBlockerRatio { get; }
        public double PHandStrengthInBlockerRange { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandBlockerTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount)
        {
            PTurnGameState = _turnGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PHandStrengthInBlockerRange = _handStrengthInBlockerRange;
            PSampleCount = _sampleCount;
        }
    }
}
