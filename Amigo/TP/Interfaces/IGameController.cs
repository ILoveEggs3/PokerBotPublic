using Amigo.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Interfaces
{
    public interface IGameController
    {
        event EventHandler<COnNewHandEventArgs> POnNewHand;
        event EventHandler<COnNewActionEventArgs> POnNewAction;
        event EventHandler<COnNewStreetEventArgs> POnNewStreet;
        event EventHandler<COnHandFinishedEventArgs> POnHandFinished;

        void Fold();
        void Check();
        void Call();
        void Bet(double _betSize);
        void Raise(double _betSize);
    }
}
