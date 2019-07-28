using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.Events
{
    public class OurTurnEventArgs : EventArgs
    {
        //GameState
        public Bitmap PBmp;
        public decimal PTotalPot;
        public List<PlayerModel> PPlayerList;
    }
}
