using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerShared;
using Resources;
using ScreenScraping;
using System.Threading;
using ScreenScraping.ScreenCapture;
using ScreenScraping.Readers.Events;

namespace ScreenScraping.Readers.TableReader.SixMax
{
    public abstract class CTableReaderSixMax : CTableReader
    {
        protected CTableReaderSixMax(IntPtr _hwnd, CReferences _references, CConstantsTable _constants, IScreenShotHelper _screenShotHelper) : base(_hwnd, _references, _constants, _screenShotHelper)
        {}
    }
}
