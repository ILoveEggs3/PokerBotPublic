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
    public class CBotPokerIFoldEverything : CBotPoker
    {
        public override CAction GetDecision(CTableInfosNLHE2Max _headsUpTable, CGame2MaxManualController _simulatorThatRepresentCurrentGame = null)
        {
            base.InitializeNewInformations(_headsUpTable);

            switch (FFCurrentStreet)
            {
                case CTableInfos.ToursPossible.Preflop:
                    if (_headsUpTable.isOurHandInThisRange("AA"))
                        return RaiseAllIn();
                    else if (FFPreflopTypePot == CTableInfos.TypesPot.OneBet)
                    {
                        if (_headsUpTable.isOurHandInThisRange("22+ A2s+ K2s+ Q2s+ J2s+ T2s+ 92s+ 82s+ 74s+ 63s+ 52s+ 42s+ 32s A2o+ K2o+ Q4o+ J5o+ T6o+ 97o+ 86o+ 76o"))
                            return new CAction(CAction.ActionsPossible.Raise, 2);
                        else
                            return Fold();
                    }                        
                    else if (FFPreflopTypePot == CTableInfos.TypesPot.ThreeBet)
                    {
                        if (_headsUpTable.isOurHandInThisRange("JJ+"))
                            return RaiseAllIn();
                        else if (_headsUpTable.isOurHandInThisRange("Q8s+ J9s+ A2s+ K9s+ 22+ QJo+ KJo+ 67s+"))
                            return Call();
                        else
                            return Fold();
                    }
                    else
                        return Fold();
                    break;
                default:
                    if (_simulatorThatRepresentCurrentGame.GetAllowedActions().Contains(CAction.ActionsPossible.Call))
                        return Fold();
                    else
                        return Check();
            }         
        }
    }
}
