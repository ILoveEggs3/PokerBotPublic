using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.ScreenCapture
{
    public interface IScreenShotHelper
    {
        Bitmap GetScreenshot(IntPtr ihandle);
    }
}
