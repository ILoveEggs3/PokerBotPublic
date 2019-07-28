using Amigo.Helpers;
using Amigo.Models;
using System;
using static Amigo.Controllers.CGameController;

namespace Amigo.Core
{
    /// <summary>
    /// Informations sur une table de poker ouverte "présentement" sur un ordinateur.
    /// </summary>
    public abstract class CTableInfos
    {
        public enum ToursPossible { Preflop, Flop, Turn, River };
        public enum TypesPot { Limped, RaisedLimped, LimpedThreeBet, OneBet, TwoBet, ThreeBet, FourBet, FiveBetEtPlus };

        private decimal FFSmallBlind;

        public decimal PSmallBlind
        {
            get { return FFSmallBlind; }
        }

        private decimal FFBigBlind;

        public decimal PBigBlind
        {
            get { return FFBigBlind; }
        }

        private decimal FFAntes;

        public decimal PAntes
        {
            get { return FFAntes; }
        }

        public decimal PPot
        {
            private set;
            get;
        }

        public string PBoard { private set; get; }


        public Tuple<CCard, CCard> PHeroCards { private set; get; }

        public CTableInfos(decimal _smallBlind, decimal _bigBlind, decimal _antes, decimal _pot, string _board, Tuple<CCard, CCard> _heroCards)
        {
            if (_smallBlind <= 0)
                throw new ArgumentOutOfRangeException("Small blind must be greater than 0");
            else if (_bigBlind <= 0 || _bigBlind < _smallBlind)
                throw new ArgumentOutOfRangeException("Big blind must be greater than 0 and greater or equal than the small blind.");
            else if (_antes < 0)
                throw new ArgumentOutOfRangeException("Antes must be greater or equal to 0");
            else if (_pot <= 0)
                throw new ArgumentOutOfRangeException("Pot must be greater than 0, since there will always be atleast a small blind and a big blind");
            else if (_board == null)
                throw new ArgumentNullException("_board");
            else if (_heroCards == null)
                throw new ArgumentNullException("_heroCards");
            else if (_heroCards.Item1 == null || _heroCards.Item2 == null)
                throw new ArgumentNullException("Card1 or card2 of the hero is null. This should never happen!");

            FFSmallBlind = _smallBlind;
            FFBigBlind = _bigBlind;
            FFAntes = _antes;
            PPot = _pot;
            PBoard = _board;
            PHeroCards = _heroCards;
        }

        /// <summary>
        /// Returns true if our hand is in the received range.
        /// </summary>
        /// <param name="_range">Range must be in this format (22+ AJo+ AJs+ QJs).</param>
        /// <returns>Returns true if our hand is in the received range. Otherwise, it returns false.</returns>
        public bool isOurHandInThisRange(string _range)
        {
            CLogger.AddLog(new CLog("Is our hand in this range? " + _range));

            string[] theTable = CPokerRangeConverter.GetInstance().ConvertRange(_range);
            string hand1 = PHeroCards.Item1.ToString() + PHeroCards.Item2.ToString();
            string hand2 = PHeroCards.Item2.ToString() + PHeroCards.Item1.ToString();

            foreach (string theComboInTheTableFromTheRangeConverter in theTable)
            {
                if (theComboInTheTableFromTheRangeConverter == hand1 || theComboInTheTableFromTheRangeConverter == hand2)
                {
                    CLogger.AddLog(new CLog("True"));
                    return true;
                }
            }

            CLogger.AddLog(new CLog("False"));
            return false;
        }

        public abstract ToursPossible GetTourActuel();
        public abstract TypesPot GetTypePot();
        public abstract decimal GetEffectiveStacksInBB();
        public abstract decimal GetEffectivePot();
    }
}
