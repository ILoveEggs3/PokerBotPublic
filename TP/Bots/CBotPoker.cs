using Amigo.Controllers;
using Amigo.Core;
using Amigo.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amigo.Models.CAction;
using static Amigo.Core.CTableInfos;
using static Amigo.Models.CPlayer;

namespace Amigo.Bots
{
    public abstract class CBotPoker
    {
        protected CTableInfosNLHE2Max FFTableInfos;
        protected Stopwatch FFTimer;

        /// <summary>
        /// Represents the last street (preflop, flop turn or river) we were on at the last decision.
        /// </summary>
        protected ToursPossible FFStreetLastDecision;
        protected ToursPossible FFCurrentStreet;
        protected TypesPot FFPreflopTypePot;
        protected PossiblePositions FFHeroPosition;
        protected decimal FFEffectiveStacks;
        protected int FFEffectiveStacksRounded;
        protected string FFHeroCards;

        public CBotPoker()
        {
            FFTableInfos = null;
            FFTimer = Stopwatch.StartNew();
        }

        public abstract CAction GetDecision(CTableInfosNLHE2Max _headsUpTable, CGame2MaxManualController _simulatorThatRepresentCurrentGame = null);

        protected void InitializeNewInformations(CTableInfosNLHE2Max _headsUpTable)
        {
            #region Logging actions
            CLogger.AddHeaderLog();
            CLogger.AddLog(new CLog("-- NEW DECISION --"));
            CLogger.AddEmptyLineLog();
            #endregion

            if (_headsUpTable == null)
                throw new ArgumentNullException("_headsUpTable");

            #region Informations variables     
            FFStreetLastDecision = FFCurrentStreet; // Order is important for this variable
            FFCurrentStreet = _headsUpTable.GetTourActuel();

            if (FFCurrentStreet == ToursPossible.Preflop && FFStreetLastDecision == ToursPossible.River)
                FFStreetLastDecision = ToursPossible.Preflop;

            FFPreflopTypePot = _headsUpTable.GetTypePot();
            FFHeroPosition = _headsUpTable.PHero.PPosition;
            FFEffectiveStacks = _headsUpTable.GetEffectiveStacksInBB();
            FFEffectiveStacksRounded = Convert.ToInt32(Math.Round(FFEffectiveStacks, 0));
            FFHeroCards = (_headsUpTable.PHeroCards.Item1.ToString() + _headsUpTable.PHeroCards.Item2.ToString());
            #endregion
            #region Logging actions
            CLogger.AddLog(new CLog("Hero cards: " + FFHeroCards));
            CLogger.AddLog(new CLog("Hero position: " + FFHeroPosition));
            CLogger.AddLog(new CLog("Current street: " + FFCurrentStreet));
            CLogger.AddLog(new CLog("Preflop type pot: " + FFPreflopTypePot));
            CLogger.AddLog(new CLog("Effective stacks in BB: " + FFEffectiveStacks.ToString() + " BB"));
            CLogger.AddLog(new CLog("Effective stacks in BB (rounded): " + FFEffectiveStacksRounded + " BB"));
            CLogger.AddEmptyLineLog();
            #endregion

            FFTableInfos = _headsUpTable;
        }

        protected CAction RaiseAllIn()
        {
            CPlayer hero = FFTableInfos.PHero;
            CPlayer villain = FFTableInfos.PVillain;

            if (villain.PNumberOfChipsLeft == 0)
                return new CAction(ActionsPossible.Call, villain.PLastBet);
            else
                return new CAction(ActionsPossible.Raise, (hero.PNumberOfChipsLeft + hero.PLastBet));
        }

        protected CAction Raise(decimal _betSize)
        {
            return new CAction(ActionsPossible.Raise, _betSize);
        }

        protected CAction Check()
        {
            return new CAction(ActionsPossible.Check);
        }

        protected CAction Call()
        {
            return new CAction(ActionsPossible.Call, FFTableInfos.PVillain.PLastBet);
        }
        protected CAction Fold()
        {
            return new CAction(ActionsPossible.Fold);
        }
    }
}
