using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amigo.Core;
using Amigo.Controllers;
using Amigo.Models;

namespace Amigo.Bots
{
    public class CBotPokerJamEverything : CBotPoker
    {
        public override CAction GetDecision(CTableInfosNLHE2Max _headsUpTable, CGame2MaxManualController _simulatorThatRepresentCurrentGame = null)
        {
            base.InitializeNewInformations(_headsUpTable);
            return RaiseAllIn();
        }
    }
}
