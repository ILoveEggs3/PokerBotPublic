namespace Amigo.Models.MyModels.MadeHands.Blockers
{
    public abstract class AMadeHandBlocker
    {
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public double PBlockerRatio { get; }
        public double PHandStrengthInBlockerRange { get; }
        public int PSampleCount { get; }

        protected AMadeHandBlocker(ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount)
        {
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PBlockerRatio = _blockerRatio;
            PHandStrengthInBlockerRange = _handStrengthInBlockerRange;
            PSampleCount = _sampleCount;
        }
    }
}
