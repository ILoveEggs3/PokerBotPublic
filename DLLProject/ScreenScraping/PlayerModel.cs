using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping
{
    public class PlayerModel
    {
        public decimal Stack { get; set; }
        public decimal Bet { get; set; }
        public PokerShared.CPokerPositionModel.TenMax Position { get; set; }
    }
}
