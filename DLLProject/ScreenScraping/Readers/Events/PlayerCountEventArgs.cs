using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.Events
{
    public class PlayerCountEventArgs : EventArgs
    {
        public uint PPlayerCount;
        public IntPtr PHwnd;
    }
}
