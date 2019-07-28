using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands
{
    public class CAveragePlayerMadeHandSDFlop : AMadeHand
    {
        public CFlopGameState PGameState { get; }

        public CAveragePlayerMadeHandSDFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _unifiedCount, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
