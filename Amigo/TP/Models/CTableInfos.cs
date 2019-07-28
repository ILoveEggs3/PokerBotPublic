using Shared.Helpers;
using Shared.Models;
using Shared.Poker.Helpers;
using System;
using static Shared.Poker.Models.CAction;

namespace Shared.Poker.Models
{
    /// <summary>
    /// Informations sur une table de poker ouverte "présentement" sur un ordinateur.
    /// </summary>
    public abstract class CTableInfos
    {
        public enum Street { Preflop, Flop, Turn, River };
        public enum TypesPot { Limped, RaisedLimped, LimpedThreeBet, LimpedFourBetEtPlus, OneBet, TwoBet, ThreeBet, FourBet, FiveBetEtPlus };

        private double FFSmallBlind;

        public double PSmallBlind
        {
            get { return FFSmallBlind; }
        }

        private double FFBigBlind;

        public double PBigBlind
        {
            get { return FFBigBlind; }
        }

        private double FFAntes;

        public double PAntes
        {
            get { return FFAntes; }
        }

        public double PPot
        {
            set;
            get;
        }

        public CBoard PBoard { private set; get; }

        public Tuple<CCard, CCard> PHeroCards { private set; get; }

        public CTableInfos(double _smallBlind, double _bigBlind, double _antes, CBoard _board, Tuple<CCard, CCard> _heroCards)
        {
            if (_smallBlind <= 0)
                throw new ArgumentOutOfRangeException("Small blind must be greater than 0");
            else if (_bigBlind <= 0 || _bigBlind < _smallBlind)
                throw new ArgumentOutOfRangeException("Big blind must be greater than 0 and greater or equal than the small blind.");
            else if (_antes < 0)
                throw new ArgumentOutOfRangeException("Antes must be greater or equal to 0");
            else if (_board == null)
                throw new ArgumentNullException("_board");
            else if (_heroCards == null)
                throw new ArgumentNullException("_heroCards");
            else if (_heroCards.Item1 == null || _heroCards.Item2 == null)
                throw new ArgumentNullException("Card1 or card2 of the hero is null. This should never happen!");

            FFSmallBlind = _smallBlind;
            FFBigBlind = _bigBlind;
            FFAntes = _antes;
            PPot = 0;
            PBoard = _board;
            PHeroCards = _heroCards;
        }



        public abstract Street GetTourActuel();
        public abstract TypesPot GetTypePot();
        public abstract double GetEffectivePot();
    }
}
