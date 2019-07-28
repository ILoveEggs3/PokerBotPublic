using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models
{
    public class CSessionInfo: ICloneable
    {
        /// <summary>
        /// Nombre de profit que le joueur a fait dans une session
        /// </summary>
        public decimal PNbProfit { get; set; }

        public decimal PNbWins { get; set; }

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
