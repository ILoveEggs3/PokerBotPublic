using Amigo.Controllers;
using HoldemHand;
using Shared.Poker.Models;
using static Shared.Poker.Models.CAction;

namespace Amigo.Bots
{
    public class CBotPokerJamEverything : CBotPoker
    {
        public override void CreateNewHand()
        {
        }

        public override CAction GetDecision(AState _currentGameState, Hand _heroHand, int _indexPlayerThatIsPlaying)
        {
            CPlayer hero = _currentGameState.GetHeroPlayer();

            CAction Call()
            {
                return new CAction(PokerAction.Call, _currentGameState.PLastBet);
            }
            CAction Raise(double _amount)
            {
                return new CAction(PokerAction.Raise, _amount);
            }
            CAction RaiseAllIn()
            {
                if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Raise))
                    return Raise(hero.PNumberOfChipsLeft + hero.PLastBet);
                else
                    return Call();
            }

            return RaiseAllIn();
        }
    }
}
