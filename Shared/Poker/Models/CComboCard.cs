using HoldemHand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandType = HoldemHand.Hand.HandTypes;

using static Shared.Poker.Models.CAction;
using Amigo.Helpers;
using Shared.Helpers;

namespace Shared.Poker.Models
{
    public class CComboCard: ICloneable
    {
        public enum KickerType { Top, Good, Medium, Bad }


        public CCard PCard1 { private set; get; }
        public CCard PCard2 { private set; get; }
        public ulong PMask
        {
            get
            {
                return PCard1.PMask | PCard2.PMask;
            }
        }

        /// <summary>
        /// Based on the action since the beginning of a hand, this is the probability that villain has the hand he claims to have.
        /// Example: If villain calls preflop 30% of the time with 92hh on the BB, he is always gonna check/call this type of flush draw on the flop. 
        /// HOWEVER, he doesn't have 92hh 100% of the time, even if he does check/call 100% of the time, he's only gonna have it 30% of the time, because
        /// he has the hand preflop 30% of the time!
        /// </summary>
        public double PProbabilityThatHeHasTheHand;
        private double FFProbabilityCheckingCurrentStreet;
        private double FFProbabilityBettingCurrentStreet;
        private double FFProbabilityRaisingCurrentStreet;
        private double FFProbabilityCallingCurrentStreet;
        private double FFProbabilityFoldingCurrentStreet;

        public double PProbabilityCheckingCurrentStreet
        {
            set
            {
                FFProbabilityCheckingCurrentStreet = value;
            }
            get
            {
                double probabilityCheckingCurrentStreet = (FFProbabilityCheckingCurrentStreet / GetSumOfAllProbabilities());

                return Math.Round((PProbabilityThatHeHasTheHand / probabilityCheckingCurrentStreet) * 2);
            }
        }
        public double PProbabilityBettingCurrentStreet
        {
            set
            {
                FFProbabilityBettingCurrentStreet = value;
            }
            get
            {
                double probabilityBettingCurrentStreet = (FFProbabilityBettingCurrentStreet / GetSumOfAllProbabilities());

                return Math.Round((PProbabilityThatHeHasTheHand * probabilityBettingCurrentStreet) / 2);
            }
        }

        public double PProbabilityRaisingCurrentStreet
        {
            set
            {
                FFProbabilityRaisingCurrentStreet = value;
            }
            get
            {
                double probabilityRaisingCurrentStreet = (FFProbabilityRaisingCurrentStreet / GetSumOfAllProbabilities());

                return Math.Round((PProbabilityThatHeHasTheHand * probabilityRaisingCurrentStreet), 2);
            }
        }

        public double PProbabilityCallingCurrentStreet
        {
            set
            {
                FFProbabilityCallingCurrentStreet = value;
            }
            get
            {
                double probabilityCallingCurrentStreet = (FFProbabilityCallingCurrentStreet / GetSumOfAllProbabilities());

                return Math.Round((PProbabilityThatHeHasTheHand * probabilityCallingCurrentStreet), 2);
            }
        }

        public double PProbabilityFoldingCurrentStreet
        {
            set
            {
                FFProbabilityFoldingCurrentStreet = value;
            }
            get
            {
                double probabilityFoldingCurrentStreet = (FFProbabilityFoldingCurrentStreet / GetSumOfAllProbabilities());

                return Math.Round((PProbabilityThatHeHasTheHand * probabilityFoldingCurrentStreet), 2);
            }
        }

        private double GetSumOfAllProbabilities()
        {
            return (FFProbabilityCheckingCurrentStreet + FFProbabilityBettingCurrentStreet + FFProbabilityRaisingCurrentStreet + FFProbabilityCallingCurrentStreet + FFProbabilityFoldingCurrentStreet);
        }

        public void OnStreetChanged(PokerAction _action)
        {
            switch (_action)
            {
                case PokerAction.Bet:
                    double probabilityBettingCurrentStreet = (FFProbabilityBettingCurrentStreet / GetSumOfAllProbabilities());

                    PProbabilityThatHeHasTheHand = (PProbabilityThatHeHasTheHand * probabilityBettingCurrentStreet);                    
                    break;
                case PokerAction.Call:
                    double probabilityCallingCurrentStreet = (FFProbabilityCallingCurrentStreet / GetSumOfAllProbabilities());

                    PProbabilityThatHeHasTheHand = (PProbabilityThatHeHasTheHand * probabilityCallingCurrentStreet);
                    break;
                case PokerAction.Check:
                    double probabilityCheckingCurrentStreet = (FFProbabilityCheckingCurrentStreet / GetSumOfAllProbabilities());

                    PProbabilityThatHeHasTheHand = (PProbabilityThatHeHasTheHand * probabilityCheckingCurrentStreet);
                    break;
                case PokerAction.Raise:
                    double probabilityRaisingCurrentStreet = (FFProbabilityRaisingCurrentStreet / GetSumOfAllProbabilities());

                    PProbabilityThatHeHasTheHand = (PProbabilityThatHeHasTheHand * probabilityRaisingCurrentStreet);
                    break;
            }

            ResetProbabilitiesToZero();
        }

        private void ResetProbabilitiesToZero()
        {
            PProbabilityBettingCurrentStreet = 0;
            PProbabilityCheckingCurrentStreet = 0;
            PProbabilityFoldingCurrentStreet = 0;
            PProbabilityRaisingCurrentStreet = 0;
            PProbabilityCallingCurrentStreet = 0;
        }

        public CComboCard(CCard _card1, CCard _card2, double _probabilityHeHasTheHand)
        {
            if (_card1 == null)
                throw new ArgumentNullException("_card1");
            else if (_card2 == null)
                throw new ArgumentNullException("_card2");

            PCard1 = _card1;
            PCard2 = _card2;
            PProbabilityCheckingCurrentStreet = 0;
            PProbabilityBettingCurrentStreet = 0;
            PProbabilityRaisingCurrentStreet = 0;
            PProbabilityCallingCurrentStreet = 0;
            PProbabilityFoldingCurrentStreet = 0;
            PProbabilityThatHeHasTheHand = _probabilityHeHasTheHand;
        }

        public CComboCard(ulong _pocketMask, double _probabilityHeHasTheHand)
        {
            IEnumerable<string> cardsEnumerated = Hand.Cards(_pocketMask);

            PCard1 = cardsEnumerated.ElementAt(0).ToCCarte();
            PCard2 = cardsEnumerated.ElementAt(1).ToCCarte();
            PProbabilityCheckingCurrentStreet = 0;
            PProbabilityBettingCurrentStreet = 0;
            PProbabilityRaisingCurrentStreet = 0;
            PProbabilityCallingCurrentStreet = 0;
            PProbabilityFoldingCurrentStreet = 0;
            PProbabilityThatHeHasTheHand = _probabilityHeHasTheHand;
        }

        private CComboCard(CComboCard _objectToClone)
        {
            PCard1 = (CCard)_objectToClone.PCard1.Clone();
            PCard2 = (CCard)_objectToClone.PCard2.Clone();
            FFProbabilityCheckingCurrentStreet = _objectToClone.FFProbabilityCheckingCurrentStreet;
            FFProbabilityBettingCurrentStreet = _objectToClone.FFProbabilityBettingCurrentStreet;
            FFProbabilityRaisingCurrentStreet = _objectToClone.FFProbabilityRaisingCurrentStreet;
            FFProbabilityCallingCurrentStreet = _objectToClone.FFProbabilityCallingCurrentStreet;
            FFProbabilityFoldingCurrentStreet = _objectToClone.FFProbabilityFoldingCurrentStreet;
            PProbabilityThatHeHasTheHand = _objectToClone.PProbabilityThatHeHasTheHand;
        }

        public static bool operator ==(CComboCard _combo1, CComboCard _combo2)
        {
            // AhKh AhKh OR AhKh KhAh OR KhAh AhKh OR KhAh KhAh --> The returns covers these 4 situations
            return ((_combo1.PCard1 == _combo2.PCard1 && _combo1.PCard2 == _combo2.PCard2) || (_combo1.PCard1 == _combo2.PCard2 && _combo1.PCard2 == _combo2.PCard1));
        }

        public override bool Equals(object _combo2)
        {
            // Is null?
            if (ReferenceEquals(null, _combo2))
                return false;

            // Is the same object?
            if (ReferenceEquals(this, _combo2))
                return true;

            // Is the same type?
            if (_combo2.GetType() != GetType())
                return false;

            return (this == ((CComboCard)_combo2));
        }

        public static bool operator !=(CComboCard _combo1, CComboCard _combo2)
        {
            // AhKh AhKh OR AhKh KhAh OR KhAh AhKh OR KhAh KhAh --> The returns covers these 4 situations
            return !(_combo1 == _combo2);
        }

        public CCard.Value GetHighestCard()
        {
            if (PCard1 > PCard2)
                return PCard1.PValue;
            else
                return PCard2.PValue;
        }

        public Hand ToHoldemHand()
        {
            Hand convertedHand = new Hand();
            convertedHand.PocketCards = PCard1.ToString() + PCard2.ToString();

            return convertedHand;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            return new CComboCard(this);
        }
    }
}
