using Shared.Poker.Models;
using System;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Events
{
    public sealed class COnNewStreetEventArgs : EventArgs
    {
        public Street PCurrentStreet { get; private set; }
        public CBoard PBoard { get; private set; }        

        public COnNewStreetEventArgs(Street _currentStreet, CBoard _board)
        {
            PCurrentStreet = _currentStreet;
            PBoard = _board ?? throw new ArgumentNullException("_board", "Invalid board");
        }
    }
}
