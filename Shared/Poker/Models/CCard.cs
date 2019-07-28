using System;
using HoldemHand;

namespace Shared.Poker.Models
{
    public class CCard : ICloneable
    {
        // Les énumérés sont basés sur les charactères ASCII pour les indices.
        public enum Value
        {
            Ace = 65,
            Two = 50, Three = 51, Four = 52, Five = 53, Six = 54, Seven = 55, Eight = 56, Nine = 57, Ten = 84,
            Jack = 74,
            Queen = 81,
            King = 75
        }

        public enum Type
        {
            Spades = 115,
            Hearts = 104,
            Diamonds = 100,
            Clubs = 99
        }

        public enum HandStrength { Bad, Medium, Good, Best }

        public Value PValue
        {
            private set;
            get;
        }

        public Type PType
        {
            private set;
            get;
        }

        public ulong PMask
        {
            //Not Optimal at all
            get { return ToMask(); }
        }

        public Hand PHand;

        private ulong ToMask()
        {
            ulong mask = 0;
            int value = 0;
            switch (PValue)
            {
                case CCard.Value.Ace:
                    value = Hand.RankAce;
                    break;
                case CCard.Value.Two:
                    value = Hand.Rank2;
                    break;
                case CCard.Value.Three:
                    value = Hand.Rank3;
                    break;
                case CCard.Value.Four:
                    value = Hand.Rank4;
                    break;
                case CCard.Value.Five:
                    value = Hand.Rank5;
                    break;
                case CCard.Value.Six:
                    value = Hand.Rank6;
                    break;
                case CCard.Value.Seven:
                    value = Hand.Rank7;
                    break;
                case CCard.Value.Eight:
                    value = Hand.Rank8;
                    break;
                case CCard.Value.Nine:
                    value = Hand.Rank9;
                    break;
                case CCard.Value.Ten:
                    value = Hand.RankTen;
                    break;
                case CCard.Value.Jack:
                    value = Hand.RankJack;
                    break;
                case CCard.Value.Queen:
                    value = Hand.RankQueen;
                    break;
                case CCard.Value.King:
                    value = Hand.RankKing;
                    break;
                default:
                    throw new Exception("Bad Card Format");
            }
            int type = 0;
            switch (PType)
            {
                case CCard.Type.Spades:
                    type = Hand.Spades;
                    break;
                case CCard.Type.Hearts:
                    type = Hand.Hearts;
                    break;
                case CCard.Type.Diamonds:
                    type = Hand.Diamonds;
                    break;
                case CCard.Type.Clubs:
                    type = Hand.Clubs;
                    break;
                default:
                    throw new Exception("Bad Card Format");
            }
            mask |= (ulong)0x1 << (value + type * 13);

            return mask;
        }

        public CCard(Value _cardValue, Type _cardType)
        {
            if (!Enum.IsDefined(typeof(Value), _cardValue) || !Enum.IsDefined(typeof(Type), _cardType))
                throw new ArgumentException();

            PValue = _cardValue;
            PType = _cardType;
            PHand = new Hand();
            PHand.PocketMask = ToMask();
        }

        public CCard(CCard _card)
        {
            PValue = _card.PValue;
            PType = _card.PType;
            PHand = new Hand();
            PHand.PocketMask = ToMask();
        }

        public static bool operator ==(CCard _card1, CCard _card2)
        {
            return _card1?.PMask == _card2?.PMask;
        }

        public override bool Equals(object _card2)
        {
            // Is null?
            if (ReferenceEquals(null, _card2))            
                return false;            

            // Is the same object?
            if (ReferenceEquals(this, _card2))            
                return true;
            
            // Is the same type?
            if (_card2.GetType() != GetType())            
                return false;            

            return PMask == ((CCard)_card2).PMask;
        }

        public static bool operator !=(CCard _card1, CCard _card2)
        {
            return !(_card1 == _card2);
        }

        public static bool operator >(CCard _card1, CCard _card2)
        {
            switch (_card1.PValue)
            {
                case Value.Ace:
                    return (_card1.PValue != _card2.PValue);
                case Value.Two:
                    return false;
                case Value.Three:
                    return (_card2.PValue == Value.Two);
                case Value.Four:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three);
                case Value.Five:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four);
                case Value.Six:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five);
                case Value.Seven:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six);
                case Value.Eight:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven);
                case Value.Nine:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven || _card2.PValue == Value.Eight);
                case Value.Ten:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven || _card2.PValue == Value.Eight || _card2.PValue == Value.Nine);
                case Value.Jack:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven || _card2.PValue == Value.Eight || _card2.PValue == Value.Nine || _card2.PValue == Value.Ten);
                case Value.Queen:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven || _card2.PValue == Value.Eight || _card2.PValue == Value.Nine || _card2.PValue == Value.Ten || _card2.PValue == Value.Jack);
                case Value.King:
                    return (_card2.PValue == Value.Two || _card2.PValue == Value.Three || _card2.PValue == Value.Four || _card2.PValue == Value.Five || _card2.PValue == Value.Six || _card2.PValue == Value.Seven || _card2.PValue == Value.Eight || _card2.PValue == Value.Nine || _card2.PValue == Value.Ten || _card2.PValue == Value.Jack || _card2.PValue == Value.Queen);
                default:
                    throw new Exception("Unable to detect the value of the card 1!");
            }
        }

        public static bool operator <(CCard _card1, CCard _card2)
        {
            return ((!(_card1 > _card2)) && (_card1.PValue != _card2.PValue));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Concat((char)(PValue), (char)PType);
        }

        public object Clone()
        {
            return new CCard(this);
        }
    }
}
