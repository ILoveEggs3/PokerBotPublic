using Amigo.Models.MyModels.GameState;

namespace Amigo.Models.MyModels.AveragePlayerValueHands
{ 
    public class CAveragePlayerValueHandsFlop : AAveragePlayerValueHands
    {
        public CFlopGameState PGameState { get; }

        public CAveragePlayerValueHandsFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, double _handStrength, decimal _unifiedCount, long _sampleCount) : base(_boardType, _boardHeat, _handStrength, _unifiedCount, _sampleCount)
        {
            PGameState = _flopGameState;
        }
    }
}
