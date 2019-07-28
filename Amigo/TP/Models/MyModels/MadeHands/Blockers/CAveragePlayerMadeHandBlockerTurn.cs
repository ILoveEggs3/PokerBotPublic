using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands.Blockers
{
    public class CAveragePlayerMadeHandBlockerTurn : AMadeHandBlocker
    {
        public CTurnGameState PGameState { get; }

        public CAveragePlayerMadeHandBlockerTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, double _blockerRatio, double _handStrengthInBlockerRange, int _sampleCount) : base(_boardType, _boardHeat, _blockerRatio, _handStrengthInBlockerRange, _sampleCount)
        {
            PGameState = _turnGameState;
        }
    }
}
