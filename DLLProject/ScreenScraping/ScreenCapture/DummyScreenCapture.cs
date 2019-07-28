using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.ScreenCapture
{
    public class DummyScreenCapture : IScreenShotHelper
    {
        private static volatile DummyScreenCapture FFInstance = new DummyScreenCapture();
        private Bitmap FFReturningBmp = new Bitmap(1, 1);

        private DummyScreenCapture() { }

        public static DummyScreenCapture PInstance
        {
            get { return FFInstance; }
        }
        public Bitmap GetScreenshot(IntPtr ihandle)
        {
            return FFReturningBmp;
        }

        public void SetBitmap(Bitmap _bmp)
        {
            FFReturningBmp = _bmp;
        }
    }
}
