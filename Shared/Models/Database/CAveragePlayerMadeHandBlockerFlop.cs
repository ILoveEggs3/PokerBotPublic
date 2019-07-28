namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandBlockerFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PBlockerRatio { get; }
        public double PHandStrengthInBlockerRange { get; }
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandBlockerFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PBlockerRatio = _blockerRatio;
            PHandStrengthInBlockerRange = _handStrengthInBlockerRange;
            PSampleCount = _sampleCount;
        }
    }
}
