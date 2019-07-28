using Amigo.Models.MyModels.AveragePlayerValueHands;
using Amigo.Models.MyModels.FoldStats;
using Amigo.Models.MyModels.GameState;
using Amigo.Models.MyModels.MadeHands;
using Amigo.Models.MyModels.MadeHands.Blockers;
using Amigo.Models.MyModels.MadeHands.FDOnly;
using Amigo.Models.MyModels.OtherStats;
using HoldemHand;
using Shared.Helpers;
using Shared.Models.Database;
using Shared.Poker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using static Amigo.Helpers.CDBHelperHandInfos;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CAction;
using static Shared.Poker.Models.CPlayer;
using static Shared.Poker.Models.CTableInfos;
using static Amigo.Helpers.EnumUtil;

namespace Amigo.Helpers
{
    public static class CDBHelper
    {
        static PokerAction PokerActionBDToPokerAction(PokerActionBD p)
        {
            if (p == PokerActionBD.None)
                return PokerAction.None;
            else
                return (PokerAction)(0x1 << ((int)p - 1));
        }

        private const byte CC_ALL_FLOP_GAME_STATES_COUNT = 100;
        private const short CC_ALL_TURN_GAME_STATES_COUNT = 906;
        private const short CC_ALL_RIVER_GAME_STATES_COUNT = 10198;
        private const int CC_ALL_AVERAGE_PLAYER_BLUFFS_FLOP = 36608;


        public static Dictionary<long, CFlopGameState> PDicAllFlopGameStatesByID = new Dictionary<long, CFlopGameState>(CC_ALL_FLOP_GAME_STATES_COUNT);
        public static Dictionary<(TypesPot, PokerPosition), long> PPreflopIDs = new Dictionary<(TypesPot, PokerPosition), long>(CC_ALL_FLOP_GAME_STATES_COUNT);
        public static Dictionary<(long, PokerAction, long?), CFlopGameState> PDicAllFlopGameStatesByInfos = new Dictionary<(long, PokerAction, long?), CFlopGameState>(CC_ALL_FLOP_GAME_STATES_COUNT);

        public static Dictionary<(long, PokerAction), List<(long?, long)>> PDicAllFlopGameStatesByInfosV2 = new Dictionary<(long, PokerAction), List<(long?, long)>>();
        //public static Dictionary<(TypesPot, PokerPosition, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>> PDicAllFlopGameTransitions = new Dictionary<(TypesPot, PokerPosition, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>>();

        public static Dictionary<long, CTurnGameState> PDicAllTurnGameStatesByID = new Dictionary<long, CTurnGameState>(CC_ALL_TURN_GAME_STATES_COUNT);
        public static Dictionary<(long, PokerAction, long?), CTurnGameState> PDicAllTurnGameStatesByInfos = new Dictionary<(long, PokerAction, long?), CTurnGameState>(CC_ALL_TURN_GAME_STATES_COUNT);
        public static Dictionary<(long, PokerAction), List<(long?, long)>> PDicAllTurnGameStatesByInfosV2 = new Dictionary<(long, PokerAction), List<(long?, long)>>();
        public static Dictionary<(long, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>> PDicAllTurnGameTransitions = new Dictionary<(long, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>>();

        public static Dictionary<long, CRiverGameState> PDicAllRiverGameStatesByID = new Dictionary<long, CRiverGameState>(CC_ALL_RIVER_GAME_STATES_COUNT);
        public static Dictionary<(long, PokerAction, long?), CRiverGameState> PDicAllRiverGameStatesByInfos = new Dictionary<(long, PokerAction, long?), CRiverGameState>(CC_ALL_RIVER_GAME_STATES_COUNT);
        public static Dictionary<(long, PokerAction), List<(long?, long)>> PDicAllRiverGameStatesByInfosV2 = new Dictionary<(long, PokerAction), List<(long?, long)>>();
        public static Dictionary<(long, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>> PDicAllRiverGameTransitions = new Dictionary<(long, PokerAction), List<(long?, List<(BoardMetaDataFlags, long)>)>>();

        public static Dictionary<(TypesPot, PokerPosition), Dictionary<ulong, long>> PAveragePlayerPreflopRange = new Dictionary<(TypesPot, PokerPosition), Dictionary<ulong, long>>(1326); // 1st item = pocket mask, 2nd item = Sample count
        public static Dictionary<ulong, List<ulong>> PAllHandsByCard = new Dictionary<ulong, List<ulong>>(52);
        public static Dictionary<(TypesPot, PokerPosition), long> PDicTotalPreflopSampleCount = new Dictionary<(TypesPot, PokerPosition), long>();         

        public static Dictionary<(long, BoardMetaDataFlags), long> PAllRangeSamplesFlop = new Dictionary<(long, BoardMetaDataFlags), long>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(bool, bool, bool, bool, byte, double, long)>> PAveragePlayerBluffsFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(bool, bool, bool, bool, byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(byte, double, long)>> PAveragePlayerBluffsWithAlotsOfEquityFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), (double, long)> PAveragePlayerMadeHandSDFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(byte, double, long)>> PAveragePlayerMadeHandFDFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), (double, long)> PAveragePlayerMadeHandSDAndFDFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(double, double, long)>> PAveragePlayerValueHandsFlop = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(double, double, long)>>();

        public static Dictionary<(long, BoardMetaDataFlags), long> PAllRangeSamplesTurn = new Dictionary<(long, BoardMetaDataFlags), long>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(bool, bool, byte, double, long)>> PAveragePlayerBluffsTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(bool, bool, byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(byte, double, long)>> PAveragePlayerBluffsWithAlotsOfEquityTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), (double, long)> PAveragePlayerMadeHandSDTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(byte, double, long)>> PAveragePlayerMadeHandFDTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), (double, long)> PAveragePlayerMadeHandSDAndFDTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(double, double, long)>> PAveragePlayerValueHandsTurn = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(double, double, long)>>();


        public static Dictionary<(long, BoardMetaDataFlags), long> PAllRangeSamplesRiver = new Dictionary<(long, BoardMetaDataFlags), long>();
        public static Dictionary<(long, BoardMetaDataFlags), Dictionary<(BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority), (long, BoardMetaDataFlags)>> PGameStateWithSamples = new Dictionary<(long, BoardMetaDataFlags), Dictionary<(BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority), (long, BoardMetaDataFlags)>>();
        public static Dictionary<(long, BoardMetaDataFlags), Dictionary<(bool, bool), List<(byte, double, long)>>> PAveragePlayerBluffsRiver = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), Dictionary<(bool, bool), List<(byte, double, long)>>>();
        public static Dictionary<(long, BoardMetaDataFlags), List<(byte, double, long)>> PAveragePlayerBluffsWithAlotsOfEquityRiver = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
        public static Dictionary<(long, BoardMetaDataFlags), Dictionary<double, (double, long)>> PAveragePlayerValueHandsRiver = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), Dictionary<double, (double, long)>>();


        public static Dictionary<(Street, long, BoardMetaDataFlags, bool?), long> PGameStatesStats = new Dictionary<(Street, long, BoardMetaDataFlags, bool?), long>();

        public static Dictionary<ulong, Dictionary<CBoardModel, List<CHandModel>>> PAllBoards = new Dictionary<ulong, Dictionary<CBoardModel, List<CHandModel>>>();

        private static SQLiteConnection FFConnection = GetConnection();

        #region Insert queries
        #region Preflop queries
        public static void InsertAveragePlayerPreflopRange(ConcurrentBag<CAveragePlayerPreflopRange> _lstAvgPlayerPreflopRange)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerPreflopRange avgPlayer in _lstAvgPlayerPreflopRange)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerPreflopRanges(TypePot, Position, PocketMask, HandDescription, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT SampleCount FROM AveragePlayerPreflopRanges WHERE TypePot=? AND Position=? AND PocketMask=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PTypePot));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PPosition));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandDescription));

                        // For the select
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PTypePot));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PPosition));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PPocketMask));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        #endregion
        #region Flop queries        
        public static void InsertAveragePlayerBluffsFlop(ConcurrentBag<CAveragePlayerBluffsFlop> _lstAvgPlayerBluffsFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsFlop avgPlayer in _lstAvgPlayerBluffsFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsFlop(FlopGameStateID, BoardType, BoardHeat, BDFD, BDSD, SD, FD, IndexHighestCardExcludingBoard, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, ?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerBluffsFlop WHERE FlopGameStateID=? AND BoardType=? AND BDFD=? AND BDSD=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsFlop WHERE FlopGameStateID=? AND BoardType=? AND BDFD=? AND BDSD=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // 2nd select
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsFlopDebug(ConcurrentBag<Tuple<CAveragePlayerBluffsFlop, CDebugGeneralHandInfos>> _lstAvgPlayerBluffsFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (Tuple<CAveragePlayerBluffsFlop, CDebugGeneralHandInfos> infos in _lstAvgPlayerBluffsFlop)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsFlop(FlopGameStateID, BoardType, BoardHeat, BDFD, BDSD, SD, FD, IndexHighestCardExcludingBoard, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    CAveragePlayerBluffsFlop avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsBackdoorStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIndexHighestCardExcludingBoard)));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityFlop(ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityFlop> _lstAveragePlayerBluffsWithLotsOfEquityFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsWithLotsOfEquityFlop avgPlayer in _lstAveragePlayerBluffsWithLotsOfEquityFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsWithALotsOfEquityFlop (FlopGameStateID, BoardType, BoardHeat, NbOuts, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerBluffsWithALotsOfEquityFlop WHERE FlopGameStateID=? AND BoardType=? AND NbOuts=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsWithALotsOfEquityFlop WHERE FlopGameStateID=? AND BoardType=? AND NbOuts=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));
                                               
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityDebugFlop(ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityFlop, CDebugGeneralHandInfos>> _lstAveragePlayerBluffsWithLotsOfEquityFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerBluffsWithLotsOfEquityFlop)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsWithALotsOfEquityFlop (FlopGameStateID, BoardType, BoardHeat, NbOuts, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    CAveragePlayerBluffsWithLotsOfEquityFlop avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandSDAndFDFlop(ConcurrentBag<CAveragePlayerMadeHandSDAndFDFlop> _lstAveragePlayerMadeHandSDAndFDFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandSDAndFDFlop avgPlayer in _lstAveragePlayerMadeHandSDAndFDFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandSDAndFDFlop (FlopGameStateID, BoardType, BoardHeat, UnifiedCount, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandSDAndFDFlop WHERE FlopGameStateID=? AND BoardType=?), 0) + ?,COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandSDAndFDFlop WHERE FlopGameStateID=? AND BoardType=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandSDAndFDDebugFlop(ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDFlop, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandSDAndFDFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandSDAndFDFlop)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandSDAndFDFlop (FlopGameStateID, BoardType, BoardHeat, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandFDFlop(ConcurrentBag<CAveragePlayerMadeHandFDFlop> _lstAveragePlayerMadeHandFDFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandFDFlop avgPlayer in _lstAveragePlayerMadeHandFDFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandFDFlop (FlopGameStateID, BoardType, BoardHeat, IndexHighestCardExcludingBoardOfFlushCard, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandFDFlop WHERE FlopGameStateID=? AND BoardType=? AND IndexHighestCardExcludingBoardOfFlushCard=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandFDFlop WHERE FlopGameStateID=? AND BoardType=? AND IndexHighestCardExcludingBoardOfFlushCard=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandFDDebugFlop(ConcurrentBag<Tuple<CAveragePlayerMadeHandFDFlop, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandFDFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandFDFlop)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandFDFlop (FlopGameStateID, BoardType, BoardHeat, IndexHighestCardExcludingBoardOfFlushCard, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    CAveragePlayerMadeHandFDFlop avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandSDFlop(ConcurrentBag<CAveragePlayerMadeHandSDFlop> _lstAveragePlayerMadeHandSDFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandSDFlop avgPlayer in _lstAveragePlayerMadeHandSDFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandSDFlop (FlopGameStateID, BoardType, BoardHeat, UnifiedCount, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandSDFlop WHERE FlopGameStateID=? AND BoardType=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandSDFlop WHERE FlopGameStateID=? AND BoardType=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandSDDebugFlop(ConcurrentBag<Tuple<CAveragePlayerMadeHandSDFlop, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandSDFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandSDFlop)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandSDFlop (FlopGameStateID, BoardType, BoardHeat, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerValueHandsFlop(ConcurrentBag<CAveragePlayerValueHandsFlop> _lstAveragePlayerValueHandsFlop)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerValueHandsFlop avgPlayer in _lstAveragePlayerValueHandsFlop)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerValueHandsFlop (FlopGameStateID, BoardType, BoardHeat, HandStrength, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerValueHandsFlop WHERE FlopGameStateID=? AND BoardType=? AND HandStrength=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerValueHandsFlop WHERE FlopGameStateID=? AND BoardType=? AND HandStrength=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the 2nd select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerValueHandsDebugFlop(ConcurrentBag<Tuple<CAveragePlayerValueHandsFlop, CDebugGeneralHandInfos>> _lstAveragePlayerValueHandsFlop)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerValueHandsFlop)
                {
                    string sql = "INSERT INTO AveragePlayerValueHandsFlop (FlopGameStateID, BoardType, BoardHeat, HandStrength, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertFlopFoldStats(ConcurrentBag<CFlopFoldStats> _lstFlopFoldStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var flopStat in _lstFlopFoldStats)
                {
                    string sql = "INSERT OR REPLACE INTO FlopAllGameStatesFoldStats (FlopGameStateID, BoardType, BoardHeat, CanRaise, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT SampleCount FROM FlopAllGameStatesFoldStats WHERE FlopGameStateID=? AND BoardType=? AND CanRaise=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PCanRaise));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PCanRaise));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertFlopOtherStats(ConcurrentBag<CFlopOtherStats> _lstFlopOtherStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var flopStat in _lstFlopOtherStats)
                {
                    string sql = "INSERT OR REPLACE INTO FlopAllGameStatesOtherStats (FlopGameStateID, BoardType, BoardHeat, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT SampleCount FROM FlopAllGameStatesOtherStats WHERE FlopGameStateID=? AND BoardType=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardHeat));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", flopStat.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        #endregion
        #region Turn queries
        public static void InsertAveragePlayerBluffsTurn(ConcurrentBag<CAveragePlayerBluffsTurn> _lstAvgPlayerBluffsTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsTurn avgPlayer in _lstAvgPlayerBluffsTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsTurn(TurnGameStateID, BoardType, BoardHeat, SD, FD, IndexHighestCardExcludingBoard, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerBluffsTurn WHERE TurnGameStateID=? AND BoardType=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsTurn WHERE TurnGameStateID=? AND BoardType=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsTurnDebug(ConcurrentBag<Tuple<CAveragePlayerBluffsTurn, CDebugGeneralHandInfos>> _lstAvgPlayerBluffsTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAvgPlayerBluffsTurn)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsTurn(TurnGameStateID, BoardType, BoardHeat, SD, FD, IndexHighestCardExcludingBoard, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityTurn(ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityTurn> _lstAveragePlayerBluffsWithLotsOfEquityTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsWithLotsOfEquityTurn avgPlayer in _lstAveragePlayerBluffsWithLotsOfEquityTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID, BoardType, BoardHeat, NbOuts, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerBluffsWithALotsOfEquityTurn WHERE TurnGameStateID=? AND BoardType=? AND NbOuts=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsWithALotsOfEquityTurn WHERE TurnGameStateID=? AND BoardType=? AND NbOuts=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityTurnDebug(ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityTurn, CDebugGeneralHandInfos>> _lstAveragePlayerBluffsWithLotsOfEquityTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerBluffsWithLotsOfEquityTurn)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID, BoardType, BoardHeat, NbOuts, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PNumberOfOuts));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandSDAndFDTurn(ConcurrentBag<CAveragePlayerMadeHandSDAndFDTurn> _lstAveragePlayerMadeHandSDAndFDTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandSDAndFDTurn avgPlayer in _lstAveragePlayerMadeHandSDAndFDTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandSDAndFDTurn (TurnGameStateID, BoardType, BoardHeat, UnifiedCount, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandSDAndFDTurn WHERE TurnGameStateID=? AND BoardType=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandSDAndFDTurn WHERE TurnGameStateID=? AND BoardType=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandSDAndFDTurnDebug(ConcurrentBag<Tuple<CAveragePlayerMadeHandSDAndFDTurn, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandSDAndFDTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandSDAndFDTurn)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandSDAndFDTurn (TurnGameStateID, BoardType, BoardHeat, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandFDTurn(ConcurrentBag<CAveragePlayerMadeHandFDTurn> _lstAveragePlayerMadeHandFDTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandFDTurn avgPlayer in _lstAveragePlayerMadeHandFDTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandFDTurn (TurnGameStateID, BoardType, BoardHeat, IndexHighestCardExcludingBoardOfFlushCard, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandFDTurn WHERE TurnGameStateID=? AND BoardType=? AND IndexHighestCardExcludingBoardOfFlushCard=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandFDTurn WHERE TurnGameStateID=? AND BoardType=? AND IndexHighestCardExcludingBoardOfFlushCard=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandFDTurnDebug(ConcurrentBag<Tuple<CAveragePlayerMadeHandFDTurn, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandFDTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandFDTurn)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandFDTurn (TurnGameStateID, BoardType, BoardHeat, IndexHighestCardExcludingBoardOfFlushCard, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoardOfFlushCard));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandSDTurn(ConcurrentBag<CAveragePlayerMadeHandSDTurn> _lstAveragePlayerMadeHandSDTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandSDTurn avgPlayer in _lstAveragePlayerMadeHandSDTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandSDTurn (TurnGameStateID, BoardType, BoardHeat, UnifiedCount, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerMadeHandSDTurn WHERE TurnGameStateID=? AND BoardType=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandSDTurn WHERE TurnGameStateID=? AND BoardType=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandSDTurnDebug(ConcurrentBag<Tuple<CAveragePlayerMadeHandSDTurn, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandSDTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandSDTurn)
                {
                    string sql = "INSERT INTO AveragePlayerMadeHandSDTurn (TurnGameStateID, BoardType, BoardHeat, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerValueHandsTurn(ConcurrentBag<CAveragePlayerValueHandsTurn> _lstAveragePlayerValueHandsTurn)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerValueHandsTurn avgPlayer in _lstAveragePlayerValueHandsTurn)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerValueHandsTurn (TurnGameStateID, BoardType, BoardHeat, HandStrength, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerValueHandsTurn WHERE TurnGameStateID=? AND BoardType=? AND HandStrength=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerValueHandsTurn WHERE TurnGameStateID=? AND BoardType=? AND HandStrength=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerValueHandsTurnDebug(ConcurrentBag<Tuple<CAveragePlayerValueHandsTurn, CDebugGeneralHandInfos>> _lstAveragePlayerValueHandsTurn)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerValueHandsTurn)
                {
                    string sql = "INSERT INTO AveragePlayerValueHandsTurn (TurnGameStateID, BoardType, BoardHeat, HandStrength, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertTurnFoldStats(ConcurrentBag<CTurnFoldStats> _lstTurnFoldStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var turnStat in _lstTurnFoldStats)
                {
                    string sql = "INSERT OR REPLACE INTO TurnAllGameStatesFoldStats (TurnGameStateID, BoardType, BoardHeat, CanRaise, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT SampleCount FROM TurnAllGameStatesFoldStats WHERE TurnGameStateID=? AND BoardType=? AND CanRaise=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PCanRaise));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PCanRaise));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertTurnOtherStats(ConcurrentBag<CTurnOtherStats> _lstTurnOtherStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var turnStat in _lstTurnOtherStats)
                {
                    string sql = "INSERT OR REPLACE INTO TurnAllGameStatesOtherStats (TurnGameStateID, BoardType, BoardHeat, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT SampleCount FROM TurnAllGameStatesOtherStats WHERE TurnGameStateID=? AND BoardType=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardHeat));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", turnStat.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        #endregion
        #region River queries
        public static void InsertAveragePlayerBluffsRiver(ConcurrentBag<CAveragePlayerBluffsRiver> _lstAvgPlayerBluffsRiver)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsRiver avgPlayer in _lstAvgPlayerBluffsRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsRiver(RiverGameStateID, BoardType, BoardHeat, SD, FD, IndexHighestCardExcludingBoard, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, ?, ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsRiver WHERE RiverGameStateID=? AND BoardType=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsRiver WHERE RiverGameStateID=? AND BoardType=? AND SD=? AND FD=? AND IndexHighestCardExcludingBoard=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsRiverDebug(ConcurrentBag<Tuple<CAveragePlayerBluffsRiver, CDebugGeneralHandInfos>> _lstAvgPlayerBluffsRiver)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAvgPlayerBluffsRiver)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsRiver(RiverGameStateID, BoardType, BoardHeat, SD, FD, IndexHighestCardExcludingBoard, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsStraightDraw)));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PIsFlushDraw)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PIndexHighestCardExcludingBoard));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityRiver(ConcurrentBag<CAveragePlayerBluffsWithLotsOfEquityRiver> _lstAveragePlayerBluffsWithLotsOfEquityRiver)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerBluffsWithLotsOfEquityRiver avgPlayer in _lstAveragePlayerBluffsWithLotsOfEquityRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerBluffsWithALotsOfEquityRiver (RiverGameStateID, BoardType, BoardHeat, NbOuts, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT UnifiedCount FROM AveragePlayerBluffsWithALotsOfEquityRiver WHERE RiverGameStateID=? AND BoardType=? AND NbOuts=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerBluffsWithALotsOfEquityRiver WHERE RiverGameStateID=? AND BoardType=? AND NbOuts=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", Convert.ToSByte(avgPlayer.PNumberOfOuts)));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerBluffsWithALotsOfEquityDebugRiver(ConcurrentBag<Tuple<CAveragePlayerBluffsWithLotsOfEquityRiver, CDebugGeneralHandInfos>> _lstAveragePlayerBluffsWithLotsOfEquityRiver)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerBluffsWithLotsOfEquityRiver)
                {
                    string sql = "INSERT INTO AveragePlayerBluffsWithALotsOfEquityRiver (RiverGameStateID, BoardType, BoardHeat, NbOuts, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PNumberOfOuts));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerMadeHandsBlockersRiver(ConcurrentBag<CAveragePlayerMadeHandBlockerRiver> _lstAveragePlayerMadeHandBlockerRiver)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerMadeHandBlockerRiver avgPlayer in _lstAveragePlayerMadeHandBlockerRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandBlockersRiver (RiverGameStateID, BoardType, BoardHeat, BlockerRatio, HandStrengthInBlockerRange, SampleCount) VALUES (?, ?, ?, ?, ?, COALESCE((SELECT SampleCount FROM AveragePlayerMadeHandBlockersRiver WHERE RiverGameStateID=? AND BoardType=? AND BlockerRatio=? AND HandStrengthInBlockerRange=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBlockerRatio));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrengthInBlockerRange));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBlockerRatio));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrengthInBlockerRange));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerMadeHandsBlockersDebugRiver(ConcurrentBag<Tuple<CAveragePlayerMadeHandBlockerRiver, CDebugGeneralHandInfos>> _lstAveragePlayerMadeHandBlockerRiver)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerMadeHandBlockerRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerMadeHandBlockersRiver (RiverGameStateID, BoardType, BoardHeat, BlockerRatio, HandStrengthInBlockerRange, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBlockerRatio));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrengthInBlockerRange));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertAveragePlayerValueHandsRiver(ConcurrentBag<CAveragePlayerValueHandsRiver> _lstAveragePlayerValueHandsRiver)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (CAveragePlayerValueHandsRiver avgPlayer in _lstAveragePlayerValueHandsRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerValueHandsRiver (RiverGameStateID, BoardType, BoardHeat, HandStrength, UnifiedCount, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT SampleCount FROM AveragePlayerValueHandsRiver WHERE RiverGameStateID=? AND BoardType=? AND HandStrength=?), 0) + ?, COALESCE((SELECT SampleCount FROM AveragePlayerValueHandsRiver WHERE RiverGameStateID=? AND BoardType=? AND HandStrength=?), 0) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PUnifiedCount));

                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertAveragePlayerValueHandsDebugRiver(ConcurrentBag<Tuple<CAveragePlayerValueHandsRiver, CDebugGeneralHandInfos>> _lstAveragePlayerValueHandsRiver)
        {
            SQLiteConnection oldConnection = FFConnection;

            FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var infos in _lstAveragePlayerValueHandsRiver)
                {
                    string sql = "INSERT OR REPLACE INTO AveragePlayerValueHandsRiver (RiverGameStateID, BoardType, BoardHeat, HandStrength, HandMask, BoardMask, HandDescription, BoardDescription, HandHistory) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    var avgPlayer = infos.Item1;
                    CDebugGeneralHandInfos currentHandInfos = infos.Item2;

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", avgPlayer.PHandStrength));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PPocketMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardMask));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PBoardDescription));
                        command.Parameters.Add(new SQLiteParameter("", currentHandInfos.PHandHistory));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            FFConnection = oldConnection;
        }
        public static void InsertRiverFoldStats(ConcurrentBag<CRiverFoldStats> _lstRiverFoldStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var riverStat in _lstRiverFoldStats)
                {
                    string sql = "INSERT OR REPLACE INTO RiverAllGameStatesFoldStats (RiverGameStateID, BoardType, BoardHeat, CanRaise, SampleCount) VALUES (?, ?, ?, ?, COALESCE((SELECT SampleCount FROM RiverAllGameStatesFoldStats WHERE RiverGameStateID=? AND BoardType=? AND CanRaise=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardHeat));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PCanRaise));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PCanRaise));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        public static void InsertRiverOtherStats(ConcurrentBag<CRiverOtherStats> _lstRiverOtherStats)
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
            {
                foreach (var riverStat in _lstRiverOtherStats)
                {
                    string sql = "INSERT OR REPLACE INTO RiverAllGameStatesOtherStats (RiverGameStateID, BoardType, BoardHeat, SampleCount) VALUES (?, ?, ?, COALESCE((SELECT SampleCount FROM RiverAllGameStatesOtherStats WHERE RiverGameStateID=? AND BoardType=?), -1) + 1);";

                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardType));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardHeat));

                        // For the select here
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PGameState.PID));
                        command.Parameters.Add(new SQLiteParameter("", riverStat.PBoardType));

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
        #endregion
        #endregion
        #region Select queries
        #region Preflop queries
        public static void LoadAllAveragePlayerPreflopRanges()
        {
            var allTypesPot = new HashSet<(TypesPot, PokerPosition)>() { { (TypesPot.Limped, PokerPosition.BTN) },
                                                                             { (TypesPot.Limped, PokerPosition.BB) },
                                                                             { (TypesPot.RaisedLimped, PokerPosition.BTN) },
                                                                             { (TypesPot.RaisedLimped, PokerPosition.BB) },
                                                                             { (TypesPot.TwoBet, PokerPosition.BTN) },
                                                                             { (TypesPot.TwoBet, PokerPosition.BB) },
                                                                             { (TypesPot.ThreeBet, PokerPosition.BTN) },
                                                                             { (TypesPot.ThreeBet, PokerPosition.BB) },
                                                                             { (TypesPot.FourBet, PokerPosition.BTN) },
                                                                             { (TypesPot.FourBet, PokerPosition.BB) },
                                                                             { (TypesPot.FiveBetEtPlus, PokerPosition.BTN) },
                                                                             { (TypesPot.FiveBetEtPlus, PokerPosition.BB) } };

            foreach (var key in allTypesPot)
            {
                if (FFConnection.State == System.Data.ConnectionState.Closed)
                    FFConnection.Open();

                string sql = "SELECT * FROM AveragePlayerPreflopRanges WHERE TypePot=? AND Position=?;";

                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter("", key.Item1));
                    command.Parameters.Add(new SQLiteParameter("", key.Item2));

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        long totalSampleCount = 0;

                        PAveragePlayerPreflopRange.Add(key, new Dictionary<ulong, long>(1326));

                        foreach(var hand in Hand.Hands(0, 0, 2))                                                    
                            PAveragePlayerPreflopRange[key].Add(hand, 0);                                                    

                        while (reader.Read())
                        {
                            ulong pocketMask = reader.GetFieldData<ulong>(2);
                            long sampleCount = reader.GetFieldData<long>(4);
                            
                            PAveragePlayerPreflopRange[key][pocketMask] += sampleCount;
                            totalSampleCount += sampleCount;
                        }

                        if (!PDicTotalPreflopSampleCount.ContainsKey(key))
                            PDicTotalPreflopSampleCount.Add(key, 0);

                        PDicTotalPreflopSampleCount[key] = totalSampleCount;
                    }
                }                
            }
        }
        #endregion
        #region Flop queries        
        public static void LoadAllAveragePlayerBluffsFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, bool, bool, bool, bool, byte), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerBluffsFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool isBDFD = reader.GetFieldData<bool>(3);
                        bool isBDSD = reader.GetFieldData<bool>(4);
                        bool isSD = reader.GetFieldData<bool>(5);
                        bool isFD = reader.GetFieldData<bool>(6);
                        byte index = reader.GetFieldData<byte>(7);
                        double unifiedCount = reader.GetFieldData<double>(8);
                        long sampleCount = reader.GetFieldData<long>(9);

                        rawTable.Add((flopGameState.PID, boardType, isBDFD, isBDSD, isSD, isFD, index), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(bool, bool, bool, bool, byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Key.Item4, row.Key.Item5, row.Key.Item6, row.Key.Item7, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                    data.Add(key, new List<(bool, bool, bool, bool, byte, double, long)>());

                data[key].Add(value);
            }

            PAveragePlayerBluffsFlop = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));
        }
        public static void LoadAllAveragePlayerWithALotsOfEquityFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, byte), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerBluffsWithALotsOfEquityFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        byte numberOfOuts = reader.GetFieldData<byte>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);

                        // insert here
                        rawTable.Add((flopGameState.PID, boardType, numberOfOuts), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                if (!data.ContainsKey(key))
                {
                    data.Add(key, new List<(byte, double, long)>());
                }

                data[key].Add((row.Key.Item3, value.Item1, value.Item2));
            }


            PAveragePlayerBluffsWithAlotsOfEquityFlop = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));

            foreach (var item in PAveragePlayerBluffsWithAlotsOfEquityFlop)
            {
                PAveragePlayerBluffsWithAlotsOfEquityFlop[item.Key].Sort((x, y) =>
                {
                    if (x.Item1 > y.Item1) return -1;
                    if (x.Item1 < y.Item1) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAllAveragePlayerMadeHandFDFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, byte), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerMadeHandFDFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        byte highestCardExcludingBoardOfFlush = reader.GetFieldData<byte>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);

                        // insert here
                        rawTable.Add((flopGameState.PID, boardType, highestCardExcludingBoardOfFlush), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                {
                    data.Add(key, new List<(byte, double, long)>());
                }

                data[key].Add(value);
            }

            PAveragePlayerMadeHandFDFlop = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));

            foreach (var item in PAveragePlayerMadeHandFDFlop)
            {
                PAveragePlayerMadeHandFDFlop[item.Key].Sort((x, y) =>
                {
                    if (x.Item2 > y.Item2) return -1;
                    if (x.Item2 < y.Item2) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAllAveragePlayerMadeHandSDAndFDFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerMadeHandSDAndFDFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double unifiedCount = reader.GetFieldData<double>(3);
                        int sampleCount = reader.GetFieldData<int>(4);
                        // insert here

                        rawTable.Add((flopGameState.PID, boardType), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                data.Add(key, (value.Item1, value.Item2));
            }

            PAveragePlayerMadeHandSDAndFDFlop = data.ToDictionary(x => x.Key, x => x.Value);
        }
        public static void LoadAllAveragePlayerMadeHandSDFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerMadeHandSDFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double unifiedCount = reader.GetFieldData<double>(3);
                        int sampleCount = reader.GetFieldData<int>(4);
                        // insert here

                        rawTable.Add((flopGameState.PID, boardType), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                data.Add(key, (value.Item1, value.Item2));
            }

            PAveragePlayerMadeHandSDFlop = data.ToDictionary(x => x.Key, x => x.Value);
        }
        public static void LoadAllAveragePlayerValueHandsFlop()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, double), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerValueHandsFlop;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double handStrength = reader.GetFieldData<double>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        long sampleCount = reader.GetFieldData<long>(5);

                        rawTable.Add((flopGameState.PID, boardType, handStrength), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(double, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                    data.Add(key, new List<(double, double, long)>());

                data[key].Add(value);
            }


            PAveragePlayerValueHandsFlop = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => (y.Item1, y.Item2, y.Item3)));

            foreach (var item in PAveragePlayerValueHandsTurn)
            {
                PAveragePlayerValueHandsTurn[item.Key].Sort((x, y) =>
                {
                    if (x.Item1 > y.Item1) return -1;
                    if (x.Item1 < y.Item1) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAllFlopGameStatesFoldStats()
        {

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM FlopAllGameStatesFoldStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool canRaise = reader.GetFieldData<bool>(3);
                        long sampleCount = reader.GetFieldData<long>(4);
                        PGameStatesStats.Add((Street.Flop, flopGameState.PID, boardType, canRaise), sampleCount);
                        if (sampleCount > 0)
                        {
                            

                            /*var state_id_key = new AbstractStateID(Street.Flop, flopGameState.PID);
                            var state_key = new AbstractState(state_id_key, boardType);
                            var action_key = new AbstractAction(PokerAction.Fold, null);

                            if (!PStateTransitions.ContainsKey(state_key))
                                PStateTransitions.Add(state_key, new List<AbstractActionCount>());

                            AbstractActionCount state_count_key;

                            if (PStateTransitions[state_key].Count > 0)
                                state_count_key = new AbstractActionCount(action_key, sampleCount + PStateTransitions[state_key].Last().count);
                            else
                                state_count_key = new AbstractActionCount(action_key, sampleCount);

                            PStateTransitions[state_key].Add(state_count_key);*/
                        }
                    }
                }
            }
        }
        public static void LoadAllFlopGameStatesOtherStats()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM FlopAllGameStatesOtherStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CFlopGameState flopGameState = PDicAllFlopGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        long sampleCount = reader.GetFieldData<long>(3);

                        //if (sampleCount > 0)
                            PGameStatesStats.Add((Street.Flop, flopGameState.PID, boardType, null), sampleCount);
                    }
                }
            }
        }        
        #endregion
        #region Turn queries
        public static void LoadAllAveragePlayerBluffsTurn()
        {
            //CREATE TABLE AveragePlayerBluffsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard)
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, bool, bool, byte), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerBluffsTurn;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool isSD = reader.GetFieldData<bool>(3);
                        bool isFD = reader.GetFieldData<bool>(4);
                        byte index = reader.GetFieldData<byte>(5);
                        double unifiedCount = reader.GetFieldData<double>(6);
                        long sampleCount = reader.GetFieldData<long>(7);

                        rawTable.Add((turnGameState.PID, boardType, isSD, isFD, index), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(bool, bool, byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Key.Item4, row.Key.Item5, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                    data.Add(key, new List<(bool, bool, byte, double, long)>());

                data[key].Add(value);
            }

            PAveragePlayerBluffsTurn = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));
        }
        public static void LoadAveragePlayerValueHandsTurnTable()
        {
            //CREATE TABLE AveragePlayerValueHandsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, HandStrength),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, HandStrength)
            const string sql = "SELECT * FROM AveragePlayerValueHandsTurn;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, double), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double handStrength = reader.GetFieldData<double>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        long sampleCount = reader.GetFieldData<long>(5);

                        rawTable.Add((turnGameState.PID, boardType, handStrength), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(double, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                    data.Add(key, new List<(double, double, long)>());

                data[key].Add(value);
            }


            PAveragePlayerValueHandsTurn = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => (y.Item1, y.Item2, y.Item3)));

            foreach (var item in PAveragePlayerValueHandsTurn)
            {
                PAveragePlayerValueHandsTurn[item.Key].Sort((x, y) =>
                {
                    if (x.Item1 > y.Item1) return -1;
                    if (x.Item1 < y.Item1) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAverageMadeHandSDTurnTable()
        {
            //CREATE TABLE AveragePlayerMadeHandSDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))           
            //PRIMARY KEY(TurnGameStateID, BoardType)
            const string sql = "SELECT * FROM AveragePlayerMadeHandSDTurn;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double unifiedCount = reader.GetFieldData<double>(3);
                        int sampleCount = reader.GetFieldData<int>(4);
                        // insert here

                        rawTable.Add((turnGameState.PID, boardType), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                data.Add(key, (value.Item1, value.Item2));
            }

            PAveragePlayerMadeHandSDTurn = data.ToDictionary(x => x.Key, x => x.Value);
        }
        public static void LoadAverageMadeHandFDTurnTable()
        {
            //CREATE TABLE AveragePlayerMadeHandFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,IndexHighestCardExcludingBoardOfFlushCard INTEGER(1),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, IndexHighestCardExcludingBoardOfFlushCard),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, IndexHighestCardExcludingBoardOfFlushCard)
            const string sql = "SELECT * FROM AveragePlayerMadeHandFDTurn;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, byte), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        byte highestCardExcludingBoardOfFlush = reader.GetFieldData<byte>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);

                        // insert here
                        rawTable.Add((turnGameState.PID, boardType, highestCardExcludingBoardOfFlush), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item3, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                {
                    data.Add(key, new List<(byte, double, long)>());
                }

                data[key].Add(value);
            }

            PAveragePlayerMadeHandFDTurn = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));

            foreach (var item in PAveragePlayerMadeHandFDTurn)
            {
                PAveragePlayerMadeHandFDTurn[item.Key].Sort((x, y) =>
                {
                    if (x.Item2 > y.Item2) return -1;
                    if (x.Item2 < y.Item2) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAverageMadeHandSDAndFDTurn()
        {
            //CREATE TABLE AveragePlayerMadeHandSDAndFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType)
            const string sql = "SELECT * FROM AveragePlayerMadeHandSDAndFDTurn;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double unifiedCount = reader.GetFieldData<double>(3);
                        int sampleCount = reader.GetFieldData<int>(4);
                        // insert here

                        rawTable.Add((turnGameState.PID, boardType), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), (double, long)>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                data.Add(key, (value.Item1, value.Item2));
            }

            PAveragePlayerMadeHandSDAndFDTurn = data.ToDictionary(x => x.Key, x => x.Value);
        }
        public static void LoadAveragePlayerBluffsWithALotsOfEquityTurn()
        {
            //CREATE TABLE AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, NbOuts),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, NbOuts)
            const string sql = "SELECT * FROM AveragePlayerBluffsWithALotsOfEquityTurn;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, byte), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState flopGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        byte numberOfOuts = reader.GetFieldData<byte>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);

                        // insert here
                        rawTable.Add((flopGameState.PID, boardType, numberOfOuts), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                if (!data.ContainsKey(key))
                {
                    data.Add(key, new List<(byte, double, long)>());
                }

                data[key].Add((row.Key.Item3, value.Item1, value.Item2));
            }


            PAveragePlayerBluffsWithAlotsOfEquityTurn = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));

            foreach (var item in PAveragePlayerBluffsWithAlotsOfEquityTurn)
            {
                PAveragePlayerBluffsWithAlotsOfEquityTurn[item.Key].Sort((x, y) =>
                {
                    if (x.Item1 > y.Item1) return -1;
                    if (x.Item1 < y.Item1) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAllTurnGameStatesFoldStats()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM TurnAllGameStatesFoldStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool canRaise = reader.GetFieldData<bool>(3);
                        long sampleCount = reader.GetFieldData<long>(4);
                        PGameStatesStats.Add((Street.Turn, turnGameState.PID, boardType, canRaise), sampleCount);
                        if (sampleCount > 0)
                        {
                            

                            /*var state_id_key = new AbstractStateID(Street.Turn, turnGameState.PID);
                            var state_key = new AbstractState(state_id_key, boardType);
                            var action_key = new AbstractAction(PokerAction.Fold, null);

                            if (!PStateTransitions.ContainsKey(state_key))
                                PStateTransitions.Add(state_key, new List<AbstractActionCount>());

                            AbstractActionCount state_count_key;

                            if (PStateTransitions[state_key].Count > 0)
                                state_count_key = new AbstractActionCount(action_key, sampleCount + PStateTransitions[state_key].Last().count);
                            else
                                state_count_key = new AbstractActionCount(action_key, sampleCount);

                            PStateTransitions[state_key].Add(state_count_key);*/
                        }
                    }
                }
            }         
        }
        public static void LoadAllTurnGameStatesOtherStats()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM TurnAllGameStatesOtherStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTurnGameState turnGameState = PDicAllTurnGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        long sampleCount = reader.GetFieldData<long>(3);

                        //if (sampleCount > 0)
                            PGameStatesStats.Add((Street.Turn, turnGameState.PID, boardType, null), sampleCount);
                    }
                }
            }
        }
        #endregion
        #region River queries
        public static void LoadAllRiverGameStatesWithSample()
        {
            if (PDicAllRiverGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load PDicAllRiverGameStatesByID and PDicAllRiverGameStatesByInfos before");
            else if (PDicAllRiverBoardTypesByGroupType.Count == 0)
                throw new InvalidOperationException("Must load DicAllriverBoardTypesByGroupType before");

            #region Initialization
            var allPermutationPriorityTuples = new List<(BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority)>(6);

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.PairedTypeHandGroup,
                                              BoardMetaDataFlagsPriority.StraightTypeHandGroup,
                                              BoardMetaDataFlagsPriority.FlushTypeHandGroup));

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.PairedTypeHandGroup, 
                                              BoardMetaDataFlagsPriority.FlushTypeHandGroup, 
                                              BoardMetaDataFlagsPriority.StraightTypeHandGroup));

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.StraightTypeHandGroup,
                                              BoardMetaDataFlagsPriority.PairedTypeHandGroup,
                                              BoardMetaDataFlagsPriority.FlushTypeHandGroup));

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.StraightTypeHandGroup,
                                              BoardMetaDataFlagsPriority.FlushTypeHandGroup,
                                              BoardMetaDataFlagsPriority.PairedTypeHandGroup));

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.FlushTypeHandGroup,
                                              BoardMetaDataFlagsPriority.PairedTypeHandGroup,
                                              BoardMetaDataFlagsPriority.StraightTypeHandGroup));

            allPermutationPriorityTuples.Add((BoardMetaDataFlagsPriority.FlushTypeHandGroup,
                                              BoardMetaDataFlagsPriority.StraightTypeHandGroup,
                                              BoardMetaDataFlagsPriority.PairedTypeHandGroup));
            #endregion

            foreach(var riverGameState in PDicAllRiverGameStatesByID.Values)
            {
                foreach (var boardMetaData in CDBHelperHandInfos.PLstAllRiverBoardTypes)
                {
                    var tupleInfos = (riverGameState.PID, boardMetaData);
                    var numberOfSamples = PAllRangeSamplesRiver[tupleInfos];

                    PGameStateWithSamples.Add(tupleInfos, new Dictionary<(BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority, BoardMetaDataFlagsPriority), (long, BoardMetaDataFlags)>(allPermutationPriorityTuples.Count));

                    if (numberOfSamples > 0)
                    {
                        foreach (var permutation in allPermutationPriorityTuples)
                            PGameStateWithSamples[tupleInfos].Add(permutation, tupleInfos);
                    }
                    else
                    {
                        BoardMetaDataFlags firstBoardMetaDataPaired = (boardMetaData & BoardMetaDataFlags.Paired) | (boardMetaData & BoardMetaDataFlags.TwoPaired) | (boardMetaData & BoardMetaDataFlags.Trips) | (boardMetaData & BoardMetaDataFlags.FullHouse) | (boardMetaData & BoardMetaDataFlags.Quads);
                        BoardMetaDataFlags firstBoardMetaDataStraight = (boardMetaData & BoardMetaDataFlags.StraightDrawPossible) | (boardMetaData & BoardMetaDataFlags.StraightPossible) | (boardMetaData & BoardMetaDataFlags.OneCardStraightPossible) | (boardMetaData & BoardMetaDataFlags.StraightComplete);
                        BoardMetaDataFlags firstBoardMetaDataFlush = (boardMetaData & BoardMetaDataFlags.FlushDrawPossible) | (boardMetaData & BoardMetaDataFlags.FlushPossible) | (boardMetaData & BoardMetaDataFlags.OneCardFlushPossible) | (boardMetaData & BoardMetaDataFlags.FlushComplete) | (boardMetaData & BoardMetaDataFlags.StraightFlushComplete);

                        foreach (var tuplePermutation in allPermutationPriorityTuples)
                        {
                            BoardMetaDataFlags boardMetaDataPaired = firstBoardMetaDataPaired;
                            BoardMetaDataFlags boardMetaDataStraight = firstBoardMetaDataStraight;
                            BoardMetaDataFlags boardMetaDataFlush = firstBoardMetaDataFlush;
                            BoardMetaDataFlags boardTotal = (firstBoardMetaDataPaired | firstBoardMetaDataStraight | firstBoardMetaDataFlush);

                            #region Local methods
                            bool IsValidBoardType()
                            {
                                return PDicAllRiverBoardTypesByGroupType.ContainsKey(boardMetaDataPaired) &&
                                       PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired].ContainsKey(boardMetaDataStraight) &&
                                       PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired][boardMetaDataStraight].ContainsKey(boardMetaDataFlush);
                            }
                            void LoadNextDownPairedMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataPaired = PDicBoardMetaDataFlagsDown[boardMetaDataPaired];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNextDownStraightMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataStraight = PDicBoardMetaDataFlagsDown[boardMetaDataStraight];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNextDownFlushMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataFlush = PDicBoardMetaDataFlagsDown[boardMetaDataFlush];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNextUpPairedMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataPaired = PDicBoardMetaDataFlagsUp[boardMetaDataPaired];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNextUpStraightMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataStraight = PDicBoardMetaDataFlagsUp[boardMetaDataStraight];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNextUpFlushMetaData(int _numLevels)
                            {
                                int levelSkipped = 0;

                                while (levelSkipped++ < _numLevels)
                                    boardMetaDataFlush = PDicBoardMetaDataFlagsUp[boardMetaDataFlush];

                                boardTotal = (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush);
                            }
                            void LoadNumberOfSample()
                            {
                                if (IsValidBoardType())
                                {
                                    var key = (riverGameState.PID, boardTotal);

                                    if (PAllRangeSamplesRiver.ContainsKey(key))
                                        numberOfSamples = PAllRangeSamplesRiver[key];
                                }
                                else
                                    numberOfSamples = 0;
                            }
                            void Reset()
                            {
                                boardMetaDataPaired = firstBoardMetaDataPaired;
                                boardMetaDataStraight = firstBoardMetaDataStraight;
                                boardMetaDataFlush = firstBoardMetaDataFlush;
                                boardTotal = (firstBoardMetaDataPaired | firstBoardMetaDataStraight | firstBoardMetaDataFlush);
                            }
                            bool TryGoDown(int _numberOfLevels)
                            {
                                Reset();

                                switch (tuplePermutation.Item1)
                                {
                                    case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                        LoadNextDownPairedMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                                    LoadNextDownStraightMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.FlushTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownFlushMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                                    LoadNextDownFlushMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.StraightTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownStraightMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }

                                        break;
                                    case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                        LoadNextDownStraightMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                                    LoadNextDownPairedMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.FlushTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownFlushMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }

                                                    break;
                                                case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                                    LoadNextDownFlushMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.PairedTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownPairedMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }
                                        break;
                                    case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                        LoadNextDownFlushMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                                    LoadNextDownPairedMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.StraightTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownStraightMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                                    LoadNextDownStraightMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.PairedTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextDownPairedMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }
                                        break;
                                    default:
                                        throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                }

                                // If we arrived here, it means logically that we had atleast one sample since it returns false otherwise
                                PGameStateWithSamples[tupleInfos].Add(tuplePermutation, (riverGameState.PID, (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush)));
                                return true;
                            }
                            bool TryGoUp(int _numberOfLevels)
                            {
                                Reset();

                                switch (tuplePermutation.Item1)
                                {
                                    case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                        LoadNextUpPairedMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                                    LoadNextUpStraightMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.FlushTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpFlushMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                                    LoadNextUpFlushMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.StraightTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpStraightMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }

                                        break;
                                    case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                        LoadNextUpStraightMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                                    LoadNextUpPairedMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.FlushTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpFlushMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }

                                                    break;
                                                case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                                    LoadNextUpFlushMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.PairedTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpPairedMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }
                                        break;
                                    case BoardMetaDataFlagsPriority.FlushTypeHandGroup:
                                        LoadNextUpFlushMetaData(_numberOfLevels);
                                        LoadNumberOfSample();

                                        if (numberOfSamples <= 0)
                                        {
                                            Reset();

                                            switch (tuplePermutation.Item2)
                                            {
                                                case BoardMetaDataFlagsPriority.PairedTypeHandGroup:
                                                    LoadNextUpPairedMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.StraightTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpStraightMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                case BoardMetaDataFlagsPriority.StraightTypeHandGroup:
                                                    LoadNextUpStraightMetaData(_numberOfLevels);
                                                    LoadNumberOfSample();

                                                    if (numberOfSamples <= 0)
                                                    {
                                                        if (tuplePermutation.Item3 != BoardMetaDataFlagsPriority.PairedTypeHandGroup)
                                                            throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");

                                                        Reset();
                                                        LoadNextUpPairedMetaData(_numberOfLevels);
                                                        LoadNumberOfSample();

                                                        if (numberOfSamples <= 0)
                                                            return false;
                                                    }
                                                    break;
                                                default:
                                                    throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                            }
                                        }
                                        break;
                                    default:
                                        throw new InvalidOperationException("Invalid BoardMetaDataFlagsPriority!");
                                }

                                // If we arrived here, it means logically that we had atleast one sample since it returns false otherwise
                                PGameStateWithSamples[tupleInfos].Add(tuplePermutation, (riverGameState.PID, (boardMetaDataPaired | boardMetaDataStraight | boardMetaDataFlush)));
                                return true;
                            }
                            #endregion

                            if (!TryGoDown(1))
                            {
                                if (!TryGoDown(2))
                                {
                                    if (!TryGoUp(1))
                                    {
                                        if (!TryGoDown(3))
                                        {
                                            TryGoDown(4);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // testing stuff
            var countLst = PGameStateWithSamples.Values.Where(x => x.Count == 0).ToList();

            int numberofzero = 0;
            int notnumber = 0;

            int numberofzero2 = 0;
            int notnumber2 = 0;

            int numberofzero3 = 0;
            int notnumber3 = 0;

            foreach (var riverGameState in CDBHelper.PDicAllRiverGameStatesByID.Values)
            {
                foreach (var boardType in CDBHelperHandInfos.PLstAllRiverBoardTypes)
                {
                    var tupleInfos = (riverGameState.PID, boardType);

                    if (PGameStateWithSamples[tupleInfos].ContainsKey((BoardMetaDataFlagsPriority.FlushTypeHandGroup, BoardMetaDataFlagsPriority.StraightTypeHandGroup, BoardMetaDataFlagsPriority.PairedTypeHandGroup)))
                    {
                        var newTupleInfos = PGameStateWithSamples[tupleInfos][(BoardMetaDataFlagsPriority.FlushTypeHandGroup, BoardMetaDataFlagsPriority.StraightTypeHandGroup, BoardMetaDataFlagsPriority.PairedTypeHandGroup)];

                        if (PAllRangeSamplesRiver[newTupleInfos] == 0)
                            ++numberofzero;
                        else
                            ++notnumber;
                    }
                    else
                        ++numberofzero;

                    if (PAllRangeSamplesRiver[tupleInfos] == 0)
                        ++numberofzero2;
                    else
                        ++notnumber2;
                }                    
            }

            foreach(var flopGameState in CDBHelper.PDicAllFlopGameStatesByID.Values)
            {
                foreach(var boardType in CDBHelperHandInfos.PLstAllFlopBoardTypes)
                {
                    var tupleInfos = (flopGameState.PID, boardType);

                    if (PAllRangeSamplesFlop[tupleInfos] == 0)
                        ++numberofzero3;
                    else
                        ++notnumber3;
                }
            }
            int count3 = 0;

        }
        public static void LoadAllAveragePlayerBluffsRiver()
        {
            //CREATE TABLE AveragePlayerBluffsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard)
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, bool, bool, byte), (double, long)>();
            string sql = "SELECT * FROM AveragePlayerBluffsRiver;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool isSD = reader.GetFieldData<bool>(3);
                        bool isFD = reader.GetFieldData<bool>(4);
                        byte index = reader.GetFieldData<byte>(5);
                        double unifiedCount = reader.GetFieldData<double>(6);
                        long sampleCount = reader.GetFieldData<long>(7);

                        rawTable.Add((riverGameState.PID, boardType, isSD, isFD, index), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), Dictionary<(bool, bool), List<(byte, double, long)>>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Key.Item5, row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))                
                    data.Add(key, new Dictionary<(bool, bool), List<(byte, double, long)>>(50));                                                       

                if (!data[key].ContainsKey((row.Key.Item3, row.Key.Item4)))
                    data[key].Add((row.Key.Item3, row.Key.Item4), new List<(byte, double, long)>(20));

                data[key][(row.Key.Item3, row.Key.Item4)].Add(value);
            }

            PAveragePlayerBluffsRiver = data;
        }
        public static void LoadAveragePlayerValueHandsRiverTable()
        {
            //CREATE TABLE AveragePlayerValueHandsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, HandStrength),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, HandStrength)
            const string sql = "SELECT * FROM AveragePlayerValueHandsRiver;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, double), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double handStrength = reader.GetFieldData<double>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        long sampleCount = reader.GetFieldData<long>(5);

                        rawTable.Add((riverGameState.PID, boardType, handStrength), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), Dictionary<double, (double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = (row.Value.Item1, row.Value.Item2);

                if (!data.ContainsKey(key))
                    data.Add(key, new Dictionary<double, (double, long)>(50));

                data[key].Add(row.Key.Item3, value);
            }

            PAveragePlayerValueHandsRiver = data;
        }
        public static void LoadAllAveragePlayerWithALotsOfEquityRiver()
        {
            //CREATE TABLE AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, NbOuts),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID))
            //PRIMARY KEY(TurnGameStateID, BoardType, NbOuts)
            const string sql = "SELECT * FROM AveragePlayerBluffsWithALotsOfEquityRiver;";
            var rawTable = new Dictionary<(long, CBoardModel.BoardMetaDataFlags, byte), (double, long)>();

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        byte numberOfOuts = reader.GetFieldData<byte>(3);
                        double unifiedCount = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);

                        // insert here
                        rawTable.Add((riverGameState.PID, boardType, numberOfOuts), (unifiedCount, sampleCount));
                    }
                }
            }

            var data = new Dictionary<(long, CBoardModel.BoardMetaDataFlags), List<(byte, double, long)>>();
            foreach (var row in rawTable)
            {
                var key = (row.Key.Item1, row.Key.Item2);
                var value = row.Value;

                if (!data.ContainsKey(key))
                {
                    data.Add(key, new List<(byte, double, long)>());
                }

                data[key].Add((row.Key.Item3, value.Item1, value.Item2));
            }


            PAveragePlayerBluffsWithAlotsOfEquityRiver = data.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y));

            foreach (var item in PAveragePlayerBluffsWithAlotsOfEquityRiver)
            {
                PAveragePlayerBluffsWithAlotsOfEquityRiver[item.Key].Sort((x, y) =>
                {
                    if (x.Item1 > y.Item1) return -1;
                    if (x.Item1 < y.Item1) return 1;
                    return 0;
                });
            }
        }
        public static void LoadAllAveragePlayerMadeHandBlockersRiver()
        {
            throw new NotImplementedException();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM AveragePlayerMadeHandBlockersRiver;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        double blockerRatio = reader.GetFieldData<double>(3);
                        double handStrengthInBlocker = reader.GetFieldData<double>(4);
                        int sampleCount = reader.GetFieldData<int>(5);
                        // insert here
                    }
                }
            }
        }
        public static void LoadAllRiverGameStatesFoldStats()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM RiverAllGameStatesFoldStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        bool canRaise = reader.GetFieldData<bool>(3);
                        long sampleCount = reader.GetFieldData<long>(4);
                        PGameStatesStats.Add((Street.River, riverGameState.PID, boardType, canRaise), sampleCount);
                        if (sampleCount > 0)
                        {
                           

                            /*var state_id_key = new AbstractStateID(Street.River, riverGameState.PID);
                            var state_key = new AbstractState(state_id_key, boardType);
                            var action_key = new AbstractAction(PokerAction.Fold, null);

                            if (!PStateTransitions.ContainsKey(state_key))
                                PStateTransitions.Add(state_key, new List<AbstractActionCount>());

                            AbstractActionCount state_count_key;

                            if (PStateTransitions[state_key].Count > 0)
                                state_count_key = new AbstractActionCount(action_key, sampleCount + PStateTransitions[state_key].Last().count);
                            else
                                state_count_key = new AbstractActionCount(action_key, sampleCount);

                            PStateTransitions[state_key].Add(state_count_key);*/
                        }
                    }
                }
            }         
        }
        public static void LoadAllRiverGameStatesOtherStats()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM RiverAllGameStatesOtherStats;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CRiverGameState riverGameState = PDicAllRiverGameStatesByID[reader.GetFieldData<long>(0)];
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(1);
                        long sampleCount = reader.GetFieldData<long>(3);

                        //if (sampleCount > 0)
                            PGameStatesStats.Add((Street.River, riverGameState.PID, boardType, null), sampleCount);
                    }
                }
            }
        }
        public static void LoadAllRiverBoardTypesByGroupType()
        {
            foreach (var boardMetaData in PLstAllRiverBoardTypes)
            {
                BoardMetaDataFlags boardMetaDataPaired = (boardMetaData & BoardMetaDataFlags.Paired) | (boardMetaData & BoardMetaDataFlags.TwoPaired) | (boardMetaData & BoardMetaDataFlags.Trips) | (boardMetaData & BoardMetaDataFlags.FullHouse) | (boardMetaData & BoardMetaDataFlags.Quads);
                BoardMetaDataFlags boardMetaDataStraight = (boardMetaData & BoardMetaDataFlags.StraightDrawPossible) | (boardMetaData & BoardMetaDataFlags.StraightPossible) | (boardMetaData & BoardMetaDataFlags.OneCardStraightPossible) | (boardMetaData & BoardMetaDataFlags.StraightComplete);
                BoardMetaDataFlags boardMetaDataFlush = (boardMetaData & BoardMetaDataFlags.FlushDrawPossible) | (boardMetaData & BoardMetaDataFlags.FlushPossible) | (boardMetaData & BoardMetaDataFlags.OneCardFlushPossible) | (boardMetaData & BoardMetaDataFlags.FlushComplete) | (boardMetaData & BoardMetaDataFlags.StraightFlushComplete);

                if (!PDicAllRiverBoardTypesByGroupType.ContainsKey(boardMetaDataPaired))
                    PDicAllRiverBoardTypesByGroupType.Add(boardMetaDataPaired, new Dictionary<BoardMetaDataFlags, Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>>());

                if (!PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired].ContainsKey(boardMetaDataStraight))
                    PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired].Add(boardMetaDataStraight, new Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>());

                if (!PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired][boardMetaDataStraight].ContainsKey(boardMetaDataFlush))
                    PDicAllRiverBoardTypesByGroupType[boardMetaDataPaired][boardMetaDataStraight].Add(boardMetaDataFlush, boardMetaData);
            }
        }
        #endregion
        #region Other queries
        public static void LoadAllGameStates()
        {
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            string sql = "SELECT * FROM FlopAllGameStates;";

            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long ID = reader.GetFieldData<long>(0);
                        TypesPot typePot = reader.GetFieldData<TypesPot>(1);
                        PokerPosition possiblePosition = reader.GetFieldData<PokerPosition>(2);
                        PokerAction typeAction = PokerActionBDToPokerAction(reader.GetFieldData<PokerActionBD>(3));
                        long? typeBet = reader.GetFieldData<long?>(4);

                        CFlopGameState flopGameState = new CFlopGameState(ID, typePot, possiblePosition, typeAction, typeBet);
                        PDicAllFlopGameStatesByID.Add(ID, flopGameState);
                    }
                }
            }

            var qweasd = GenerateAllPossibleActionsFlop();

            var preflopID = 0;
            foreach (var item in qweasd)
            {
                var key = (item.Key.Item1, item.Key.Item2);

                if (!PPreflopIDs.ContainsKey(key))
                    PPreflopIDs.Add(key, preflopID++);
            }

            PDicAllFlopGameStatesByInfos = qweasd.ToDictionary(x => (PPreflopIDs[(x.Key.Item1, x.Key.Item2)], x.Key.Item3, x.Key.Item4), x => x.Value);

            PDicAllFlopGameStatesByInfosV2 = PDicAllFlopGameStatesByInfos.Select(x => ((x.Key.Item1, x.Key.Item2), (x.Key.Item3, x.Value.PID))).GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToList());



            var tempDicFlopUniqueGameStates = GetUniquesGameStatesFlop(PDicAllFlopGameStatesByInfos);

            sql = "SELECT * FROM TurnAllGameStates;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long ID = reader.GetFieldData<long>(0);
                        long flopGameStateID = reader.GetFieldData<long>(1);
                        PokerAction typeAction = PokerActionBDToPokerAction(reader.GetFieldData<PokerActionBD>(2));
                        long? typeBet = reader.GetFieldData<long?>(3);

                        CTurnGameState turnGameState = new CTurnGameState(ID, PDicAllFlopGameStatesByID[flopGameStateID], typeAction, typeBet);
                        PDicAllTurnGameStatesByID.Add(ID, turnGameState);
                    }
                }
            }

            var qwe = GenerateAllPossibleActionsTurn(tempDicFlopUniqueGameStates.Values.ToList()); // Must load the informations from this function, not from the DB
            var tempDicTurnUniqueGameStates = GetUniquesGameStatesTurn(qwe);

            foreach (var x in qwe)
            {
                PDicAllTurnGameStatesByInfos.Add((x.Key.Item1.PID, x.Key.Item2, x.Key.Item3), x.Value);
            }

            PDicAllTurnGameStatesByInfosV2 = PDicAllTurnGameStatesByInfos.Select(x => ((x.Key.Item1, x.Key.Item2), (x.Key.Item3, x.Value.PID))).GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToList());

            sql = "SELECT * FROM RiverAllGameStates;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long ID = reader.GetFieldData<long>(0);
                        long turnGameStateID = reader.GetFieldData<long>(1);
                        PokerAction typeAction = PokerActionBDToPokerAction(reader.GetFieldData<PokerActionBD>(2));
                        long? typeBet = reader.GetFieldData<long?>(3);

                        CRiverGameState riverGameState = new CRiverGameState(ID, PDicAllTurnGameStatesByID[turnGameStateID], typeAction, typeBet);
                        PDicAllRiverGameStatesByID.Add(ID, riverGameState);
                    }
                }
            }


            var qwe2 = GenerateAllPossibleActionsRiver(tempDicTurnUniqueGameStates.Values.ToList()); // Must load the informations from this function, not from the DB

            PDicAllRiverGameStatesByInfos = new Dictionary<(long, PokerAction, long?), CRiverGameState>();

            foreach (var x in qwe2)
            {
                PDicAllRiverGameStatesByInfos.Add((x.Key.Item1.PID, x.Key.Item2, x.Key.Item3), x.Value);
            }

            PDicAllRiverGameStatesByInfosV2 = PDicAllRiverGameStatesByInfos.Select(x => ((x.Key.Item1, x.Key.Item2), (x.Key.Item3, x.Value.PID))).GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToList());

        }
        public static void LoadAllRangesTotalSamples()
        {
            if (PDicAllFlopGameStatesByID.Count == 0 || PDicAllFlopGameStatesByInfos.Count == 0)
                throw new InvalidOperationException("Must load the database of flop game state ID before");
            else if (PDicAllTurnGameStatesByID.Count == 0 || PDicAllTurnGameStatesByInfos.Count == 0)
                throw new InvalidOperationException("Must load the database of turn game state ID before");
            else if (PDicAllRiverGameStatesByID.Count == 0 || PDicAllRiverGameStatesByInfos.Count == 0)
                throw new InvalidOperationException("Must load the database of river game state ID before");

            #region Local methods
            void LFLoadFlop()
            {
                foreach (CFlopGameState flopGameState in PDicAllFlopGameStatesByID.Values)
                {
                    foreach (BoardMetaDataFlags boardType in PLstAllFlopBoardTypes)
                    {
                        var key = (flopGameState.PID, boardType);                    
                        long totalSampleCount = 0;

                        if (PAveragePlayerBluffsFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsFlop[key].Sum(x => x.Item7);
                        if (PAveragePlayerBluffsWithAlotsOfEquityFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsWithAlotsOfEquityFlop[key].Sum(x => x.Item3);
                        if (PAveragePlayerMadeHandFDFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandFDFlop[key].Sum(x => x.Item3);
                        if (PAveragePlayerMadeHandSDAndFDFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandSDAndFDFlop[key].Item2;
                        if (PAveragePlayerMadeHandSDFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandSDFlop[key].Item2;
                        if (PAveragePlayerValueHandsFlop.ContainsKey(key))
                            totalSampleCount += PAveragePlayerValueHandsFlop[key].Sum(x => x.Item3);

                        PAllRangeSamplesFlop.Add(key, totalSampleCount);
                    }
                }
            }
            void LFLoadTurn()
            {
                foreach (CTurnGameState turnGameState in PDicAllTurnGameStatesByID.Values)
                {
                    foreach (BoardMetaDataFlags boardType in PLstAllTurnBoardTypes)
                    {
                        var key = (turnGameState.PID, boardType);
                        long totalSampleCount = 0;

                        if (PAveragePlayerBluffsTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsTurn[key].Sum(x => x.Item5);
                        if (PAveragePlayerBluffsWithAlotsOfEquityTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsWithAlotsOfEquityTurn[key].Sum(x => x.Item3);
                        if (PAveragePlayerMadeHandFDTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandFDTurn[key].Sum(x => x.Item3);
                        if (PAveragePlayerMadeHandSDAndFDTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandSDAndFDTurn[key].Item2;
                        if (PAveragePlayerMadeHandSDTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerMadeHandSDTurn[key].Item2;
                        if (PAveragePlayerValueHandsTurn.ContainsKey(key))
                            totalSampleCount += PAveragePlayerValueHandsTurn[key].Sum(x => x.Item3);

                        PAllRangeSamplesTurn.Add(key, totalSampleCount);
                    }
                }
            }
            void LFLoadRiver()
            {
                foreach (CRiverGameState riverGameState in PDicAllRiverGameStatesByID.Values)
                {
                    foreach (BoardMetaDataFlags boardType in PLstAllRiverBoardTypes)
                    {
                        var key = (riverGameState.PID, boardType);
                        long totalSampleCount = 0;

                        if (PAveragePlayerBluffsRiver.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsRiver[key].Values.Sum(x => x.Sum(y => y.Item3));
                        if (PAveragePlayerBluffsWithAlotsOfEquityRiver.ContainsKey(key))
                            totalSampleCount += PAveragePlayerBluffsWithAlotsOfEquityRiver[key].Sum(x => x.Item3);
                        if (PAveragePlayerValueHandsRiver.ContainsKey(key))
                            totalSampleCount += PAveragePlayerValueHandsRiver[key].Values.Sum(x => x.Item2);

                        PAllRangeSamplesRiver.Add(key, totalSampleCount);
                    }
                }
            }
            #endregion

            LFLoadFlop();
            LFLoadTurn();
            LFLoadRiver();
        }

        public static void LoadAllStatesTransitions()
        {
            
            //var PDicAllFlopGameTransitions = PDicAllFlopGameStatesByInfosV2.ToDictionary(x => (x.Key.Item1, x.Key.Item2), x => x.Value.Select(y => (y.Item1, PAllRangeSamplesFlop.Where(z => z.Key.Item1 == y.Item2).Select(z => (z.Key.Item2, z.Value)).ToList())).ToList());





            //var PDicAllTurnGameTransitions = PDicAllTurnGameStatesByInfosV2.ToDictionary(x => x.Key, x => x.Value.Select(y => (y.Item1, PAllRangeSamplesTurn.Where(z => (z.Key.Item1 == y.Item2) && (z.Value != 0)).Select(z => (z.Key.Item2, z.Value)).ToList())).ToList());



            //var PDicAllRiverGameTransitions = PDicAllRiverGameStatesByInfosV2.ToDictionary(x => x.Key, x => x.Value.Select(y => (y.Item1, PAllRangeSamplesRiver.Where(z => (z.Key.Item1 == y.Item2) && (z.Value != 0)).Select(z => (z.Key.Item2, z.Value)).ToList())).ToList());


        }

        public static void LoadAllHandsByCard()
        {
            foreach(var pocketMask in PocketHands.AllHands)
            {
                foreach (var cardInHand in Hand.Hands(0, ~pocketMask, 1))
                {
                    if (!PAllHandsByCard.ContainsKey(cardInHand))
                        PAllHandsByCard.Add(cardInHand, new List<ulong>(51));

                    PAllHandsByCard[cardInHand].Add(pocketMask);
                }
            }
        }
        #endregion
        #endregion
        private static SQLiteConnection GetConnection(string _dbPath = @"C:\\AveragePlayerDB\\PlayerInfosDB.amigo")
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;mode=ro;");

            return dbConnection;
        }

        #region Create table if not exist queries
        #region Preflop
        public static void CreateAveragePlayerPreflopRangesTableIfNotExist()
        {
            string sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerPreflopRanges (TypePot INTEGER(1) NOT NULL,Position INTEGER(1) NOT NULL,PocketMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TypePot, Position, PocketMask));";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }
        }
        #endregion
        #region Flop queries        
        private static Dictionary<(long, PokerAction, long?), CFlopGameState> GetUniquesGameStatesFlop(Dictionary<(long, PokerAction, long?), CFlopGameState> _dicFlopActions)
        {
            var dicFilteredActions = new Dictionary<(long, PokerAction, long?), CFlopGameState>(_dicFlopActions.Count);

            foreach (var item in _dicFlopActions)
            {
                if (!dicFilteredActions.ContainsValue(item.Value))
                    dicFilteredActions.Add(item.Key, item.Value);
            }

            return dicFilteredActions;
        }               
        private static Dictionary<(TypesPot, PokerPosition, PokerAction, long?), CFlopGameState> GenerateAllPossibleActionsFlop()
        {
            var dicActions = new Dictionary<(TypesPot, PokerPosition, PokerAction, long?), CFlopGameState>(150);
            long currentID = 1;

            void GenerateActionsForTwoBetPot()
            {
                #region BTN

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Check, null));

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent100), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent100));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent133), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent133));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                #region Bet - Action not supported
                // Bet all in
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllInShort));

                #region Call - Actions not supported
                // Call 133%
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                // Call all in
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                #region Raise - Actions not supported
                // Raise AnySizingExceptAllIn
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                #region CallVsRaise - Actions not supported
                // CallVsRaise AnySizingExceptAllIn
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.ReRaise, null), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.ReRaise, null));

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                #endregion

                #region BB
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Check, null));

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                #region Bet - Actions not supported
                // Bet 133%
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                // Bet all in
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent100), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent100));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent133), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent133));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllInShort));

                #region Call - Action not supported
                // Call all in
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                #region Raise - Actions not supported
                // Raise AnySizingExceptAllIn
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                #region CallVsRaise - Actions not supported
                // CallVsRaise AnySizingExceptAllIn
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.ReRaise, null), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.ReRaise, null));

                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                dicActions.Add((TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CFlopGameState(currentID++, TypesPot.TwoBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                #endregion
            }
            void GenerateActionsForThreeBetPot()
            {
                #region BTN
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Check, null));

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn));

                #region Bet - Actions not supported
                // Bet 100%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                // Bet 133%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn));

                #region Call - Actions not supported
                // Call 100%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                // Call 133%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                #region Raise - Actions not supported
                // Raise AnySizingExceptAllIn
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                #region CallVsRaise - Actions not supported
                // CallVsRaise AnySizingExceptAllIn
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                #region ReRaise - Action not supported
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.ReRaise, null), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                #region CallVsReRaise - Actions not supported
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion
                #endregion

                #region BB
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Check, null));

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn));

                #region Bet - Actions not supported
                // Bet 100%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                // Bet 133%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn));

                #region Call - Actions not supported
                // Call 100%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                // Call 133%
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                #region Raise - Actions not supported
                // Raise AnySizingExceptAllIn
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                #region CallVsRaise - Actions not supported
                // CallVsRaise AnySizingExceptAllIn
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                #region ReRaise - Action not supported
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.ReRaise, null), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion

                #region CallVsReRaise - Actions not supported
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                dicActions.Add((TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(TypesPot.ThreeBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                #endregion
                #endregion
            }
            void GenerateActionsForFourBetPot()
            {
                #region BTN
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Check, null));

                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn));

                #region Bet - Actions not supported
                // Bet 72%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // Bet 100%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // Bet 133%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // All in short
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn));

                #region Call - Actions not supported
                // Call 72%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // Call 100%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // Call 133%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // All in short
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                #endregion

                // Call vs raise must be here, since the action that are not supported from raise use these actions
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                #region Raise - Actions not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region CallVsRaise - Actions not supported
                // CallVsRaise AnySizeExceptAllIn
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region ReRaise - Action not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.ReRaise, null), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region CallVsReRaise - Actions not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(TypesPot.FourBet, PokerPosition.BTN, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion
                #endregion

                #region BB
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Check, null), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Check, null));

                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn));

                #region Bet - Actions not supported
                // Bet 72%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // Bet 100%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // Bet 133%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                // All in short
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent33));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent50));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn));

                #region Call - Actions not supported
                // Call 72%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // Call 100%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // Call 133%
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                // All in short
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                #endregion

                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max));
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CFlopGameState(currentID++, TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                #region Raise - Action not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region CallVsRaise - Actions not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region ReRaise - Action not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.ReRaise, null), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion

                #region CallVsReRaise - Actions not supported
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                dicActions.Add((TypesPot.FourBet, PokerPosition.BB, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(TypesPot.FourBet, PokerPosition.BB, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                #endregion
                #endregion
            }



            GenerateActionsForTwoBetPot();
            GenerateActionsForThreeBetPot();
            GenerateActionsForFourBetPot();
            currentID = 0;
            Dictionary<(TypesPot, PokerPosition, PokerAction, long?), CFlopGameState> GenerateActions()
            {
                var qwe = new Dictionary<(TypesPot, PokerPosition, PokerAction, long?), CFlopGameState>();
                foreach (var typePot in GetValues<TypesPot>())
                {
                    if ((typePot == TypesPot.TwoBet) || (typePot == TypesPot.ThreeBet) || (typePot == TypesPot.FourBet))
                    {
                        foreach (var pokerPosition in GetValues<PokerPosition>())
                        {
                            if (pokerPosition != PokerPosition.Unknown)
                            {
                                foreach (var pokerAction in GetValues<PokerAction>())
                                {
                                    if ((pokerAction & (PokerAction.None | PokerAction.Fold)) == 0)
                                    {
                                        if (pokerAction == PokerAction.None)
                                            continue;
                                        foreach (var sizing in CAction.GetListFromPokerAction(typePot, pokerAction))
                                        {
                                            (TypesPot, PokerPosition, PokerAction, long?) key = (typePot, pokerPosition, pokerAction, sizing.Item1);
                                            var value = new CFlopGameState(currentID++, typePot, pokerPosition, pokerAction, sizing.Item1);
                                            qwe.Add(key, value);
                                        }
                                    }
                                }
                                //dicActions.Add((typePot, pokerPosition, PokerAction.Fold, null), new CFlopGameState(currentID++, typePot, pokerPosition, PokerAction.Fold, null));
                            }
                        }
                    }
                }
                return qwe;
            }

            var qwe2 = GenerateActions();

            //Lol ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //dicActions = qwe2;

            return dicActions;
        }                        
        public static void CreateAllAveragePlayerStatsFlopTableIfNotExist()
        {
            string sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,BDFD INTEGER(1) NOT NULL,BDSD INTEGER(1) NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType, BDFD, BDSD, SD, FD, IndexHighestCardExcludingBoard),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
            string sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType, NbOuts),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
            string sqlAveragePlayerMadeHandSDAndFDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDAndFDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
            string sqlAveragePlayerMadeHandFDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandFDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,IndexHighestCardExcludingBoardOfFlushCard INTEGER(1),UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType, IndexHighestCardExcludingBoardOfFlushCard),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
            string sqlAveragePlayerMadeHandSDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
            string sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(FlopGameStateID, BoardType, HandStrength),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDAndFDFlop, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandFDFlop, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDFlop, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }

            #region Create debug database
            using (SQLiteConnection sqlConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo"))
            {
                sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,BDFD INTEGER(1) NOT NULL,BDSD INTEGER(1) NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,HandMask INTEGER,BoardMask INTEGER,HandDescription TEXT,BoardDescription TEXT,HandHistory TEXT,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
                sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
                sqlAveragePlayerMadeHandSDAndFDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDAndFDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
                sqlAveragePlayerMadeHandFDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandFDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,IndexHighestCardExcludingBoardOfFlushCard INTEGER(1),HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
                sqlAveragePlayerMadeHandSDFlop = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";
                sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsFlop (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));";

                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                    sqlConnection.Open();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDAndFDFlop, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandFDFlop, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDFlop, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, sqlConnection))
                        command.ExecuteNonQuery();
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            #endregion
        }        
        #endregion
        #region Turn        
        private static Dictionary<(CFlopGameState, PokerAction, long?), CTurnGameState> GetUniquesGameStatesTurn(Dictionary<(CFlopGameState, PokerAction, long?), CTurnGameState> _dicTurnActions)
        {
            var dicFilteredActions = new Dictionary<(CFlopGameState, PokerAction, long?), CTurnGameState>(_dicTurnActions.Count);

            foreach (var item in _dicTurnActions)
            {
                if (!dicFilteredActions.ContainsValue(item.Value))
                    dicFilteredActions.Add(item.Key, item.Value);
            }

            return dicFilteredActions;
        }
        private static Dictionary<(CFlopGameState, PokerAction, long?), CTurnGameState> GenerateAllPossibleActionsTurn(List<CFlopGameState> _lstFlopActions)
        {
            var dicActions = new Dictionary<(CFlopGameState, PokerAction, long?), CTurnGameState>(150);
            long currentID = 1;

            foreach (var flopGameState in _lstFlopActions)
            {
                void GenerateActionsForTwoBetPot()
                {
                    if (flopGameState.PTypePot != TypesPot.TwoBet)
                        throw new Exception("Invalid type pot");

                    #region Two bet
                    switch (flopGameState.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    #region Bet - Action not supported
                                    // Bet all in
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    // Call all in
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    // Raise all in
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    // CallVsRaise all in
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.ReRaise, null));

                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)flopGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }

                                    else
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)flopGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                        #region Call - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                        #region Call - Actions not supported
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    if ((BetSizePossible)flopGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                        #region Raise - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                        #region Raise - Actions not supported
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        #endregion
                                    }

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)flopGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region CallVsRaise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.ReRaise:
                                    #region Reraise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsReRaise:
                                    #region Call vs ReRaise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.ReRaise, null));

                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion                                    

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.ReRaise, null));

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)flopGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Actions not supported
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.ReRaise:
                                    #region Reraise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsReRaise:
                                    #region CallVsReRaise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)BetSizePossible.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action not supported");
                            }
                            break;
                    }
                    #endregion                    
                }
                void GenerateActionsForThreeBetPot()
                {
                    if (flopGameState.PTypePot != TypesPot.ThreeBet)
                        throw new Exception("Invalid type pot");

                    #region Three bet
                    switch (flopGameState.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));
                                    #endregion
                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Raise, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                    }
                    #endregion
                }
                void GenerateActionsForFourBetPot()
                {
                    if (flopGameState.PTypePot != TypesPot.FourBet)
                        throw new Exception("Invalid type pot");

                    #region Four bet
                    switch (flopGameState.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (flopGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((flopGameState, PokerAction.Check, null), new CTurnGameState(currentID++, flopGameState, PokerAction.Check, null));

                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CTurnGameState(currentID++, flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Call - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((flopGameState, PokerAction.ReRaise, null), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((flopGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(flopGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Fold:
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                    }
                    #endregion
                }
                // If it's not an all in short or all in
                if ((flopGameState.PTypeBet != 135) && (flopGameState.PTypeBet != 136))
                {
                    switch (flopGameState.PTypePot)
                    {
                        case TypesPot.TwoBet:
                            GenerateActionsForTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            GenerateActionsForThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            GenerateActionsForFourBetPot();
                            break;
                        default:
                            throw new Exception("Invalid type pot, it should be converted to a supported type pot before!");
                    }
                }
            }

            return dicActions;
        }
        public static void CreateTurnAllGameStatesTableIfNotExist(List<CFlopGameState> _lstFlopGameStates, bool _createOnDebugDatabase)
        {
            var turnGameStates = GetUniquesGameStatesTurn(GenerateAllPossibleActionsTurn(_lstFlopGameStates));
            #region Local methods
            void LFCreateData()
            {
                void InsertTurnGameState(CTurnGameState _turnGameState)
                {
                    if (FFConnection.State == System.Data.ConnectionState.Closed)
                        FFConnection.Open();

                    string sqlLocal = "INSERT INTO TurnAllGameStates (FlopGameStateID, TypeAction, TypeBet) values (?, ?, ?);";
                    using (SQLiteCommand command = new SQLiteCommand(sqlLocal, FFConnection))
                    {
                        command.Parameters.Add(new SQLiteParameter("FlopGameStateID", _turnGameState.PGameStateID.PID));
                        command.Parameters.Add(new SQLiteParameter("TypeAction", _turnGameState.PTypeAction));

                        switch (_turnGameState.PTypeAction)
                        {
                            case CAction.PokerAction.Call:
                            case CAction.PokerAction.Bet:
                            case CAction.PokerAction.Raise:
                            case CAction.PokerAction.CallVsRaise:
                            case CAction.PokerAction.CallVsReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", _turnGameState.PTypeBet));
                                break;
                            case CAction.PokerAction.Check:
                            case CAction.PokerAction.ReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", DBNull.Value));
                                break;
                            default:
                                throw new NotImplementedException("The action None and the action Fold is not allowed");
                        }

                        command.ExecuteNonQuery();
                    }
                }
                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                {
                    foreach (var turnGameState in turnGameStates.Values)
                        InsertTurnGameState(turnGameState);

                    transaction.Commit();
                }
            }
            #endregion

            string sql = "CREATE TABLE IF NOT EXISTS TurnAllGameStates (ID INTEGER PRIMARY KEY ASC,FlopGameStateID INTEGER, TypeAction INTEGER NOT NULL,TypeBet INTEGER, FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));SELECT count(*) FROM TurnAllGameStates;";

            SQLiteConnection oldConnection = FFConnection;

            if (_createOnDebugDatabase)
                FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if ((long)reader[0] == 0)
                                LFCreateData();
                        }
                        else
                            throw new Exception("Unable to read TurnAllGameStates table! Was the table created?");
                    }
                }
            }
            finally
            {
                FFConnection.Close();
                FFConnection = oldConnection;
            }

            CreateRiverAllGameStatesTableIfNotExist(turnGameStates.Values.ToList(), _createOnDebugDatabase);
        }
        public static void CreateAllAveragePlayerStatsTurnTableIfNotExist()
        {
            string sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
            string sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, NbOuts),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
            string sqlAveragePlayerMadeHandSDAndFDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDAndFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
            string sqlAveragePlayerMadeHandFDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,IndexHighestCardExcludingBoardOfFlushCard INTEGER(1),UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, IndexHighestCardExcludingBoardOfFlushCard),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
            string sqlAveragePlayerMadeHandSDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
            string sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(TurnGameStateID, BoardType, HandStrength),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDAndFDTurn, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandFDTurn, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDTurn, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }

            #region Create debug database
            using (SQLiteConnection sqlConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo"))
            {
                sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,HandMask INTEGER,BoardMask INTEGER,HandDescription TEXT,BoardDescription TEXT,HandHistory TEXT,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
                sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
                sqlAveragePlayerMadeHandSDAndFDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDAndFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
                sqlAveragePlayerMadeHandFDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandFDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,IndexHighestCardExcludingBoardOfFlushCard INTEGER(1),HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
                sqlAveragePlayerMadeHandSDTurn = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandSDTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";
                sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsTurn (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));";

                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                    sqlConnection.Open();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDAndFDTurn, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandFDTurn, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandSDTurn, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, sqlConnection))
                        command.ExecuteNonQuery();
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            #endregion
        }
        #endregion
        #region River
        private static Dictionary<(CTurnGameState, PokerAction, long?), CRiverGameState> GetUniquesGameStatesRiver(Dictionary<(CTurnGameState, PokerAction, long?), CRiverGameState> _dicRiverActions)
        {
            var dicFilteredActions = new Dictionary<(CTurnGameState, PokerAction, long?), CRiverGameState>(_dicRiverActions.Count);

            foreach (var item in _dicRiverActions)
            {
                if (!dicFilteredActions.ContainsValue(item.Value))
                    dicFilteredActions.Add(item.Key, item.Value);
            }

            return dicFilteredActions;
        }
        private static Dictionary<(CTurnGameState, PokerAction, long?), CRiverGameState> GenerateAllPossibleActionsRiver(List<CTurnGameState> _lstTurnActions)
        {
            var dicActions = new Dictionary<(CTurnGameState, PokerAction, long?), CRiverGameState>(150);
            long currentID = 1;

            foreach (var turnGameState in _lstTurnActions)
            {
                void GenerateActionsForTwoBetPot()
                {
                    if (turnGameState.PGameStateID.PTypePot != TypesPot.TwoBet)
                        throw new Exception("Invalid type pot");

                    #region Two bet
                    switch (turnGameState.PGameStateID.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    #region Bet - Action not supported
                                    // Bet all in
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    // Call all in
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    // Raise all in
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    // CallVsRaise all in
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.ReRaise, null));

                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)turnGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }

                                    else
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)turnGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                        #region Call - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                        #region Call - Actions not supported
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    if ((BetSizePossible)turnGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                        #region Raise - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                        #region Raise - Actions not supported
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                        #endregion
                                    }

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)turnGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region CallVsRaise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.ReRaise:
                                    #region Reraise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsReRaise:
                                    #region Call vs raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.ReRaise, null));

                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion                                    

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.ReRaise, null));

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));

                                    if ((BetSizePossible)turnGameState.PTypeBet != BetSizePossible.Percent133)
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Action not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                        #endregion
                                    }
                                    else
                                    {
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                        #region Bet - Actions not supported
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                        #endregion
                                    }

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn));
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.ReRaise:
                                    #region Reraise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsReRaise:
                                    #region CallVsReRaise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)BetSizePossible.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.FourPoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleTwoBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn));
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleTwoBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action not supported");
                            }
                            break;
                    }
                    #endregion                    
                }
                void GenerateActionsForThreeBetPot()
                {
                    if (turnGameState.PGameStateID.PTypePot != TypesPot.ThreeBet)
                        throw new Exception("Invalid type pot");

                    #region Three bet
                    switch (turnGameState.PGameStateID.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.ThreePoint5Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleThreeBetPot.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Raise, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleThreeBetPot.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                    }
                    #endregion
                }
                void GenerateActionsForFourBetPot()
                {
                    if (turnGameState.PGameStateID.PTypePot != TypesPot.FourBet)
                        throw new Exception("Invalid type pot");

                    #region Four bet
                    switch (turnGameState.PGameStateID.PPosition)
                    {
                        case PokerPosition.BTN:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.CallVsRaise:
                                    #region Call vs raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                        case PokerPosition.BB:
                            switch (turnGameState.PTypeAction)
                            {
                                case PokerAction.Check:
                                    #region Check
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Bet:
                                    #region Bet
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Call:
                                    #region Call
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50));
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn));

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn));
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn));

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                case PokerAction.Raise:
                                    #region Raise
                                    dicActions.Add((turnGameState, PokerAction.Check, null), new CRiverGameState(currentID++, turnGameState, PokerAction.Check, null));

                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn), new CRiverGameState(currentID++, turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn));

                                    #region Bet - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Bet, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Call - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent33), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent50), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent72), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent100), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.Percent133), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Call, (long)BetSizePossible.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region Raise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.Raise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.TwoPoint7Max), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsRaise, (long)RaiseSizePossibleFourBetPot.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region ReRaise - Action not supported
                                    dicActions.Add((turnGameState, PokerAction.ReRaise, null), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion

                                    #region CallVsReRaise - Actions not supported
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AnySizingExceptAllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllInShort), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    dicActions.Add((turnGameState, PokerAction.CallVsReRaise, (long)CallSizePossibleVsReRaise.AllIn), dicActions[(turnGameState, PokerAction.Bet, (long)BetSizePossible.AllIn)]);
                                    #endregion
                                    #endregion
                                    break;
                                default:
                                    throw new Exception("Action is not supported");
                            }
                            break;
                    }
                    #endregion
                }
                // If it's not an all in short or all in
                if ((turnGameState.PTypeBet != 135) && (turnGameState.PTypeBet != 136))
                {
                    switch (turnGameState.PGameStateID.PTypePot)
                    {
                        case TypesPot.TwoBet:
                            GenerateActionsForTwoBetPot();
                            break;
                        case TypesPot.ThreeBet:
                            GenerateActionsForThreeBetPot();
                            break;
                        case TypesPot.FourBet:
                            GenerateActionsForFourBetPot();
                            break;
                        default:
                            throw new Exception("Invalid type pot, it should be converted to a supported type pot before!");
                    }
                }
            }

            return dicActions;
        }
        public static void CreateRiverAllGameStatesTableIfNotExist(List<CTurnGameState> _lstTurnGameState, bool _createOnDebugDatabase)
        {
            var riverGameStates = GetUniquesGameStatesRiver(GenerateAllPossibleActionsRiver(_lstTurnGameState));

            void CreateData()
            {
                void InsertRiverGameState(CRiverGameState _riverGameState)
                {
                    if (FFConnection.State == System.Data.ConnectionState.Closed)
                        FFConnection.Open();

                    string sqlLocal = "INSERT INTO RiverAllGameStates (TurnGameStateID, TypeAction, TypeBet) values (?, ?, ?);";
                    using (SQLiteCommand command = new SQLiteCommand(sqlLocal, FFConnection))
                    {
                        command.Parameters.Add(new SQLiteParameter("TurnGameStateID", _riverGameState.PGameStateID.PID));
                        command.Parameters.Add(new SQLiteParameter("TypeAction", _riverGameState.PTypeAction));

                        switch (_riverGameState.PTypeAction)
                        {
                            case CAction.PokerAction.Call:
                            case CAction.PokerAction.Bet:
                            case CAction.PokerAction.Raise:
                            case CAction.PokerAction.CallVsRaise:
                            case CAction.PokerAction.CallVsReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", _riverGameState.PTypeBet));
                                break;
                            case CAction.PokerAction.Check:
                            case CAction.PokerAction.ReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", DBNull.Value));
                                break;
                            default:
                                throw new NotImplementedException("The action None and the action Fold is not allowed");
                        }

                        command.ExecuteNonQuery();
                    }
                }
                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                {
                    foreach (var riverGameState in riverGameStates.Values)
                        InsertRiverGameState(riverGameState);

                    transaction.Commit();
                }
            }

            string sql = "CREATE TABLE IF NOT EXISTS RiverAllGameStates (ID INTEGER PRIMARY KEY ASC,TurnGameStateID INTEGER,TypeAction INTEGER NOT NULL,TypeBet INTEGER, FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));SELECT count(*) FROM RiverAllGameStates;";

            SQLiteConnection oldConnection = FFConnection;

            if (_createOnDebugDatabase)
                FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if ((long)reader[0] == 0)
                                CreateData();
                        }
                        else
                            throw new Exception("Unable to read RiverAllGameStates table! Was the table created?");
                    }
                }
            }
            finally
            {
                FFConnection.Close();
                FFConnection = oldConnection;
            }
        }
        public static void CreateAllAveragePlayerStatsRiverTableIfNotExist()
        {
            string sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(RiverGameStateID, BoardType, SD, FD, IndexHighestCardExcludingBoard),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
            string sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(RiverGameStateID, BoardType, NbOuts),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
            string sqlAveragePlayerMadeHandBlockerRiver = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandBlockersRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,BlockerRatio REAL NOT NULL CHECK(BlockerRatio > 0),HandStrengthInBlockerRange REAL NOT NULL CHECK(HandStrengthInBlockerRange >= 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(RiverGameStateID, BoardType, BlockerRatio, HandStrengthInBlockerRange),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
            string sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,UnifiedCount REAL NOT NULL CHECK(UnifiedCount > 0),SampleCount INTEGER NOT NULL CHECK(SampleCount > 0),PRIMARY KEY(RiverGameStateID, BoardType, HandStrength),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandBlockerRiver, FFConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }

            #region Create debug database
            using (SQLiteConnection sqlConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo"))
            {
                sqlBluffs = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SD INTEGER(1) NOT NULL,FD INTEGER(1) NOT NULL,IndexHighestCardExcludingBoard INTEGER(1) NOT NULL,HandMask INTEGER,BoardMask INTEGER,HandDescription TEXT,BoardDescription TEXT,HandHistory TEXT,FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
                sqlBluffWithLotsOfEquity = "CREATE TABLE IF NOT EXISTS AveragePlayerBluffsWithALotsOfEquityRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,NbOuts INTEGER(1) NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
                sqlAveragePlayerMadeHandBlockerRiver = "CREATE TABLE IF NOT EXISTS AveragePlayerMadeHandBlockersRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,BlockerRatio REAL NOT NULL CHECK(BlockerRatio > 0),HandStrengthInBlockerRange REAL NOT NULL CHECK(HandStrengthInBlockerRange >= 0),HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";
                sqlValueHands = "CREATE TABLE IF NOT EXISTS AveragePlayerValueHandsRiver (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,HandStrength REAL NOT NULL,HandMask INTEGER NOT NULL,BoardMask INTEGER NOT NULL,HandDescription TEXT NOT NULL,BoardDescription NOT NULL,HandHistory TEXT NOT NULL,FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));";

                if (sqlConnection.State == System.Data.ConnectionState.Closed)
                    sqlConnection.Open();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffs, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlBluffWithLotsOfEquity, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlAveragePlayerMadeHandBlockerRiver, sqlConnection))
                        command.ExecuteNonQuery();

                    using (SQLiteCommand command = new SQLiteCommand(sqlValueHands, sqlConnection))
                        command.ExecuteNonQuery();
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            #endregion
        }
        #endregion
        #region Other
        public static void CreateAllGameStatesTableIfNotExist(bool _createOnDebugDatabase)
        {
            throw new NotImplementedException(); //broke this. will fix maybe one day
            /*var flopGameStates = GetUniquesGameStatesFlop(GenerateAllPossibleActionsFlop());
            void CreateData()
            {
                void InsertFlopGameState(CFlopGameState _flopGameState)
                {
                    if (FFConnection.State == System.Data.ConnectionState.Closed)
                        FFConnection.Open();

                    string sqlLocal = "INSERT INTO FlopAllGameStates (TypePot, Position, TypeAction, TypeBet) values (?, ?, ?, ?);";
                    using (SQLiteCommand command = new SQLiteCommand(sqlLocal, FFConnection))
                    {
                        command.Parameters.Add(new SQLiteParameter("TypePot", _flopGameState.PTypePot));
                        command.Parameters.Add(new SQLiteParameter("Position", _flopGameState.PPosition));
                        command.Parameters.Add(new SQLiteParameter("TypeAction", _flopGameState.PTypeAction));

                        switch (_flopGameState.PTypeAction)
                        {
                            case CAction.PokerAction.Call:
                            case CAction.PokerAction.Bet:
                            case CAction.PokerAction.Raise:
                            case CAction.PokerAction.CallVsRaise:
                            case CAction.PokerAction.CallVsReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", _flopGameState.PTypeBet));
                                break;
                            case CAction.PokerAction.Check:
                            case CAction.PokerAction.ReRaise:
                                command.Parameters.Add(new SQLiteParameter("TypeBet", DBNull.Value));
                                break;
                            default:
                                throw new NotImplementedException("The action None and the action Fold is not allowed");
                        }

                        command.ExecuteNonQuery();
                    }
                }
                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                {
                    foreach (var flopGameState in flopGameStates.Values)
                        InsertFlopGameState(flopGameState);

                    transaction.Commit();
                }
            }

            string sql = "CREATE TABLE IF NOT EXISTS FlopAllGameStates (ID INTEGER PRIMARY KEY ASC,TypePot INTEGER NOT NULL,Position INTEGER NOT NULL,TypeAction INTEGER NOT NULL,TypeBet INTEGER);SELECT count(*) FROM FlopAllGameStates;";

            SQLiteConnection oldConnection = FFConnection;

            if (_createOnDebugDatabase)
                FFConnection = GetConnection(@"C:\\AveragePlayerDB\\PlayersInfosDebugDB.amigo");

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if ((long)reader[0] == 0)
                                CreateData();
                        }
                        else
                            throw new Exception("Unable to read FlopAllGameStates table! Was the table created?");
                    }
                }
            }
            finally
            {
                FFConnection.Close();
                FFConnection = oldConnection;
            }

            CreateTurnAllGameStatesTableIfNotExist(flopGameStates.Values.ToList(), _createOnDebugDatabase);
        }
        public static void CreateAllGameStatesFoldStatsTableIfNotExist()
        {
            if (PDicAllFlopGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the flop game states from the database before calling this function");
            else if (PDicAllTurnGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the turn game states from the database before calling this function");
            else if (PDicAllRiverGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the river game states from the database before calling this function");
            else if (PLstAllFlopBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all flop board types from the database before calling this function");
            else if (PLstAllTurnBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all turn board types from the database before calling this function");
            else if (PLstAllRiverBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all river board types from the database before calling this function");

            string sqlFlop = "CREATE TABLE IF NOT EXISTS FlopAllGameStatesFoldStats (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,CanRaise INTEGER(1) NOT NULL CHECK(CanRaise IN (0, 1)),SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(FlopGameStateID, BoardType, CanRaise),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));SELECT count(*) FROM FlopAllGameStatesFoldStats;";
            string sqlTurn = "CREATE TABLE IF NOT EXISTS TurnAllGameStatesFoldStats (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,CanRaise INTEGER(1) NOT NULL CHECK(CanRaise IN (0, 1)),SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(TurnGameStateID, BoardType, CanRaise),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));SELECT count(*) FROM TurnAllGameStatesFoldStats;";
            string sqlRiver = "CREATE TABLE IF NOT EXISTS RiverAllGameStatesFoldStats (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,CanRaise INTEGER(1) NOT NULL CHECK(CanRaise IN (0, 1)),SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(RiverGameStateID, BoardType, CanRaise),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));SELECT count(*) FROM RiverAllGameStatesFoldStats;";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlFlop, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateFlopData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstFlopFoldStats = new ConcurrentBag<CFlopFoldStats>();

                                    foreach (var flopGameStateInfos in PDicAllFlopGameStatesByInfos.Keys)
                                    {
                                        switch (flopGameStateInfos.Item2)
                                        {
                                            case PokerAction.Bet:
                                            case PokerAction.Raise:
                                            case PokerAction.ReRaise:
                                                var flopGameState = PDicAllFlopGameStatesByInfos[flopGameStateInfos];

                                                foreach (var boardType in PLstAllFlopBoardTypes)
                                                {
                                                    lstFlopFoldStats.Add(new CFlopFoldStats(flopGameState, (ushort)boardType, 0, false, 0));

                                                    if (flopGameStateInfos.Item3 != null && flopGameStateInfos.Item3 != 135 && flopGameStateInfos.Item3 != 136)
                                                        lstFlopFoldStats.Add(new CFlopFoldStats(flopGameState, (ushort)boardType, 0, true, 0));
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    InsertFlopFoldStats(lstFlopFoldStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateFlopData();
                        }
                        else
                            throw new Exception("Unable to read FlopAllGameStates table! Was the table created?");
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand(sqlTurn, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateTurnData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstTurnFoldStats = new ConcurrentBag<CTurnFoldStats>();
                                                                        
                                    foreach (var turnGameStateInfos in PDicAllTurnGameStatesByInfos.Keys)
                                    {
                                        switch (turnGameStateInfos.Item2)
                                        {
                                            case PokerAction.Bet:
                                            case PokerAction.Raise:
                                            case PokerAction.ReRaise:
                                                var turnGameState = PDicAllTurnGameStatesByInfos[turnGameStateInfos];

                                                foreach (var boardType in PLstAllTurnBoardTypes)
                                                {
                                                    lstTurnFoldStats.Add(new CTurnFoldStats(turnGameState, (ushort)boardType, 0, false, 0));

                                                    if (turnGameStateInfos.Item3 != null && turnGameStateInfos.Item3 != 135 && turnGameStateInfos.Item3 != 136)
                                                        lstTurnFoldStats.Add(new CTurnFoldStats(turnGameState, (ushort)boardType, 0, true, 0));
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    InsertTurnFoldStats(lstTurnFoldStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateTurnData();
                        }
                        else
                            throw new Exception("Unable to read TurnAllGameStates table! Was the table created?");
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand(sqlRiver, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateRiverData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstRiverFoldStats = new ConcurrentBag<CRiverFoldStats>();

                                    foreach (var riverGameStateInfos in PDicAllRiverGameStatesByInfos.Keys)
                                    {
                                        switch (riverGameStateInfos.Item2)
                                        {
                                            case PokerAction.Bet:
                                            case PokerAction.Raise:
                                            case PokerAction.ReRaise:
                                                var riverGameState = PDicAllRiverGameStatesByInfos[riverGameStateInfos];

                                                foreach (var boardType in PLstAllRiverBoardTypes)
                                                {
                                                    lstRiverFoldStats.Add(new CRiverFoldStats(riverGameState, (ushort)boardType, 0, false, 0));

                                                    if (riverGameStateInfos.Item3 != null && riverGameStateInfos.Item3 != 135 && riverGameStateInfos.Item3 != 136)
                                                        lstRiverFoldStats.Add(new CRiverFoldStats(riverGameState, (ushort)boardType, 0, true, 0));
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    InsertRiverFoldStats(lstRiverFoldStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateRiverData();
                        }
                        else
                            throw new Exception("Unable to read RiverAllGameStates table! Was the table created?");
                    }
                }
            }
            finally
            {
                FFConnection.Close();
            }         */   
        }
        public static void CreateAllGameStatesOtherStatsTableIfNotExist()
        {
            if (PDicAllFlopGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the flop game states from the database before calling this function");
            else if (PDicAllTurnGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the turn game states from the database before calling this function");
            else if (PDicAllRiverGameStatesByID.Count == 0)
                throw new InvalidOperationException("Must load the river game states from the database before calling this function");
            else if (PLstAllFlopBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all flop board types from the database before calling this function");
            else if (PLstAllTurnBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all turn board types from the database before calling this function");
            else if (PLstAllRiverBoardTypes.Count == 0)
                throw new InvalidOperationException("Must load all river board types from the database before calling this function");

            string sqlFlop = "CREATE TABLE IF NOT EXISTS FlopAllGameStatesOtherStats (FlopGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(FlopGameStateID, BoardType),FOREIGN KEY(FlopGameStateID) REFERENCES FlopAllGameStates(ID));SELECT count(*) FROM FlopAllGameStatesOtherStats;";
            string sqlTurn = "CREATE TABLE IF NOT EXISTS TurnAllGameStatesOtherStats (TurnGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(TurnGameStateID, BoardType),FOREIGN KEY(TurnGameStateID) REFERENCES TurnAllGameStates(ID));SELECT count(*) FROM TurnAllGameStatesOtherStats;";
            string sqlRiver = "CREATE TABLE IF NOT EXISTS RiverAllGameStatesOtherStats (RiverGameStateID INTEGER NOT NULL,BoardType INTEGER(2) NOT NULL,BoardHeat REAL NOT NULL,SampleCount INTEGER NOT NULL CHECK(SampleCount > -2),PRIMARY KEY(RiverGameStateID, BoardType),FOREIGN KEY(RiverGameStateID) REFERENCES RiverAllGameStates(ID));SELECT count(*) FROM RiverAllGameStatesOtherStats;";

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sqlFlop, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateFlopData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstFlopFoldStats = new ConcurrentBag<CFlopOtherStats>();

                                    foreach (var flopGameState in PDicAllFlopGameStatesByID.Values)
                                    {
                                        foreach (var boardType in PLstAllFlopBoardTypes)                                                
                                            lstFlopFoldStats.Add(new CFlopOtherStats(flopGameState, (ushort)boardType, 0, 0));                                        
                                    }

                                    InsertFlopOtherStats(lstFlopFoldStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateFlopData();
                        }
                        else
                            throw new Exception("Unable to read FlopAllGameStates table! Was the table created?");
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand(sqlTurn, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateTurnData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstTurnOtherStats = new ConcurrentBag<CTurnOtherStats>();

                                    foreach (var turnGameState in PDicAllTurnGameStatesByID.Values)
                                    {
                                        foreach (var boardType in PLstAllTurnBoardTypes)
                                            lstTurnOtherStats.Add(new CTurnOtherStats(turnGameState, (ushort)boardType, 0, 0));                                                                                
                                    }

                                    InsertTurnOtherStats(lstTurnOtherStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateTurnData();
                        }
                        else
                            throw new Exception("Unable to read TurnAllGameStates table! Was the table created?");
                    }
                }

                using (SQLiteCommand command = new SQLiteCommand(sqlRiver, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            void CreateRiverData()
                            {
                                using (SQLiteTransaction transaction = FFConnection.BeginTransaction())
                                {
                                    var lstRiverOtherStats = new ConcurrentBag<CRiverOtherStats>();

                                    foreach (var riverGameState in PDicAllRiverGameStatesByID.Values)
                                    {
                                        foreach (var boardType in PLstAllRiverBoardTypes)                                        
                                            lstRiverOtherStats.Add(new CRiverOtherStats(riverGameState, (ushort)boardType, 0, 0));                                        
                                    }

                                    InsertRiverOtherStats(lstRiverOtherStats);
                                    transaction.Commit();
                                }
                            }

                            if ((long)reader[0] == 0)
                                CreateRiverData();
                        }
                        else
                            throw new Exception("Unable to read RiverAllGameStates table! Was the table created?");
                    }
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }
        #endregion
        #endregion

    }
}