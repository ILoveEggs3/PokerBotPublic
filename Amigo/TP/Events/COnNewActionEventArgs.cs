using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Amigo.Events
{
    public sealed class COnNewActionEventArgs: EventArgs
    {
        public int PPlayerID { get; private set; }
        public double PPot { get; private set; }
        public CAction PNewAction { get; private set; }
        public Dictionary<int, CPlayer> PPlayersStacks { get; private set; }

        public COnNewActionEventArgs(int _playerID, double _pot, CAction _newAction, Dictionary<int, CPlayer> _playerStacks)
        {
            PPlayerID = _playerID;
            PPot = _pot;
            PNewAction = _newAction ?? throw new ArgumentNullException("_newAction", "Invalid action");
            PPlayersStacks = _playerStacks ?? throw new ArgumentNullException("_playerStacks", "Invalid dictionary");
        }
    }
}
