using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands
{
    public class CAveragePlayerMadeHandSDTurn : AMadeHand
    {
        public CTurnGameState PGameState { get; }

        public CAveragePlayerMadeHandSDTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _unifiedCount, _sampleCount)
        {
            PGameState = _turnGameState;
        }
    }
}
