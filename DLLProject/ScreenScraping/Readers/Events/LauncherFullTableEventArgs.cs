using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.Events
{
    public class LauncherFullTableEventArgs : EventArgs
    {
        public Rectangle PRegion;
        public Bitmap PTableName;
        public bool PBNewImg;
    }
}
