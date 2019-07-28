using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HoldemHand;
using PregeneratedDataProject.Helpers;
using Shared.Models.Database;

namespace PregeneratedDataProject
{
    //https://fr.wikipedia.org/wiki/Loi_normale
    class Program
    {
        #region PrecalculatedArray
        static readonly double[] P = {
            0.50000,0.50399,0.50798,0.51197,0.51595,0.51994,0.52392,0.52790,0.53188,0.53586,
            0.53983,0.54380,0.54776,0.55172,0.55567,0.55962,0.56356,0.56749,0.57142,0.57535,
            0.57926,0.58317,0.58706,0.59095,0.59483,0.59871,0.60257,0.60642,0.61026,0.61409,
            0.61791,0.62172,0.62552,0.62930,0.63307,0.63683,0.64058,0.64431,0.64803,0.65173,
            0.65542,0.65910,0.66276,0.66640,0.67003,0.67364,0.67724,0.68082,0.68439,0.68793,
            0.69146,0.69497,0.69847,0.70194,0.70540,0.70884,0.71226,0.71566,0.71904,0.72240,
            0.72575,0.72907,0.73237,0.73565,0.73891,0.74215,0.74537,0.74857,0.75175,0.75490,
            0.75804,0.76115,0.76424,0.76730,0.77035,0.77337,0.77637,0.77935,0.78230,0.78524,
            0.78814,0.79103,0.79389,0.79673,0.79955,0.80234,0.80511,0.80785,0.81057,0.81327,
            0.81594,0.81859,0.82121,0.82381,0.82639,0.82894,0.83147,0.83398,0.83646,0.83891,
            0.84134,0.84375,0.84614,0.84849,0.85083,0.85314,0.85543,0.85769,0.85993,0.86214,
            0.86433,0.86650,0.86864,0.87076,0.87286,0.87493,0.87698,0.87900,0.88100,0.88298,
            0.88493,0.88686,0.88877,0.89065,0.89251,0.89435,0.89617,0.89796,0.89973,0.90147,
            0.90320,0.90490,0.90658,0.90824,0.90988,0.91149,0.91309,0.91466,0.91621,0.91774,
            0.91924,0.92073,0.92220,0.92364,0.92507,0.92647,0.92785,0.92922,0.93056,0.93189,
            0.93319,0.93448,0.93574,0.93699,0.93822,0.93943,0.94062,0.94179,0.94295,0.94408,
            0.94520,0.94630,0.94738,0.94845,0.94950,0.95053,0.95154,0.95254,0.95352,0.95449,
            0.95543,0.95637,0.95728,0.95818,0.95907,0.95994,0.96080,0.96164,0.96246,0.96327,
            0.96407,0.96485,0.96562,0.96638,0.96712,0.96784,0.96856,0.96926,0.96995,0.97062,
            0.97128,0.97193,0.97257,0.97320,0.97381,0.97441,0.97500,0.97558,0.97615,0.97670,
            0.97725,0.97778,0.97831,0.97882,0.97932,0.97982,0.98030,0.98077,0.98124,0.98169,
            0.98214,0.98257,0.98300,0.98341,0.98382,0.98422,0.98461,0.98500,0.98537,0.98574,
            0.98610,0.98645,0.98679,0.98713,0.98745,0.98778,0.98809,0.98840,0.98870,0.98899,
            0.98928,0.98956,0.98983,0.99010,0.99036,0.99061,0.99086,0.99111,0.99134,0.99158,
            0.99180,0.99202,0.99224,0.99245,0.99266,0.99286,0.99305,0.99324,0.99343,0.99361,
            0.99379,0.99396,0.99413,0.99430,0.99446,0.99461,0.99477,0.99492,0.99506,0.99520,
            0.99534,0.99547,0.99560,0.99573,0.99585,0.99598,0.99609,0.99621,0.99632,0.99643,
            0.99653,0.99664,0.99674,0.99683,0.99693,0.99702,0.99711,0.99720,0.99728,0.99736,
            0.99744,0.99752,0.99760,0.99767,0.99774,0.99781,0.99788,0.99795,0.99801,0.99807,
            0.99813,0.99819,0.99825,0.99831,0.99836,0.99841,0.99846,0.99851,0.99856,0.99861,
            0.99865,0.99869,0.99874,0.99878,0.99882,0.99886,0.99889,0.99893,0.99896,0.99900,
            0.99903,0.99906,0.99910,0.99913,0.99916,0.99918,0.99921,0.99924,0.99926,0.99929,
            0.99931,0.99934,0.99936,0.99938,0.99940,0.99942,0.99944,0.99946,0.99948,0.99950,
            0.99952,0.99953,0.99955,0.99957,0.99958,0.99960,0.99961,0.99962,0.99964,0.99965,
            0.99966,0.99968,0.99969,0.99970,0.99971,0.99972,0.99973,0.99974,0.99975,0.99976,
            0.99977,0.99978,0.99978,0.99979,0.99980,0.99981,0.99981,0.99982,0.99983,0.99983,
            0.99984,0.99985,0.99985,0.99986,0.99986,0.99987,0.99987,0.99988,0.99988,0.99989,
            0.99989,0.99990,0.99990,0.99990,0.99991,0.99992,0.99992,0.99992,0.99992,0.99992,
            0.99993,0.99993,0.99993,0.99994,0.99994,0.99994,0.99994,0.99995,0.99995,0.99995,
            0.99995,0.99995,0.99996,0.99996,0.99996,0.99996,0.99996,0.99996,0.99997,0.99997
        };
        #endregion


        static double BoardHeat(ulong boardValue, double mean, double standartDev)
        {
            double averageBoardValue = mean;//V1:15755210; V2:26698860
            double ecartType = standartDev;//V1: 6589796.72489 V2: 12295328.2387;

            double boardValueCenteredNormalized = (boardValue - averageBoardValue) / ecartType;

            boardValueCenteredNormalized += 4;
            var ind = (int)(boardValueCenteredNormalized * 100);
            if (ind < 0) ind = 0;
            else if (ind > 799) ind = 799;

            if (ind >= 400) return P[ind - 400];
            else return 1.0 - P[399 - ind];

        }

        static double BoardHeat(ulong boardValue)
        {
            const double averageBoardValue = 15755210;//V1:15755210; V2:26698860
            const double ecartType = 6589796.72489;//V1: 6589796.72489 V2: 12295328.2387;

            double boardValueCenteredNormalized = (boardValue - averageBoardValue) / ecartType;

            boardValueCenteredNormalized += 4;
            var ind = (int)(boardValueCenteredNormalized * 100);
            if (ind < 0) ind = 0;
            else if (ind > 799) ind = 799;

            if (ind >= 400) return P[ind - 400];
            else return 1.0 - P[399 - ind];

        }


        static List<ulong> GenerateAllPockets()
        {
            var pocketList = new List<ulong>();
            foreach (var item in Hand.Hands(0, 0, 2))
            {
                pocketList.Add(item);
            }
            return pocketList;
        }

        static void InsertAllPocketIntoDB()
        {
            CPocketModel pocket = new CPocketModel(3);
            var pocketList = GenerateAllPockets();
            int ind = 0;
            foreach (var item in pocketList)
            {
                CDBHelper.InsertPocket(new CPocketModel(item));
                Console.WriteLine("Inserting Pockets into DB: {0}/1326", ind++);
            }
        }

        static List<ulong> GenerateAllBoardsOfNCards(int _cardCount)
        {
            if (_cardCount < 3 || _cardCount > 5)
                throw new Exception("Bad card number");
            var ret = new List<ulong>();

            foreach (var item in Hand.Hands(_cardCount))
            {
                ret.Add(item);
            }
            return ret;
        }
        /*
        static void GenerateAllHands()
        {
            var minHeat = double.MaxValue;
            var maxHeat = -273.0d;
            var min = ulong.MaxValue;
            var max = ulong.MinValue;
            var minType = (ulong)ulong.MaxValue;
            var maxType = (ulong)0;
            var ind = 0;
            var indFlop = 0;

            var minFlopValue = ulong.MaxValue;
            var maxFlopValue = ulong.MinValue;
            var averageFlopValue = (ulong)0;

            var minTurnValue = ulong.MaxValue;
            var maxTurnValue = ulong.MinValue;
            var averageTurnValue = (ulong)0;

            var minRiverValue = ulong.MaxValue;
            var maxRiverValue = ulong.MinValue;
            var averageRiverValue = (ulong)0;

            foreach (var flop in Hand.Hands(0, 0, 3))
            {
                var flopValue = (ulong)0;
                var nbTurn = 0;
                foreach (var turn in Hand.Hands(0, flop, 1))
                {
                    var turnValue = (ulong)0;
                    var nbRiver = 0;
                    foreach (var river in Hand.Hands(0, flop | turn, 1))
                    {
                        var riverValue = (ulong)0;
                        var nbHands = 0;
                        foreach (var pocket in Hand.Hands(0, flop | turn | river, 2))
                        {
                            nbHands += 1;
                            var tempHand = new Hand();
                            var boardHand = new Hand();

                            boardHand.BoardMask = flop;
                            boardHand.PocketMask = turn | river;
                            var boardValue = boardHand.HandValue;
                            var boardValueType = (uint)boardHand.HandTypeValue;

                            tempHand.BoardMask = flop | turn | river;
                            tempHand.PocketMask = pocket;
                            var handValue = tempHand.HandValue;
                            var handValueType = (uint)tempHand.HandTypeValue;

                            var value = (uint)handValue - boardValue;
                            var valueType = (uint)handValueType - boardValueType;

                            riverValue += value;
                        }
                        riverValue /= (ulong)nbHands;
                        turnValue += riverValue;
                        minRiverValue = Math.Min(minRiverValue, riverValue);
                        maxRiverValue = Math.Max(maxRiverValue, riverValue);
                        averageRiverValue += (ulong)riverValue;
                        nbRiver++;
                    }
                    turnValue /= (ulong)nbRiver;
                    flopValue += turnValue;
                    minTurnValue = Math.Min(minTurnValue, turnValue);
                    maxTurnValue = Math.Max(maxTurnValue, turnValue);
                    averageTurnValue += (ulong)turnValue;
                    nbTurn += nbRiver;
                    Console.WriteLine("{0}%", (1.0 * ind++ / ((1.0 * 52 * 51 * 50 * 49) / 24)) * 100);
                }
                flopValue /= (ulong)nbTurn;
                minFlopValue = Math.Min(minFlopValue, flopValue);
                maxFlopValue = Math.Max(maxFlopValue, flopValue);
                averageFlopValue += (ulong)flopValue;
                Console.WriteLine("***** {0} / {1} ******", ++indFlop, (52 * 51 * 50) / 6);
            }

            using (var writer = new StreamWriter(@"C:\Users\admin\Desktop\Values.txt"))
            {
                writer.WriteLine("*** FLOP STATS ***");
                writer.WriteLine("minFlopValue: {0}", minFlopValue);
                writer.WriteLine("maxFlopValue: {0}", maxFlopValue);
                writer.WriteLine("averageFlopValue: {0}", averageFlopValue);
                writer.WriteLine("*** FLOP STATS ***");

                writer.WriteLine("*** TURN STATS ***");
                writer.WriteLine("minTurnValue: {0}", minTurnValue);
                writer.WriteLine("maxTurnValue: {0}", maxTurnValue);
                writer.WriteLine("averageTurnValue: {0}", averageTurnValue);
                writer.WriteLine("*** TURN STATS ***");

                writer.WriteLine("*** RIVER STATS ***");
                writer.WriteLine("minRiverValue: {0}", minRiverValue);
                writer.WriteLine("maxRiverValue: {0}", maxRiverValue);
                writer.WriteLine("averageRiverValue: {0}", averageRiverValue);
                writer.WriteLine("*** RIVER STATS ***");
            }
        }
        */
        static readonly object key = new object();
        static void GenerateAllHandsV2()
        {
            long ind;

            #region River
            const double RIVER_AVERAGE = 15755210;//V1:15755210; V2:26698860
            const double RIVER_STDV = 6589796.72489;//V1: 6589796.72489 V2: 12295328.2387;
            const long NB_RIVER = (52 * 51 * 50 * 49 * 48 / (5 * 4 * 3 * 2 * 1));

            long riverAverage = 0;
            decimal riverVariance = 0;
            var riverList = new List<Tuple<CBoardModel, long>>();
            ind = 0;

            Parallel.ForEach(Hand.Hands(0, 0, 5), (Action<ulong>)(board =>
            {
                long averageBoard = 0;
                var boardString = Hand.Cards(board).ToList();
                var boardValue = new Hand(boardString[0] + boardString[1], boardString[2] + boardString[3] + boardString[4]).HandValue;
                /*foreach (var pocket in Hand.Hands(0, board, 2))
                {
                    var pocketString = Hand.Cards(pocket);
                    var pocketValue = new Hand(string.Join("", pocketString), string.Join("", boardString)).HandValue; //boardString.Aggregate((x, y) => x + y)).HandValue;
                    var value = pocketValue - boardValue;
                    averageBoard += value;
                }
                averageBoard /= (47 * 46 / 2);
                var boardHeat = BoardHeat((ulong)averageBoard, RIVER_AVERAGE, RIVER_STDV);*/

                Interlocked.Add(ref riverAverage, (long)averageBoard);
                lock (key)
                {
                    riverVariance += (decimal)Math.Pow((averageBoard - RIVER_AVERAGE), 2);
                    //CBoardModel bm = new CBoardModel(board, boardHeat);
                    CBoardModel bm = new CBoardModel(board, 0);
                    riverList.Add(new Tuple<CBoardModel, long>(bm, averageBoard));
                }

                Interlocked.Increment(ref ind);
                if (ind % (NB_RIVER / 100) == 0)
                {
                    Console.WriteLine("river: {0}%", 1.0d * ind / NB_RIVER * 100);
                }
            }));
            riverAverage /= NB_RIVER;
            riverVariance /= NB_RIVER;
            #endregion

            #region Turn
            const double TURN_AVERAGE = 15755210;//V1:15755210; V2:26698860
            const double TURN_STDV = 6589796.72489;//V1: 6589796.72489 V2: 12295328.2387;
            const long NB_TURN = (52 * 51 * 50 * 49 / (4 * 3 * 2 * 1));

            long turnAverage = 0;
            decimal turnVariance = 0;
            var turnList = new List<Tuple<CBoardModel, long>>();
            ind = 0;


            Parallel.ForEach(Hand.Hands(0, 0, 4), turn =>
            {
                //var boardList = riverList.Where(x => (x.Item1.PBoardMask & turn) == turn).ToList();
                var tempAverage = (long)0;

                /*foreach (var item in boardList)
                {
                    tempAverage += item.Item2;
                }
                tempAverage /= boardList.Count;*/

                //Interlocked.Add(ref turnAverage, tempAverage);
                //var boardHeat = BoardHeat((ulong)tempAverage, TURN_AVERAGE, TURN_STDV);
                lock (key)
                {
                    //turnVariance += (decimal)Math.Pow(TURN_AVERAGE - tempAverage, 2);
                    //CBoardModel bm = new CBoardModel(turn, boardHeat);
                    CBoardModel bm = new CBoardModel(turn, 0);
                    turnList.Add(new Tuple<CBoardModel, long>(bm, tempAverage));
                }

                Interlocked.Increment(ref ind);
                if (ind % (NB_TURN / 100) == 0)
                {
                    Console.WriteLine("turn: {0}%", 1.0d * ind / NB_TURN * 100);
                }
            });
            turnAverage /= NB_TURN;
            turnVariance /= NB_TURN;
            #endregion

            #region FLop
            const double FLOP_AVERAGE = 15755210;//V1:15755210; V2:26698860
            const double FLOP_STDV = 6589796.72489;//V1: 6589796.72489 V2: 12295328.2387;
            const long NB_FLOP = (52 * 51 * 50 / (3 * 2 * 1));

            long flopAverage = 0;
            decimal flopVariance = 0;
            var flopList = new List<Tuple<CBoardModel, long>>();
            ind = 0;


            Parallel.ForEach(Hand.Hands(0, 0, 3), flop =>
            {
                //var boardList = turnList.Where(x => (x.Item1.PBoardMask & flop) == flop).ToList();
                var tempAverage = (long)0;

                /*foreach (var item in boardList)
                {
                    tempAverage += item.Item2;
                }
                tempAverage /= boardList.Count;*/

                //Interlocked.Add(ref flopAverage, tempAverage);
                //var boardHeat = BoardHeat((ulong)tempAverage, FLOP_AVERAGE, FLOP_STDV);
                lock (key)
                {
                    //flopVariance += (decimal)Math.Pow(FLOP_AVERAGE - tempAverage, 2);
                    //CBoardModel bm = new CBoardModel(flop, boardHeat);
                    CBoardModel bm = new CBoardModel(flop, 0);
                    flopList.Add(new Tuple<CBoardModel, long>(bm, tempAverage));
                }

                Interlocked.Increment(ref ind);
                if (ind % (NB_FLOP / 100) == 0)
                {
                    Console.WriteLine("flop: {0}%", 1.0d * ind / NB_FLOP * 100);
                }
            });
            flopAverage /= NB_FLOP;
            flopVariance /= NB_FLOP;
            #endregion

            #region WriteLines

            /*
            Console.WriteLine("*** River ***");
            Console.WriteLine("riverAverage: {0}", riverAverage);
            Console.WriteLine("riverVariance: {0}", riverVariance);
            Console.WriteLine("*** River ***");
            Console.WriteLine("");

            Console.WriteLine("*** Turn ***");
            Console.WriteLine("turnAverage: {0}", turnAverage);
            Console.WriteLine("turnVariance: {0}", turnAverage);
            Console.WriteLine("*** Turn ***");
            Console.WriteLine("");

            Console.WriteLine("*** Flop ***");
            Console.WriteLine("flopAverage: {0}", flopAverage);
            Console.WriteLine("flopVariance: {0}", flopAverage);
            Console.WriteLine("*** Flop ***");
            Console.WriteLine("");
            */

            #endregion

            #region Inserts

            var tempList = new List<CHandModel>();
            /*CDBHelper.InsertBoards(flopList.ConvertAll(x => x.Item1));
            ind = 0;
            foreach (var item in flopList)
            {
               Parallel.ForEach(Hand.Hands(0, item.Item1.PBoardMask, 2), pocketMask =>
               {
                   var temp = new CHandModel(pocketMask, item.Item1.PBoardMask);
                   lock (tempList)
                   {
                       tempList.Add(temp);
                   }
               });
                if (tempList.Count > 100000)
                {
                    Console.WriteLine("Inserting flopBoards into DB: {0}%", 1.0d * ind / flopList.Count * 100);
                    CDBHelper.InsertHands(tempList);
                    tempList.Clear();
                    GC.Collect();
                }
                ind++;
            }
            CDBHelper.InsertHands(tempList);
            tempList.Clear();
            GC.Collect();
            */
            //CDBHelper.InsertBoards(turnList.ConvertAll(x => x.Item1));
            ind = 0;
            /*foreach (var item in turnList)
            {
                ind++;
                if (ind % (turnList.Count / 10000) == 0)
                {
                    Console.WriteLine("Inserting turnBoards into DB: {0}%", 1.0d * ind / turnList.Count * 100);
                }
                if (CDBHelper.IsBoardHandInsideHandsTable(item.Item1.PBoardMask))
                {
                    continue;
                }
                Parallel.ForEach(Hand.Hands(0, item.Item1.PBoardMask, 2), pocketMask =>
                {
                    var temp = new CHandModel(pocketMask, item.Item1.PBoardMask);
                    lock (tempList)
                    {
                        tempList.Add(temp);
                    }
                });
                if (tempList.Count > 100000)
                {
                    //Console.WriteLine("Inserting turnBoards into DB: {0}%", 1.0d * ind / turnList.Count * 100);
                    CDBHelper.InsertHands(tempList);
                    tempList.Clear();
                    GC.Collect();
                }

            }
            CDBHelper.InsertHands(tempList);
            tempList.Clear();
            GC.Collect();*/

            //CDBHelper.InsertBoards(riverList.ConvertAll(x => x.Item1));
            ind = 0;
            foreach (var item in riverList)
            {
                ind++;
                if (ind % (riverList.Count / 10000) == 0)
                {
                    Console.WriteLine("Inserting riverBoards into DB: {0}%", 1.0d * ind / riverList.Count * 100);
                }
                if (CDBHelper.IsBoardHandInsideHandsTable(item.Item1.PBoardMask))
                {
                    continue;
                }
                Parallel.ForEach(Hand.Hands(0, item.Item1.PBoardMask, 2), pocketMask =>
                {
                    var temp = new CHandModel(pocketMask, item.Item1.PBoardMask);
                    lock (tempList)
                    {
                        tempList.Add(temp);
                    }
                });
                if (tempList.Count > 100000)
                {
                    //Console.WriteLine("Inserting riverBoards into DB: {0}%", 1.0d * ind / riverList.Count * 100);
                    CDBHelper.InsertHands(tempList);
                    tempList.Clear();
                    GC.Collect();
                }
            }
            CDBHelper.InsertHands(tempList);
            tempList.Clear();
            GC.Collect();

            #endregion
        }




        static void Main(string[] args)
        {
            //InsertAllPocketIntoDB();
            GenerateAllHandsV2();
        }
    }
}
