using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.MadeHands.FDOnly
{
    public class CAveragePlayerMadeHandFDTurn : AMadeHandFDOnly
    {
        public CTurnGameState PGameState { get; }

        public CAveragePlayerMadeHandFDTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, sbyte _indexHighestCardExcludingBoardOfFlushCard, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _indexHighestCardExcludingBoardOfFlushCard, _unifiedCount, _sampleCount)
        {
            PGameState = _turnGameState;
        }
    }
}
