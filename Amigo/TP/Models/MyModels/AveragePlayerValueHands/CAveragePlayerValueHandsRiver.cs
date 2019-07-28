using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.AveragePlayerValueHands
{
    public class CAveragePlayerValueHandsRiver : AAveragePlayerValueHands
    {
        public CRiverGameState PGameState { get; }

        public CAveragePlayerValueHandsRiver(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, double _handStrength, decimal _unifiedCount, long _sampleCount) : base(_boardType, _boardHeat, _handStrength, _unifiedCount, _sampleCount)
        {
            PGameState = _riverGameState;
        }
    }
}
