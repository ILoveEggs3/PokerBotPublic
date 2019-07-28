using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.Events
{
    public enum EventType
    {
        PlayedForHoursEvent,
        TournamentAnnouncementEvent
    }
    public class PopupEventArgs : EventArgs
    {
        public EventType PEventType;
    }
}
