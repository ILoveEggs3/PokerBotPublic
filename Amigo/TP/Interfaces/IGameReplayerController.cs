using Amigo.Events;
using HandHistories.Objects.Hand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Interfaces
{
    public interface IGameReplayerController
    {
        event EventHandler<COnNewHandEventArgs> POnNewHand;
        event EventHandler<COnNewActionEventArgs> POnNewAction;
        event EventHandler<COnNewStreetEventArgs> POnNewStreet;
        event EventHandler<COnHandFinishedEventArgs> POnHandFinished;
        event EventHandler<COnNewRangeReceivedEventArgs> POnNewRangeReceived;

        void ParseNewHand(HandHistory _handHistory);
        void Forward();
    }
}
