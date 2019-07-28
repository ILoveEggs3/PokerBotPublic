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

namespace ScreenScraping.Readers.TableReader.SixMax.EspaceJeux
{
    public sealed class CReaderController : CTableReaderSixMax
    {
        public CReaderController(IntPtr _hwnd, IScreenShotHelper _screenShotHelper) : base(_hwnd, new Resources.TwoMax.EspaceJeux.References(), new Resources.TwoMax.EspaceJeux.Constantes(), _screenShotHelper)
        {}

        public override List<CCard> GetFlop()
        {
            throw new NotImplementedException();
        }

        public override Tuple<CCard, CCard> GetHand()
        {
            throw new NotImplementedException();
        }

        public override decimal GetPlayerBet(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }

        public override decimal GetPlayerStack(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }

        public override decimal GetPot(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }

        public override CCard GetRiver()
        {
            throw new NotImplementedException();
        }

        public override CCard GetTurn()
        {
            throw new NotImplementedException();
        }

        public override bool IsHeroDealer()
        {
            throw new NotImplementedException();
        }

        public override bool IsOurTurn()
        {
            throw new NotImplementedException();
        }

        protected override void InitializeMonitoringMembers()
        {
            throw new NotImplementedException();
        }
    }
}
