using System;

namespace Shared.Poker.Models
{
    public class CPlayer
    {
        public enum PokerPosition { BTN, BB, Unknown };

        private double FFNumberOfChipsLeft;
        /// <summary>
        /// Number of chips left in a CURRENT hand
        /// </summary>
        public double PNumberOfChipsLeft
        {
            set => FFNumberOfChipsLeft = Math.Round(value, 2);            
            get => FFNumberOfChipsLeft;            
        }

        private double FFNumberOfChipsAtBeginningHand;
        /// <summary>
        /// Number of chips that the player has at the beginning of a hand
        /// </summary>
        public double PNumberOfChipsAtBeginningHand
        {
            set => FFNumberOfChipsAtBeginningHand = Math.Round(value, 2);
            get => FFNumberOfChipsAtBeginningHand;
        }

        private double FFLastBet;
        /// <summary>
        /// Last bet on the current STREET (preflop, flop, turn or river) by the player. 
        /// Example: If the player bet on the flop, but checked on the turn and we're on the turn, then this means that this field should be at 0.
        /// </summary>
        public double PLastBet
        {
            set => FFLastBet = Math.Round(value, 2);
            get => FFLastBet;
        }

        public string PName { get; set; }

        private PokerPosition FFPosition;

        public PokerPosition PPosition
        {
            get { return FFPosition; }
            set
            {
                if (!Enum.IsDefined(typeof(PokerPosition), value))
                    throw new ArgumentException("The position received for the player is not valid!");

                FFPosition = value;
            }
        }

        public CSessionInfo PSessionInfo { private set; get; }

        public CPlayer(double _stackDepart, string _nom = null)
        {
            PName = _nom;
            PNumberOfChipsAtBeginningHand = _stackDepart;
            PNumberOfChipsLeft = _stackDepart;            
            PLastBet = 0;
            PPosition = PokerPosition.Unknown;
            PSessionInfo = new CSessionInfo();
        }

        public CPlayer(double _stackDepart, PokerPosition _positionOfPlayer, string _nom = null)
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

        public CPlayer Clone()
        {
            return new CPlayer(this);
        }
    }
}
