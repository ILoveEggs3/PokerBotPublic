using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands.Blockers
{
    public class CAveragePlayerMadeHandBlockerRiver : AMadeHandBlocker
    {
        public CRiverGameState PGameState { get; }

        public CAveragePlayerMadeHandBlockerRiver(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount) : base(_boardType, _boardHeat, _blockerRatio, _handStrengthInBlockerRange, _sampleCount)
        {
            PGameState = _riverGameState;
        }
    }
}
