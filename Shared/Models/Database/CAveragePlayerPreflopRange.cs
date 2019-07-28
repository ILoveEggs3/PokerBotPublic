using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Shared.Models.Database
{
    public class CAveragePlayerPreflopRange
    {
        public TypesPot PTypePot { get; }
        public PossiblePositions PPosition { get; }
        public ulong PPocketMask { get; }
        public string PHandDescription { get; }
        public int PSampleCount { get; }

        public CAveragePlayerPreflopRange(TypesPot _typePot, PossiblePositions _position, ulong _pocketMask, string _handDescription, int _sampleCount)
        {
            PTypePot = _typePot;
            PPosition = _position;
            PPocketMask = _pocketMask;
            PHandDescription = _handDescription;
            PSampleCount = _sampleCount;
        }
    }
}
