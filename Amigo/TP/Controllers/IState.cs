using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Controllers
{
    public interface IState
    {
        bool PIsFinalState { get; }
        int PHeroIndex { get; }
        TypesPot PTypePot { get; }
        int PCurrentTurnPlayerIndex { get; }
        int PPreviousTurnPlayerIndex { get; }
        List<AState> PNextPossibleStateList { get; }
        CAction GetLastAction(int _playerIndex);
    }
}
