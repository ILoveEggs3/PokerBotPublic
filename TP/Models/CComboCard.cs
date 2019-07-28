using HoldemHand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Amigo.Models.CAction;

namespace Amigo.Models
{
    public class CComboCard
    {
        public CCard PCard1 { private set; get; }
        public CCard PCard2 { private set; get; }

        /// <summary>
        /// Based on the action since the beginning of a hand, this is the probability that villain has the hand he claims to have.
        /// Example: If villain calls preflop 30% of the time with 92hh on the BB, he is always gonna check/call this type of flush draw on the flop. 
        /// HOWEVER, he doesn't have 92hh 100% of the time, even if he does check/call 100% of the time, he's only gonna have it 30% of the time, because
        /// he has the hand preflop 30% of the time!
        /// </summary>
        private decimal FFProbabilityThatHeHasTheHand;
        private decimal FFProbabilityCheckingCurrentStreet;
        private decimal FFProbabilityBettingCurrentStreet;
        private decimal FFProbabilityRaisingCurrentStreet;
        private decimal FFProbabilityCallingCurrentStreet;
        private decimal FFProbabilityFoldingCurrentStreet;

        public decimal PProbabilityCheckingCurrentStreet
        {
            set
            {
                decimal sumOfAllProbabilities = GetSumOfAllProbabilities();

                if (value + sumOfAllProbabilities > 1)
                    FFProbabilityCheckingCurrentStreet = (1 - GetSumOfAllProbabilities());
                else
                    FFProbabilityCheckingCurrentStreet = value;
            }
            get
            {
                return Math.Round(decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityCheckingCurrentStreet), 2);
            }
        }
        public decimal PProbabilityBettingCurrentStreet
        {
            set
            {
                decimal sumOfAllProbabilities = GetSumOfAllProbabilities();

                if (value + sumOfAllProbabilities > 1)
                    FFProbabilityBettingCurrentStreet = (1 - sumOfAllProbabilities);
                else
                    FFProbabilityBettingCurrentStreet = value;
            }
            get
            {
                return Math.Round(decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityBettingCurrentStreet), 2);
            }
        }

        public decimal PProbabilityRaisingCurrentStreet
        {
            set
            {
                decimal sumOfAllProbabilities = GetSumOfAllProbabilities();

                if (value + sumOfAllProbabilities > 1)
                    FFProbabilityRaisingCurrentStreet = (1 - sumOfAllProbabilities);
                else
                    FFProbabilityRaisingCurrentStreet = value;
            }
            get
            {
                return Math.Round(decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityRaisingCurrentStreet), 2);
            }
        }

        public decimal PProbabilityCallingCurrentStreet
        {
            set
            {
                decimal sumOfAllProbabilities = GetSumOfAllProbabilities();

                if (value + sumOfAllProbabilities > 1)
                    FFProbabilityCallingCurrentStreet = (1 - sumOfAllProbabilities);
                else
                    FFProbabilityCallingCurrentStreet = value;
            }
            get
            {
                return Math.Round(decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityCallingCurrentStreet), 2);
            }
        }

        public decimal PProbabilityFoldingCurrentStreet
        {
            set
            {
                decimal sumOfAllProbabilities = GetSumOfAllProbabilities();

                if (value + sumOfAllProbabilities > 1)
                    FFProbabilityFoldingCurrentStreet = (1 - sumOfAllProbabilities);
                else
                    FFProbabilityFoldingCurrentStreet = value;
            }
            get
            {
                return Math.Round(decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityFoldingCurrentStreet), 2);
            }
        }

        private decimal GetSumOfAllProbabilities()
        {
            return (PProbabilityCheckingCurrentStreet + PProbabilityBettingCurrentStreet + PProbabilityRaisingCurrentStreet + PProbabilityCallingCurrentStreet + PProbabilityFoldingCurrentStreet);
        }

        public void OnStreetChanged(ActionsPossible _action)
        {
            switch (_action)
            {
                case ActionsPossible.Bet:
                    FFProbabilityThatHeHasTheHand = decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityBettingCurrentStreet);                    
                    break;
                case ActionsPossible.Call:
                    FFProbabilityThatHeHasTheHand = decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityCallingCurrentStreet);
                    break;
                case ActionsPossible.Check:
                    FFProbabilityThatHeHasTheHand = decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityCheckingCurrentStreet);
                    break;
                case ActionsPossible.Raise:
                    FFProbabilityThatHeHasTheHand = decimal.Multiply(FFProbabilityThatHeHasTheHand, FFProbabilityRaisingCurrentStreet);
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

        public CComboCard(CCard _card1, CCard _card2, decimal _probabilityChecking, decimal _probabilityBetting, decimal _probabilityRaising, decimal _probabilityCalling, decimal _probabilityFolding)
        {
            if (_card1 == null)
                throw new ArgumentNullException("_card1");
            else if (_card2 == null)
                throw new ArgumentNullException("_card2");

            PCard1 = _card1;
            PCard2 = _card2;
            PProbabilityCheckingCurrentStreet = _probabilityChecking;
            PProbabilityBettingCurrentStreet = _probabilityBetting;
            PProbabilityRaisingCurrentStreet = _probabilityRaising;
            PProbabilityCallingCurrentStreet = _probabilityCalling;
            PProbabilityFoldingCurrentStreet = _probabilityFolding;
            FFProbabilityThatHeHasTheHand = 1;
        }

        public Hand ToHoldemHand()
        {
            Hand convertedHand = new Hand();
            convertedHand.PocketCards = PCard1.ToString() + PCard2.ToString();

            return convertedHand;
        }
    }
}
