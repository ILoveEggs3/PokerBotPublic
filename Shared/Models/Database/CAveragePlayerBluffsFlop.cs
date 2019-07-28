namespace Shared.Models.Database
{
    public class CAveragePlayerBluffsFlop
    {
        public CFlopGameState PFlopGameState { get; }
        public ushort PBoardType { get; }
        public double PBoardHeat { get; }
        public bool PIsBackdoorFlushDraw { get; }
        public bool PIsBackdoorStraightDraw { get; }
        public bool PIsStraightDraw { get; }
        public bool PIsFlushDraw { get; }
        public sbyte PIndexHighestCardExcludingBoard { get; } // Because SQLite uses unsigned data types
        public double PUnifiedCount { get; }
        public long PSampleCount { get; }
        public CAveragePlayerBluffsFlop(CFlopGameState _flopGameState, ushort _boardType, double _boardHeat, bool _isBackDoorFlushDraw, bool _isBackDoorStraightDraw, bool _isStraightDraw, bool _isFlushDraw, sbyte _indexHighestCardExcludingBoard, double _unifiedCount, long _sampleCount)
        {
            PFlopGameState = _flopGameState;
            PBoardType = _boardType;
            PBoardHeat = _boardHeat;
            PIsBackdoorFlushDraw = _isBackDoorFlushDraw;
            PIsBackdoorStraightDraw = _isBackDoorStraightDraw;
            PIsStraightDraw = _isStraightDraw;
            PIsFlushDraw = _isFlushDraw;
            PIndexHighestCardExcludingBoard = _indexHighestCardExcludingBoard;
            PUnifiedCount = _unifiedCount;
            PSampleCount = _sampleCount;
        }
    }
}
