namespace Shared.Models.Database
{
    public class CAveragePlayerMadeHandBlockerRiver
    {
        public CRiverGameState PRiverGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PBlockerRatio { get; }
        public double PHandStrengthInBlockerRange { get; }        
        public int PSampleCount { get; }

        public CAveragePlayerMadeHandBlockerRiver(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount)
        {
            PRiverGameState = _riverGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PBlockerRatio = _blockerRatio;
            PHandStrengthInBlockerRange = _handStrengthInBlockerRange;
            PSampleCount = _sampleCount;
        }
    }
}
