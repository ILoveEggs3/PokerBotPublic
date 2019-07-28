using Amigo.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Interfaces
{
    public interface IGameHumanVsBotController : IGameController
    {
        event EventHandler<COnWaitingForHumanActionEventArgs> POnWaitingForHumanAction;
    }
}
