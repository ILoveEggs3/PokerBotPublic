using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amigo.Controllers;
using HoldemHand;
using Shared.Poker.Helpers;
using Shared.Poker.Models;
using static Shared.Poker.Models.CAction;

namespace Amigo.Bots
{
    public class CBotPokerIFoldEverything : CBotPoker
    {
        public override void CreateNewHand()
        {
        }


        public override CAction GetDecision(AState _currentGameState, Hand _heroHand, int _indexPlayerThatIsPlaying)
        {
            CPlayer hero = _currentGameState.GetHeroPlayer();

            CAction Check()
            {
                return new CAction(PokerAction.Check);
            }
            CAction Bet(double _amount)
            {
                return new CAction(PokerAction.Bet, _amount);
            }
            CAction Call()
            {
                return new CAction(PokerAction.Call, _currentGameState.PLastBet);
            }
            CAction Raise(double _amount)
            {
                return new CAction(PokerAction.Raise, _amount);
            }
            CAction Fold()
            {
                return new CAction(PokerAction.Fold);
            }
            CAction RaiseAllIn()
            {
                if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Raise))
                    return Raise(hero.PNumberOfChipsLeft + hero.PLastBet);
                else
                    return Call();
            }

            var typePot = _currentGameState.PTypePot;

            switch (_currentGameState.PCurrentStreet)
            {
                case CTableInfos.Street.Preflop:
                    if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AA"))
                        return RaiseAllIn();
                    else if (typePot == CTableInfos.TypesPot.OneBet)
                    {
                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2s+ K2s+ Q2s+ J2s+ T2s+ 92s+ 82s+ 74s+ 63s+ 52s+ 42s+ 32s A2o+ K2o+ Q4o+ J5o+ T6o+ 97o+ 86o+ 76o"))
                            return new CAction(CAction.PokerAction.Raise, 2);
                        else if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Check))
                            return Check();
                        else
                            return Fold();
                    }
                    else if (typePot == CTableInfos.TypesPot.ThreeBet)
                    {
                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+"))
                            return RaiseAllIn();
                        else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "Q8s+ J9s+ A2s+ K9s+ 22+ QJo+ KJo+ 67s+"))
                            return Call();
                        else if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Check))
                            return Check();
                        else
                            return Fold();
                    }
                    else if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Check))
                        return Check();
                    else
                        return Fold();
                default:
                    if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Check))
                        return Check();
                    else
                        return Fold();
            }
        }
    }
}
