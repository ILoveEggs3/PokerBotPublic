using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerShared;
using Resources;
using ScreenScraping;
using ScreenScraping.ScreenCapture;

namespace ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux
{
    public sealed class CReaderController : CTableReaderTwoMax
    {

        public CReaderController(IntPtr _hwnd, IScreenShotHelper _screenShotHelper) : base(_hwnd, new Resources.TwoMax.EspaceJeux.References(), new Resources.TwoMax.EspaceJeux.Constantes(), _screenShotHelper)
        {}
    }
}
