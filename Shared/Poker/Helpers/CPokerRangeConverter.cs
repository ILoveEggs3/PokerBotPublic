using HoldemHand;
using System;
using System.Collections.Generic;
using System.Linq;
using static Shared.Poker.Models.CCard;

namespace Shared.Poker.Helpers
{
    /// <summary>
    /// Convert a range into all combinaisons possible
    /// </summary>
    public class CPokerRangeConverter
    {
        private enum CardType { Heart = 'h', Diamond = 'd', Spade = 's', Clubs = 'c' };

        private CPokerRangeConverter()
        {

        }
        
        public static CPokerRangeConverter GetInstance()
        {
            return new CPokerRangeConverter();
        }

        /// <summary>
        /// Convert a range given (range must be in this format: "22+ AK+ JTs") into all combinaison possible.
        /// </summary>
        /// <param name="_range">The range separated by white space.</param>
        /// <returns>Returns all the combinations without duplicate.</returns>
        public string[] ConvertRange(string _range)
        {
            string[] handsRange = _range.Split(' ');
            string combos = "";

            for (int currentHandRange = 0; currentHandRange < handsRange.Length; ++currentHandRange)
            {
                // If the last character of the current handrange (example: JTo+) has a +
                if (handsRange[currentHandRange].Last() == '+')
                {
                    bool IsConnected(string _hand)
                    {
                        return (_hand == "23" || _hand == "34" || _hand == "45" || _hand == "56" || _hand == "67" || _hand == "78" || _hand == "89" || _hand == "9T" || _hand == "JT" || _hand == "JQ" || _hand == "QK" || _hand == "KA" ||
                                _hand == "32" || _hand == "43" || _hand == "54" || _hand == "65" || _hand == "76" || _hand == "87" || _hand == "98" || _hand == "T9" || _hand == "TJ" || _hand == "QJ" || _hand == "KQ" || _hand == "AK");
                    }
                    bool LFIsHigher(Value _valueCard1, Value _valueCard2)
                    {
                        switch (_valueCard1)
                        {
                            case Value.Ace:
                                return (_valueCard1 != _valueCard2);
                            case Value.Two:
                                return false;
                            case Value.Three:
                                return (_valueCard2 == Value.Two);
                            case Value.Four:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three);
                            case Value.Five:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four);
                            case Value.Six:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five);
                            case Value.Seven:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six);
                            case Value.Eight:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven);
                            case Value.Nine:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven || _valueCard2 == Value.Eight);
                            case Value.Ten:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven || _valueCard2 == Value.Eight || _valueCard2 == Value.Nine);
                            case Value.Jack:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven || _valueCard2 == Value.Eight || _valueCard2 == Value.Nine || _valueCard2 == Value.Ten);
                            case Value.Queen:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven || _valueCard2 == Value.Eight || _valueCard2 == Value.Nine || _valueCard2 == Value.Ten || _valueCard2 == Value.Jack);
                            case Value.King:
                                return (_valueCard2 == Value.Two || _valueCard2 == Value.Three || _valueCard2 == Value.Four || _valueCard2 == Value.Five || _valueCard2 == Value.Six || _valueCard2 == Value.Seven || _valueCard2 == Value.Eight || _valueCard2 == Value.Nine || _valueCard2 == Value.Ten || _valueCard2 == Value.Jack || _valueCard2 == Value.Queen);
                            default:
                                throw new Exception("Unable to detect the value of the card 1!");
                        }
                    }

                    string currentHand = handsRange[currentHandRange].Substring(0, handsRange[currentHandRange].Length - 1);
                    string firstHandInTheRange = currentHand;

                    if (IsPocketPair(string.Concat(firstHandInTheRange[0], firstHandInTheRange[1])))
                    {
                        while (currentHand != null)
                        {
                            if (string.IsNullOrEmpty(combos))
                                combos = GetAllCombinations(currentHand);
                            else
                                combos += " " + GetAllCombinations(currentHand);

                            switch (currentHand[1])
                            {
                                case 'A':
                                    currentHand = null;
                                    break;
                                case '2':
                                    currentHand = "33";
                                    break;
                                case '3':
                                    currentHand = "44";
                                    break;
                                case '4':
                                    currentHand = "55";
                                    break;
                                case '5':
                                    currentHand = "66";
                                    break;
                                case '6':
                                    currentHand = "77";
                                    break;
                                case '7':
                                    currentHand = "88";
                                    break;
                                case '8':
                                    currentHand = "99";
                                    break;
                                case '9':
                                    currentHand = "TT";
                                    break;
                                case 'T':
                                    currentHand = "JJ";
                                    break;
                                case 'J':
                                    currentHand = "QQ";
                                    break;
                                case 'Q':
                                    currentHand = "KK";
                                    break;
                                case 'K':
                                    currentHand = "AA";
                                    break;
                                default:
                                    throw new Exception("Cannot read the hand!");
                            }
                        }
                    }
                    else if (IsConnected(string.Concat(firstHandInTheRange[0], firstHandInTheRange[1])))
                    {                        
                        // If it's 67s type of hand, we want it to be 76s
                        if (!LFIsHigher((Value)currentHand[0], (Value)currentHand[1]))
                            currentHand = string.Concat(currentHand[1], currentHand[0], currentHand.Last());

                        while(currentHand != null)
                        {
                            if (string.IsNullOrEmpty(combos))
                                combos = GetAllCombinations(currentHand);
                            else
                                combos += " " + GetAllCombinations(currentHand);

                            switch (currentHand[0])
                            {
                                case 'A':
                                    currentHand = null;
                                    break;
                                case '2':
                                    throw new InvalidOperationException("We cannot have a hand like 23s! The first card must be higher than the second card!");                                    
                                case '3':
                                    currentHand = "43" + currentHand.Last();
                                    break;
                                case '4':
                                    currentHand = "54" + currentHand.Last();
                                    break;
                                case '5':
                                    currentHand = "65" + currentHand.Last();
                                    break;
                                case '6':
                                    currentHand = "76" + currentHand.Last();
                                    break;
                                case '7':
                                    currentHand = "87" + currentHand.Last();
                                    break;
                                case '8':
                                    currentHand = "98" + currentHand.Last();
                                    break;
                                case '9':
                                    currentHand = "T9" + currentHand.Last();
                                    break;
                                case 'T':
                                    currentHand = "JT" + currentHand.Last();
                                    break;
                                case 'J':
                                    currentHand = "QJ" + currentHand.Last();
                                    break;
                                case 'Q':
                                    currentHand = "KQ" + currentHand.Last();
                                    break;
                                case 'K':
                                    currentHand = "AK" + currentHand.Last();
                                    break;
                                default:
                                    throw new Exception("Cannot read the hand!");
                            }
                        }
                    }
                    else
                    {
                        // If it's 68s type of hand, we want it to be 86s
                        if (!LFIsHigher((Value)currentHand[0], (Value)currentHand[1]))
                            currentHand = string.Concat(currentHand[1], currentHand[0], currentHand.Last());

                        while (!IsConnected(string.Concat(currentHand[0], currentHand[1])))
                        {
                            if (string.IsNullOrEmpty(combos))
                                combos = GetAllCombinations(currentHand);
                            else
                                combos += " " + GetAllCombinations(currentHand);

                            switch (currentHand[1])
                            {
                                case 'A':
                                    throw new InvalidOperationException("We should never have an ace as a kicker!");
                                case '2':
                                    currentHand = currentHand[0] + "3" + currentHand.Last();
                                    break;
                                case '3':
                                    currentHand = currentHand[0] + "4" + currentHand.Last();
                                    break;
                                case '4':
                                    currentHand = currentHand[0] + "5" + currentHand.Last();
                                    break;
                                case '5':
                                    currentHand = currentHand[0] + "6" + currentHand.Last();
                                    break;
                                case '6':
                                    currentHand = currentHand[0] + "7" + currentHand.Last();
                                    break;
                                case '7':
                                    currentHand = currentHand[0] + "8" + currentHand.Last();
                                    break;
                                case '8':
                                    currentHand = currentHand[0] + "9" + currentHand.Last();
                                    break;
                                case '9':
                                    currentHand = currentHand[0] + "T" + currentHand.Last();
                                    break;
                                case 'T':
                                    currentHand = currentHand[0] + "J" + currentHand.Last();
                                    break;
                                case 'J':
                                    currentHand = currentHand[0] + "Q" + currentHand.Last();
                                    break;
                                case 'Q':
                                    currentHand = currentHand[0] + "K" + currentHand.Last();
                                    break;
                                case 'K':
                                    throw new InvalidOperationException("We should never have an king as a kicker! (IsConnected should return true before entering here)");
                                default:
                                    throw new Exception("Cannot read the hand!");
                            }                            
                        }

                        // For the last hand
                        if (string.IsNullOrEmpty(combos))
                            combos = GetAllCombinations(currentHand);
                        else
                            combos += " " + GetAllCombinations(currentHand);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(combos))
                        combos = GetAllCombinations(handsRange[currentHandRange]);
                    else
                        combos += " " + GetAllCombinations(handsRange[currentHandRange]);
                }
            }

            return combos.Split(' ').Distinct().OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// Could be used for the hand evaluator.
        /// </summary>
        /// <param name="_range"></param>
        /// <returns></returns>
        public string ExpandHand(string _range)
        {
            string cards = "23456789tjqka";
            string fullHandsComboString = "";

            _range = _range.ToLower();
            string[] hands = _range.Split(' ');
            foreach (var hand in hands)
            {
                if (!hand.Contains("+"))
                {
                    fullHandsComboString += hand + ",";
                    continue;
                }

                //IE: AKo+
                int indexOfPlus = hand.IndexOf("+");
                if (hand[indexOfPlus - 1] == 's' || hand[indexOfPlus - 1] == 'o')
                {
                    int kickerIndex = indexOfPlus - 2;
                    int highCardindex = indexOfPlus - 3;

                    char kicker = hand[kickerIndex];
                    char highCard = hand[highCardindex];



                    for (int i = cards.IndexOf(kicker); i < cards.IndexOf(highCard); ++i)
                    {
                        fullHandsComboString += " " + highCard.ToString().ToUpper() + cards[i].ToString().ToUpper() + hand[indexOfPlus - 1] + ",";
                    }
                }
                else
                {
                    //pocket pair

                    char card = hand[indexOfPlus - 1];

                    for (int i = cards.IndexOf(card); i <= cards.IndexOf('a'); ++i)
                    {
                        fullHandsComboString += " " + cards[i].ToString().ToUpper() + cards[i].ToString().ToUpper() + ",";
                    }
                }
            }
            return fullHandsComboString.Substring(0, fullHandsComboString.Length - 1);
        }

        /// <summary>
        /// Returns true if our hand is in the received range.
        /// </summary>
        /// <param name="_range">Range must be in this format (22+ AJo+ AJs+ QJs).</param>
        /// <returns>Returns true if our hand is in the received range. Otherwise, it returns false.</returns>
        public bool isOurHandInThisRange(Hand _handToCompare, string _range)
        {
            string[] theTable = GetInstance().ConvertRange(_range);

            foreach (string theComboInTheTableFromTheRangeConverter in theTable)
            {
                if (_handToCompare.PocketMask == Hand.ParseHand(theComboInTheTableFromTheRangeConverter))
                    return true;                
            }

            return false;
        }

        private string GetAllCombinations(string _hand)
        {
            if (_hand.Length != 2 && _hand.Length != 3)
                throw new Exception("The string given is not a poker hand! Here is the hand received: " + _hand);

            string combinaisons = "";

            if (IsOffSuited(_hand))
                combinaisons = GetOffSuitedCombinaison(_hand);
            else if (IsPocketPair(_hand))
                combinaisons = GetPocketPairCombinaison(_hand);
            else if (IsSuited(_hand))
                combinaisons = GetSuitedCombinaison(_hand);
            else
                throw new Exception("Impossible to convert the range given!");

            return combinaisons;
        }

        private string SortCombination(string _combo)
        {
            if (_combo.Length != 4)
                throw new Exception("The combination given is not valid!");

            // If it is a pocket pair.
            if (_combo[0] == _combo[2])
            {
                switch (_combo[1])
                {
                    case 'h':
                        return _combo;
                    case 'd':
                        if (_combo[3] == 'h')
                            return RevertPokerCombination(_combo);
                        else
                            return _combo;
                    case 's':
                        if ((_combo[3] == 'd') || (_combo[3] == 'h'))
                            return RevertPokerCombination(_combo);
                        else
                            return _combo;
                    case 'c':
                        return RevertPokerCombination(_combo);
                    default:
                        throw new Exception("The combination given is not valid!");
                }
            }

            // Otherwise it's something else, either a suited or a offsuited card.
            switch (_combo[2])
            {
                case 'A':
                    return RevertPokerCombination(_combo);
                case 'K':
                    if (_combo[0] != 'A')
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case 'Q':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case 'J':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case 'T':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '9':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '8':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '7':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9') && (_combo[0] != '8'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '6':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9') && (_combo[0] != '8') && (_combo[0] != '7'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '5':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9') && (_combo[0] != '8') && (_combo[0] != '7') && (_combo[0] != '6'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '4':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9') && (_combo[0] != '8') && (_combo[0] != '7') && (_combo[0] != '6') && (_combo[0] != '5'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '3':
                    if ((_combo[0] != 'A') && (_combo[0] != 'K') && (_combo[0] != 'Q') && (_combo[0] != 'J') && (_combo[0] != 'T') && (_combo[0] != '9') && (_combo[0] != '8') && (_combo[0] != '7') && (_combo[0] != '6') && (_combo[0] != '5') && (_combo[0] != '4'))
                        return RevertPokerCombination(_combo);
                    else
                        return _combo;
                case '2':
                    return _combo;
                default:
                    throw new Exception("The combination given is not valid!");
            }
        }

        private string RevertPokerCombination(string _combo)
        {
            if (_combo.Length != 4)
                throw new Exception("The combination given is not valid!");

            return string.Concat(_combo[2], _combo[3], _combo[0], _combo[1]);
        }

        private bool IsPocketPair(string _hand)
        {
            return _hand[0] == _hand[1];
        }

        private bool IsSuited(string _hand)
        {
            if (_hand.Length == 3)
                return _hand[2] == 's';
            else
                return false;
        }

        private bool IsOffSuited(string _hand)
        {
            if (_hand.Length == 3)
                return _hand[2] == 'o';
            else
                return false;
        }

        private string GetPocketPairCombinaison(string _hand)
        {
            if (!IsPocketPair(_hand))
                throw new Exception("The hand is not a pocket pair!");

            string combo1 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Diamond);
            string combo2 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Spade);
            string combo3 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Clubs);
            string combo4 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Spade);
            string combo5 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Clubs);
            string combo6 = string.Concat(_hand[0], (char)CardType.Spade, _hand[1], (char)CardType.Clubs);

            List<string> combos = new List<string>();

            combos.Add(combo1);
            combos.Add(combo2);
            combos.Add(combo3);
            combos.Add(combo4);
            combos.Add(combo5);
            combos.Add(combo6);

            for (int currentCombinationIndex = 0; currentCombinationIndex < combos.Count; currentCombinationIndex++)
                combos[currentCombinationIndex] = SortCombination(combos[currentCombinationIndex]);

            return string.Join(" ", combos);
        }

        private string GetSuitedCombinaison(string _hand)
        {
            if (!IsSuited(_hand))
                throw new Exception("The hand is not suited!");

            string combo1 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Heart);
            string combo2 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Diamond);
            string combo3 = string.Concat(_hand[0], (char)CardType.Spade, _hand[1], (char)CardType.Spade);
            string combo4 = string.Concat(_hand[0], (char)CardType.Clubs, _hand[1], (char)CardType.Clubs);

            List<string> combos = new List<string>();

            combos.Add(combo1);
            combos.Add(combo2);
            combos.Add(combo3);
            combos.Add(combo4);

            for (int currentCombinationIndex = 0; currentCombinationIndex < combos.Count; currentCombinationIndex++)
                combos[currentCombinationIndex] = SortCombination(combos[currentCombinationIndex]);

            return string.Join(" ", combos);
        }

        private string GetOffSuitedCombinaison(string _hand)
        {
            if (!IsOffSuited(_hand))
                throw new Exception("The hand is not offsuited!");

            string combo1 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Diamond);
            string combo2 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Spade);
            string combo3 = string.Concat(_hand[0], (char)CardType.Heart, _hand[1], (char)CardType.Clubs);
            string combo4 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Heart);
            string combo5 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Spade);
            string combo6 = string.Concat(_hand[0], (char)CardType.Diamond, _hand[1], (char)CardType.Clubs);
            string combo7 = string.Concat(_hand[0], (char)CardType.Spade, _hand[1], (char)CardType.Heart);
            string combo8 = string.Concat(_hand[0], (char)CardType.Spade, _hand[1], (char)CardType.Diamond);
            string combo9 = string.Concat(_hand[0], (char)CardType.Spade, _hand[1], (char)CardType.Clubs);
            string combo10 = string.Concat(_hand[0], (char)CardType.Clubs, _hand[1], (char)CardType.Heart);
            string combo11 = string.Concat(_hand[0], (char)CardType.Clubs, _hand[1], (char)CardType.Diamond);
            string combo12 = string.Concat(_hand[0], (char)CardType.Clubs, _hand[1], (char)CardType.Spade);

            List<string> combos = new List<string>();

            combos.Add(combo1);
            combos.Add(combo2);
            combos.Add(combo3);
            combos.Add(combo4);
            combos.Add(combo5);
            combos.Add(combo6);
            combos.Add(combo7);
            combos.Add(combo8);
            combos.Add(combo9);
            combos.Add(combo10);
            combos.Add(combo11);
            combos.Add(combo12);

            for (int currentCombinationIndex = 0; currentCombinationIndex < combos.Count; currentCombinationIndex++)
                combos[currentCombinationIndex] = SortCombination(combos[currentCombinationIndex]);

            return string.Join(" ", combos);
        }
    }
}
