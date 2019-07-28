using Amigo.Controllers;
using HoldemHand;
using Shared.Helpers;
using Shared.Models;
using Shared.Poker.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Bots
{
    public abstract class CBotPoker
    {
        protected Stopwatch FFTimer;
        
        public CBotPoker()
        {
            FFTimer = new Stopwatch();
        }

        /// <summary>
        /// Gets the best EV+ decision to do from the current state of game.
        /// </summary>
        /// <param name="_currentGameState">Informations that are necessary in order to receive an accurate decision from the bot.</param>
        /// <returns>Returns the best EV+ decision.</returns>
        public abstract CAction GetDecision(AState _currentGameState, Hand _heroHand, int _indexPlayerThatIsPlaying);

        public Task<CAction> GetDecisionAsync(AState _currentGameState, Hand _heroHand, int _indexPlayerThatIsPlaying)
        {
            return Task.Run(() =>
            {
                return GetDecision(_currentGameState, _heroHand, _indexPlayerThatIsPlaying);
            });
        }

        public abstract void CreateNewHand();
    }
}
