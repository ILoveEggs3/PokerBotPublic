namespace Amigo.Models.MyModels.MadeHands.FDOnly
{
    public abstract class AMadeHandFDOnly : AMadeHand
    {
        public sbyte PIndexHighestCardExcludingBoardOfFlushCard { get; }

        protected AMadeHandFDOnly(ushort _boardType, double _boardHeat, sbyte _indexHighestCardExcludingBoardOfFlushCard, decimal _unifiedCount, int _sampleCount) : base(_boardType, _boardHeat, _unifiedCount, _sampleCount)
        {
            PIndexHighestCardExcludingBoardOfFlushCard = _indexHighestCardExcludingBoardOfFlushCard;
        }
    }
}
