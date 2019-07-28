using System;
using System.Runtime.Serialization;
using HandHistories.Objects.Cards;

namespace HandHistories.Objects.Actions
{
    [DataContract]
    [KnownType(typeof(WinningsAction))]
    [KnownType(typeof(AllInAction))]
    public class HandAction
    {
        [DataMember]
        public string PlayerName { get; private set; }

        [DataMember]
        public HandActionType HandActionType { get; protected set; }

        [DataMember]
        public double Amount { get; private set; }

        [DataMember]
        public Street Street { get; private set; }

        [DataMember]
        public int ActionNumber { get; set; }

        [DataMember]
        public bool IsAllIn { get; private set; }

        public HandAction(string playerName,
            HandActionType handActionType,
            double amount,
            Street street,
            int actionNumber)
            : this(playerName, handActionType, amount, street, false, actionNumber)
        {
        }

        public HandAction(string playerName,
                          HandActionType handActionType,
                          Street street,
                          bool AllInAction = false,
                          int actionNumber = 0)
        {
            Street = street;
            HandActionType = handActionType;
            PlayerName = playerName;
            Amount = 0;
            ActionNumber = actionNumber;
            IsAllIn = AllInAction;
        }

        public HandAction(string playerName, 
                          HandActionType handActionType,                           
                          double amount,
                          Street street,
                          bool AllInAction = false,
                          int actionNumber = 0)
        {
            Street = street;
            HandActionType = handActionType;
            PlayerName = playerName;
            Amount = GetAdjustedAmount(amount, handActionType);
            ActionNumber = actionNumber;
            IsAllIn = AllInAction;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            HandAction handAction = obj as HandAction;
            if (handAction == null) return false;

            return handAction.ToString().Equals(ToString());
        }

        public override string ToString()
        {
            string format = "{0} does {1} for {2} on street {3}{4}";

            return string.Format(format,
                PlayerName,
                HandActionType,
                Amount.ToString("N2"),
                Street,
                IsAllIn ? " and is All-In" : "");
        }

        public void DecreaseAmount(double value)
        {
            //Amount = Math.Abs(Amount) - Math.Abs(value);
            //Amount = GetAdjustedAmount(Amount, HandActionType);
        }

        /// <summary>
        /// Actions like calling, betting, raising should be negative amounts.
        /// Actions such as winning should be positive.
        /// Actions such as chatting should be 0 and can cause false positives if people say certain things.
        /// </summary>
        /// <param name="amount">The amount in the action.</param>
        /// <param name="type">The type of the action.</param>
        /// <returns></returns>
        public static double GetAdjustedAmount(double amount, HandActionType type)
        {
            if (amount == 0)
            {
                return 0;
            }

            amount = Math.Abs(amount);

            switch (type)
            {
                case HandActionType.CALL:
                    return amount;                    
                case HandActionType.WINS:
                    return amount;                   
                case HandActionType.WINS_SIDE_POT:
                    return amount;                   
                case HandActionType.TIES:
                    return amount;
                case HandActionType.RAISE:
                    return amount;
                case HandActionType.ALL_IN:
                    return amount;
                case HandActionType.BET:
                    return amount;
                case HandActionType.SMALL_BLIND:
                    return amount;
                case HandActionType.BIG_BLIND:
                    return amount;
                case HandActionType.UNCALLED_BET:
                    return amount;
                case HandActionType.POSTS:
                    return amount;
                case HandActionType.ANTE:
                    return amount;
                case HandActionType.WINS_THE_LOW:
                    return amount;
                case HandActionType.ADDS:
                    return 0.0; // when someone adds to their stack it doesnt effect their winnings in the hand
                case HandActionType.CHAT:
                    return 0.0; // overwrite any $ talk in the chat
                case HandActionType.JACKPOTCONTRIBUTION:
                    return 0.0; // does not affect pot, as it goes to a jackpot
            }

            throw new ArgumentException("GetAdjustedAmount: Uknown action " + type + " to have amount " + amount);
        }

        public bool IsRaise
        {
            get
            {
                return HandActionType == HandActionType.RAISE ||
                       IsAllInAction;
            }
        }

        public bool IsPreFlopRaise
        {
            get
            {
                return Street == Street.Preflop &&
                       (HandActionType == HandActionType.RAISE || IsAllInAction);
            }
        }

        public bool IsAllInAction
        {
            get { return HandActionType == HandActionType.ALL_IN; }
        }

        public bool IsWinningsAction
        {
            get
            {
                return HandActionType == HandActionType.WINS ||
                       HandActionType == HandActionType.WINS_SIDE_POT ||
                       HandActionType == HandActionType.TIES || 
                       HandActionType == HandActionType.TIES_SIDE_POT ||
                       HandActionType == HandActionType.WINS_THE_LOW;
            }
        }

        public bool IsAggressiveAction
        {
            get
            {
                return HandActionType == HandActionType.RAISE ||                       
                       HandActionType == HandActionType.BET ||
                       IsAllInAction;
            }
        }

        public bool IsBlinds
        {
            get
            {
                return HandActionType == HandActionType.SMALL_BLIND ||
                       HandActionType == HandActionType.BIG_BLIND ||
                       HandActionType == HandActionType.POSTS;
            }
        }

        public bool IsGameAction
        {
            get
            {
                return HandActionType == Actions.HandActionType.SMALL_BLIND ||
                    HandActionType == Actions.HandActionType.BIG_BLIND ||
                    HandActionType == Actions.HandActionType.ANTE ||
                    HandActionType == Actions.HandActionType.POSTS ||
                    HandActionType == Actions.HandActionType.BET ||
                    HandActionType == Actions.HandActionType.CHECK ||
                    HandActionType == Actions.HandActionType.FOLD ||
                    HandActionType == Actions.HandActionType.ALL_IN ||
                    HandActionType == Actions.HandActionType.CALL ||
                    HandActionType == Actions.HandActionType.RAISE;
            }
        }
    }
}
