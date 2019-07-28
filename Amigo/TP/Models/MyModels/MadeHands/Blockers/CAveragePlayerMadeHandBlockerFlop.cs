using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands.Blockers
{
    public class CAveragePlayerMadeHandBlockerFlop : AMadeHandBlocker
    {
        public CFlopGameState PGameState { get; }

        public CAveragePlayerMadeHandBlockerFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount) : base(_boardType, _boardHeat, _blockerRatio, _handStrengthInBlockerRange, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
