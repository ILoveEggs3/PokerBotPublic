using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Events
{
    public sealed class COnNewHandEventArgs : EventArgs
    {
        public int PBTNPlayerIndex { get; private set; }

        public Tuple<int, double> PSmallBlind { get; private set; } // Int = index of player
        public Tuple<int, double> PBigBlind { get; private set; } // Int = index of player

        public Dictionary<int, string> PPlayerNames { get; private set; } // Int = index of player, string = name of player, can be null if unknown
        public Dictionary<int, string> PPlayerCards { get; private set; } // Int = index of player
        public Dictionary<int, double> PPlayerStacks { get; private set; } // Int = index of player

        public COnNewHandEventArgs(int _dealerIndex, Tuple<int, double> _smallBlind, Tuple<int, double> _bigBlind, Dictionary<int, string> _playerNames, Dictionary<int, string> _playerCards, Dictionary<int, double> _playerStacks)
        {
            PBTNPlayerIndex = _dealerIndex;
            PSmallBlind = _smallBlind ?? throw new ArgumentNullException("_smallBlind", "Invalid small blind");
            PBigBlind = _bigBlind ?? throw new ArgumentNullException("_bigBlind", "Invalid big blind");
            PPlayerNames = _playerNames ?? throw new ArgumentNullException("_playerNames", "Invalid dictionary");
            PPlayerCards = _playerCards ?? throw new ArgumentNullException("_playerCards", "Invalid dictionary");
            PPlayerStacks = _playerStacks ?? throw new ArgumentNullException("_playerStacks", "Invalid dictionary");            
        }
    }
}
