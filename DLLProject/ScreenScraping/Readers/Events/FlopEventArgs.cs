using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.Events
{
    public class FlopEventArgs : EventArgs
    {
        public List<PokerShared.CCard> PCardList;
        public IntPtr PHwnd;
    }
}
