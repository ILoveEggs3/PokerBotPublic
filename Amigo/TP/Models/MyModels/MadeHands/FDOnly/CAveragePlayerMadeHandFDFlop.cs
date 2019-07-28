using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands.FDOnly
{
    public class CAveragePlayerMadeHandFDFlop : AMadeHandFDOnly
    {
        public CFlopGameState PGameState { get; }

        public CAveragePlayerMadeHandFDFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, sbyte _indexHighestCardExcludingBoardOfFlushCard, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _indexHighestCardExcludingBoardOfFlushCard, _unifiedCount, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
