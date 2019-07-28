using System;
using System.Collections.Generic;
using System.Linq;
using static Shared.Poker.Models.CTableInfos;

namespace Shared.Poker.Models
{
    public class CAction : ICloneable
    {
        
        public enum BetSizePossible { Percent33 = 33, Percent50 = 50, Percent72 = 72, Percent100 = 100, Percent133 = 133, AllInShort = 135, AllIn = 136 };
        public enum CallSizePossibleVsReRaise { AnySizingExceptAllIn = 134, AllInShort = 135, AllIn = 136 }
        public enum RaiseSizePossibleTwoBetPot { TwoPoint7Max = 3, FourPoint5Max = 5, AnySizingExceptAllIn = 134, AllInShort = 135, AllIn = 136}
        public enum RaiseSizePossibleThreeBetPot { ThreePoint5Max = 4, AnySizingExceptAllIn = 134, AllInShort = 135, AllIn = 136}
        public enum RaiseSizePossibleFourBetPot { TwoPoint7Max = 3, AnySizingExceptAllIn = 134, AllIn = 135}
        [Flags]
        public enum PokerAction { None = 0, Fold = 1, Check = 2, Bet = 4, Call = 8, Raise = 16, CallVsRaise = 32, ReRaise = 64, CallVsReRaise = 128};
        [Flags] enum SimplePokerAction { None = 0, Fold = 1, Check = 2, Bet = 4, Call = (PokerAction.Call | PokerAction.CallVsRaise | PokerAction.CallVsReRaise), Raise = (PokerAction.Raise | PokerAction.ReRaise) }
        public enum PokerActionBD { None, Fold, Check, Bet, Call, Raise, CallVsRaise, ReRaise, CallVsReRaise };

        public static readonly Dictionary<BetSizePossible, double> PDicBetSize = new Dictionary<BetSizePossible, double>(5) { { BetSizePossible.Percent33, 0.33 },
                                                                                                                                { BetSizePossible.Percent50, 0.50 },
                                                                                                                                { BetSizePossible.Percent72, 0.72 },
                                                                                                                                { BetSizePossible.Percent100, 1 },
                                                                                                                                { BetSizePossible.Percent133, 1.33 } };

        public static readonly List<(BetSizePossible, double)> BetSizePossibleList = new List<(BetSizePossible, double)>()
        {
            (BetSizePossible.Percent33, 0.33d),
            (BetSizePossible.Percent50, 0.55d),
            (BetSizePossible.Percent72, 0.72d),
            (BetSizePossible.Percent100, 1.0d),
            (BetSizePossible.Percent133, 1.33d),
            (BetSizePossible.AllInShort, 2.7d),
            (BetSizePossible.AllIn, 4.5d)
        };
        public static readonly List<(CallSizePossibleVsReRaise, double)> CallSizePossibleVsReRaiseList = new List<(CallSizePossibleVsReRaise, double)>()
        {
            (CallSizePossibleVsReRaise.AnySizingExceptAllIn, 0.33d),
            (CallSizePossibleVsReRaise.AllInShort, 0.66d),
            (CallSizePossibleVsReRaise.AllIn, 1.0d)
        };
        public static readonly List<(RaiseSizePossibleTwoBetPot, double)> RaiseSizePossibleTwoBetPotList = new List<(RaiseSizePossibleTwoBetPot, double)>()
        {
            (RaiseSizePossibleTwoBetPot.TwoPoint7Max, 2.7d),
            (RaiseSizePossibleTwoBetPot.FourPoint5Max, 4.5d),
            (RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn, 6.6d),
            (RaiseSizePossibleTwoBetPot.AllInShort, 8.8d),
            (RaiseSizePossibleTwoBetPot.AllIn, 10.8d)
        };
        public static readonly List<(RaiseSizePossibleThreeBetPot, double)> RaiseSizePossibleThreeBetPotList = new List<(RaiseSizePossibleThreeBetPot, double)>()
        {
            (RaiseSizePossibleThreeBetPot.ThreePoint5Max, 3.5d),
            (RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn, 5.5d),
            (RaiseSizePossibleThreeBetPot.AllInShort, 7.5d),
            (RaiseSizePossibleThreeBetPot.AllIn, 9.5d)
        };
        public static readonly List<(RaiseSizePossibleFourBetPot, double)> RaiseSizePossibleFourBetPotList = new List<(RaiseSizePossibleFourBetPot, double)>()
        {
            (RaiseSizePossibleFourBetPot.TwoPoint7Max, 2.7d),
            (RaiseSizePossibleFourBetPot.AnySizingExceptAllIn, 4.0d),
            (RaiseSizePossibleFourBetPot.AllIn, 5.5d)
        };

        public static List<(long?, double)> GetListFromPokerAction(TypesPot _typePot, PokerAction pokerAction)
        {
           
            if ((pokerAction & (PokerAction.Call | PokerAction.Bet)) != 0)
                return BetSizePossibleList.Select(x => ((long?)x.Item1, x.Item2)).ToList();
            else if ((pokerAction & (PokerAction.Raise | PokerAction.CallVsRaise)) != 0)
            {
                switch (_typePot)
                {
                    case TypesPot.TwoBet:
                        return RaiseSizePossibleTwoBetPotList.Select(x => ((long?)x.Item1, x.Item2)).ToList();
                    case TypesPot.ThreeBet:
                        return RaiseSizePossibleThreeBetPotList.Select(x => ((long?)x.Item1, x.Item2)).ToList();
                    case TypesPot.FourBet:
                    case TypesPot.FiveBetEtPlus:
                        return RaiseSizePossibleFourBetPotList.Select(x => ((long?)x.Item1, x.Item2)).ToList();
                    default:
                        break;
                }
            } if ((pokerAction & (PokerAction.CallVsReRaise)) != 0)
                return CallSizePossibleVsReRaiseList.Select(x => ((long?)x.Item1, x.Item2)).ToList();
            return new List<(long?, double)>() { (null, 0) };
        }

        public static double GetRaiseAmount(RaiseSizePossibleTwoBetPot _raise)
        {
            switch (_raise)
            {
                case RaiseSizePossibleTwoBetPot.TwoPoint7Max:
                    return 2.7d;
                case RaiseSizePossibleTwoBetPot.FourPoint5Max:
                    return 4.5d;
                case RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn:
                    return 7.7d;
                case RaiseSizePossibleTwoBetPot.AllInShort:
                    return 10.3d;
                case RaiseSizePossibleTwoBetPot.AllIn:
                    return 14.3d;
                default:
                    throw new Exception("Unsupported value");
            }
        }

        public static double GetRaiseAmount(RaiseSizePossibleThreeBetPot _raise)
        {
            switch (_raise)
            {
                case RaiseSizePossibleThreeBetPot.ThreePoint5Max:
                    return 3.5d;
                case RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn:
                    return 5.7d;
                case RaiseSizePossibleThreeBetPot.AllInShort:
                    return 6.6d;
                case RaiseSizePossibleThreeBetPot.AllIn:
                    return 7.6d;
                default:
                    throw new Exception("Unsupported value");
            }
        }

        public static double GetRaiseAmount(RaiseSizePossibleFourBetPot _raise)
        {
            switch (_raise)
            {
                case RaiseSizePossibleFourBetPot.TwoPoint7Max:
                    return 2.7d;
                case RaiseSizePossibleFourBetPot.AnySizingExceptAllIn:
                    return 4.0d;
                case RaiseSizePossibleFourBetPot.AllIn:
                    return 4.8d;
                default:
                    throw new Exception("Unsupported value");
            }
        }

        public double PMise { get; }

        /// <summary>
        /// Action possible qu'un joueur peut effectuer dans une partie de poker.
        /// </summary>
        public PokerAction PAction { get; }
        
        /// <summary>
        /// À utiliser si on a une action qui ne nécessite pas de mise.
        /// </summary>
        /// <param name="_action">Action que le joueur a effectué.</param>
        public CAction(PokerAction _action)
        {
            if (!Enum.IsDefined(typeof(PokerAction), _action))
                throw new ArgumentException();
            else if (_action == PokerAction.Bet || _action == PokerAction.Raise || _action == PokerAction.Call)
                throw new ArgumentException("Action qui nécessite une mise. Veuillez appeler un autre constructeur de la classe CAction.");

            PAction = _action;
            PMise = 0;
        }

        /// <summary>
        /// À utiliser si on a une action qui nécessite une mise.
        /// </summary>
        /// <param name="_action">Action que le joueur a effectué.</param>
        /// <param name="_mise">Mise que le joueur a effectué.</param>
        public CAction(PokerAction _action, double _mise)
        {
            if (!Enum.IsDefined(typeof(PokerAction), _action))
                throw new ArgumentException();
            else if (_action == PokerAction.Check || _action == PokerAction.Fold || _action == PokerAction.None || _action == PokerAction.ReRaise)
                throw new ArgumentException("Action qui nécessite une mise. Veuillez appeler un autre constructeur de la classe CAction.");
            else if (_mise <= 0)
                throw new ArgumentOutOfRangeException("La mise doit être plus grande que 0.");

            PAction = _action;
            PMise = Math.Round(_mise, 2);
        }

        public CAction()
        {
            PAction = PokerAction.None;
            PMise = 0;
        }

        private CAction(CAction _action)
        {
            PAction = _action.PAction;
            PMise = _action.PMise;
        }

        public object Clone()
        {
            return new CAction(this);
        }
    }
}
