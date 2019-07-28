using System;

namespace Amigo.Models
{
    public class CCard: ICloneable
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

        public CCard(Value _cardValue, Type _cardType)
        {
           if (!Enum.IsDefined(typeof(Value), _cardValue) || !Enum.IsDefined(typeof(Type), _cardType))                
               throw new ArgumentException();

            PValue = _cardValue;
            PType = _cardType;
        }

        public CCard(CCard _card)
        {
            PValue = _card.PValue;
            PType = _card.PType;
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
