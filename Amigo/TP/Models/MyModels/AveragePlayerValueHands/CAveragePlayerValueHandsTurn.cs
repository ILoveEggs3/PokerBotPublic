using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.AveragePlayerValueHands
{
    public class CAveragePlayerValueHandsTurn : AAveragePlayerValueHands
    { 
        public CTurnGameState PGameState { get; }

        public CAveragePlayerValueHandsTurn(CTurnGameState _turnGameState, ushort _boardType, double _boardHeat, double _handStrength, decimal _unifiedCount, long _sampleCount) : base(_boardType, _boardHeat, _handStrength, _unifiedCount, _sampleCount)
        {
            PGameState = _turnGameState;
        }
    }
}
