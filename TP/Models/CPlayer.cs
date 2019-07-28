using System;

namespace Amigo.Models
{
    public class CPlayer: ICloneable
    {
        public enum PossiblePositions { BTN, BB, Unknown };

        /// <summary>
        /// Number of chips left in a CURRENT hand
        /// </summary>
        public decimal PNumberOfChipsLeft { set; get;}
        /// <summary>
        /// Number of chips that the player has at the beginning of a hand
        /// </summary>
        public decimal PNumberOfChipsAtBeginningHand { set; get; }
        /// <summary>
        /// Last bet on the current STREET (preflop, flop, turn or river) by the player. 
        /// Example: If the player bet on the flop, but checked on the turn and we're on the turn, then this means that this field should be at 0.
        /// </summary>
        public decimal PLastBet { set; get; }

        public string PName { get; set; }

        private PossiblePositions FFPosition;

        public PossiblePositions PPosition
        {
            get { return FFPosition; }
            set
            {
                if (!Enum.IsDefined(typeof(PossiblePositions), value))
                    throw new ArgumentException("The position received for the player is not valid!");

                FFPosition = value;
            }
        }

        public CSessionInfo PSessionInfo { private set; get; }

        public CPlayer(decimal _stackDepart, string _nom = null)
        {
            PName = _nom;
            PNumberOfChipsAtBeginningHand = _stackDepart;
            PNumberOfChipsLeft = _stackDepart;            
            PLastBet = 0;
            PPosition = PossiblePositions.Unknown;
            PSessionInfo = new CSessionInfo();
        }

        public CPlayer(decimal _stackDepart, PossiblePositions _positionOfPlayer, string _nom = null)
        {
            PName = _nom;
            PNumberOfChipsLeft = _stackDepart;
            PLastBet = 0;
            PPosition = _positionOfPlayer;
            PSessionInfo = new CSessionInfo();
        }

        private CPlayer(CPlayer _player)
        {
            if (_player == null)
                throw new ArgumentNullException("_player");

            PName = _player.PName;
            PNumberOfChipsAtBeginningHand = _player.PNumberOfChipsAtBeginningHand;
            PNumberOfChipsLeft = _player.PNumberOfChipsLeft;
            PLastBet = _player.PLastBet;
            PPosition = _player.PPosition;
            PSessionInfo = (CSessionInfo)_player.PSessionInfo.Clone();
        }

        public decimal ToBB(decimal _bigBlind, bool _includeHisBetIfThereIsOne)
        {
            if (_includeHisBetIfThereIsOne)
                return decimal.Divide(PNumberOfChipsLeft + PLastBet, _bigBlind);
            else
                return decimal.Divide(PNumberOfChipsLeft, _bigBlind);
        }

        public object Clone()
        {
            return new CPlayer(this);
        }
    }
}
