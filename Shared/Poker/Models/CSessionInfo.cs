using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poker.Models
{
    public class CSessionInfo: ICloneable
    {
        /// <summary>
        /// Nombre de profit que le joueur a fait dans une session
        /// </summary>
        public double PNbProfit { get; set; }

        public double PNbWins { get; set; }

        public CSessionInfo()
        {
            PNbProfit = 0;
            PNbWins = 0;
        }

        public CSessionInfo(CSessionInfo _session)
        {
            PNbProfit = _session.PNbProfit;
            PNbWins = _session.PNbWins;
        }

        public object Clone()
        {
            return new CSessionInfo(this);
        }
    }
}
