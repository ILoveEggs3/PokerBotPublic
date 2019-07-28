using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;

namespace Amigo.Events
{
    public sealed class COnWaitingForHumanActionEventArgs: EventArgs
    {
        public double PPot { get; private set; }
        public double PHeroNumberOfChipsLeft { get; private set; }
        public double PVillainLastBet { get; private set; }
        public HashSet<PokerAction> PHeroAllowedActions { get; private set; }

        public COnWaitingForHumanActionEventArgs(double _pot, double _heroNumberOfChipsLeft, double _villainLastBet, HashSet<PokerAction> _heroAllowedActions)
        {
            PPot = _pot;
            PHeroNumberOfChipsLeft = _heroNumberOfChipsLeft;
            PVillainLastBet = _villainLastBet;
            PHeroAllowedActions = _heroAllowedActions ?? throw new ArgumentNullException("_possibleActions", "Invalid action");
        }
    }
}
