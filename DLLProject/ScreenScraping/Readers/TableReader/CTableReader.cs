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

namespace ScreenScraping.Readers.TableReader
{
    public abstract class CTableReader : CAbstractReader, ITableReader
    {
        protected readonly CReferences FFReferences;
        protected readonly CConstantsTable FFConstants;

        protected CTableReader(
            IntPtr _hwnd, 
            CReferences _references,
            CConstantsTable _constants,
            IScreenShotHelper _screenShotHelper) : base(_hwnd, _screenShotHelper)
        {
            FFReferences = _references;
            FFConstants = _constants;
        }

        public abstract bool IsOurTurn();

        public abstract Tuple<CCard, CCard> GetHand();

        public abstract List<CCard> GetFlop();

        public abstract CCard GetTurn();

        public abstract CCard GetRiver();

        public abstract decimal GetPot(CPokerPositionModel.TenMax _playerPos);

        public abstract decimal GetPlayerBet(CPokerPositionModel.TenMax _playerPos);

        public abstract decimal GetPlayerStack(CPokerPositionModel.TenMax _playerPos);

        public abstract bool IsHeroDealer();
    }
}
