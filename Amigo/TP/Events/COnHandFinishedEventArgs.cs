using System;
using System.Collections.Generic;

namespace Amigo.Events
{
    public sealed class COnHandFinishedEventArgs : EventArgs
    {
        public List<Tuple<int, double>> PLstPlayersThatWonThePot { get; private set; } // Int = index of player, double = amount that the player won
        public Dictionary<int, string> PPlayerCards { get; private set; } // Int = index of player

        public COnHandFinishedEventArgs(List<Tuple<int, double>> _lstPlayersThatWonThePot, Dictionary<int, string> _playerCards)
        {
            PLstPlayersThatWonThePot = _lstPlayersThatWonThePot ?? throw new ArgumentNullException("_lstPlayersThatWonThePot", "Invalid list of players");
            PPlayerCards = _playerCards ?? throw new ArgumentNullException("_playerCards", "Invalid dictionary");
        }
    }
}
