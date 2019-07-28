using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CDebugGeneralHandInfos
    {
        public ulong PPocketMask { get; }
        public ulong PBoardMask { get; }
        public string PHandDescription { get; }
        public string PBoardDescription { get; }
        public string PHandHistory { get; }

        public CDebugGeneralHandInfos(ulong _pocketMask, ulong _boardMask, string _handDescription, string _boardDescription, string _handHistory)
        {
            PPocketMask = _pocketMask;
            PBoardMask = _boardMask;
            PHandDescription = _handDescription;
            PBoardDescription = _boardDescription;
            PHandHistory = _handHistory;
        }
    }
}
