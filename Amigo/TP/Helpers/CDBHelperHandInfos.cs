using HoldemHand;
using Shared.Helpers;
using Shared.Models.Database;
using Shared.Poker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static HoldemHand.Hand;
using static Shared.Models.Database.CBoardModel;
using static Shared.Poker.Models.CTableInfos;

namespace Amigo.Helpers
{
    public static class CDBHelperHandInfos
    {
        private const int CC_ALL_BOARDS_COUNT = 2891785;

        public enum BoardMetaDataFlagsPriority { PairedTypeHandGroup, StraightTypeHandGroup, FlushTypeHandGroup };

        public static readonly Dictionary<BoardMetaDataFlags, BoardMetaDataFlags> PDicBoardMetaDataFlagsDown = new Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>() { { BoardMetaDataFlags.Quads, BoardMetaDataFlags.FullHouse },
                                                                                                                                                                          { BoardMetaDataFlags.FullHouse, BoardMetaDataFlags.Trips },
                                                                                                                                                                          { BoardMetaDataFlags.Trips, BoardMetaDataFlags.TwoPaired },
                                                                                                                                                                          { BoardMetaDataFlags.TwoPaired, BoardMetaDataFlags.Paired },
                                                                                                                                                                          { BoardMetaDataFlags.Paired, BoardMetaDataFlags.None },

                                                                                                                                                                          { BoardMetaDataFlags.StraightComplete, BoardMetaDataFlags.OneCardStraightPossible },
                                                                                                                                                                          { BoardMetaDataFlags.OneCardStraightPossible, BoardMetaDataFlags.StraightPossible },
                                                                                                                                                                          { BoardMetaDataFlags.StraightPossible, BoardMetaDataFlags.StraightDrawPossible },
                                                                                                                                                                          { BoardMetaDataFlags.StraightDrawPossible, BoardMetaDataFlags.None },

                                                                                                                                                                          { BoardMetaDataFlags.StraightFlushComplete, BoardMetaDataFlags.FlushComplete },
                                                                                                                                                                          { BoardMetaDataFlags.FlushComplete, BoardMetaDataFlags.OneCardFlushPossible },
                                                                                                                                                                          { BoardMetaDataFlags.OneCardFlushPossible, BoardMetaDataFlags.FlushPossible },
                                                                                                                                                                          { BoardMetaDataFlags.FlushPossible, BoardMetaDataFlags.FlushDrawPossible },
                                                                                                                                                                          { BoardMetaDataFlags.FlushDrawPossible, BoardMetaDataFlags.None },

                                                                                                                                                                          { BoardMetaDataFlags.None, BoardMetaDataFlags.None } };
        public static readonly Dictionary<BoardMetaDataFlags, BoardMetaDataFlags> PDicBoardMetaDataFlagsUp = new Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>()   { { BoardMetaDataFlags.FullHouse, BoardMetaDataFlags.Quads  },
                                                                                                                                                                          { BoardMetaDataFlags.Trips, BoardMetaDataFlags.FullHouse },
                                                                                                                                                                          { BoardMetaDataFlags.TwoPaired, BoardMetaDataFlags.Trips },
                                                                                                                                                                          { BoardMetaDataFlags.Paired, BoardMetaDataFlags.TwoPaired },

                                                                                                                                                                          { BoardMetaDataFlags.StraightComplete, BoardMetaDataFlags.StraightComplete },
                                                                                                                                                                          { BoardMetaDataFlags.OneCardStraightPossible, BoardMetaDataFlags.StraightComplete },
                                                                                                                                                                          { BoardMetaDataFlags.StraightPossible, BoardMetaDataFlags.OneCardStraightPossible },
                                                                                                                                                                          { BoardMetaDataFlags.StraightDrawPossible, BoardMetaDataFlags.StraightPossible },

                                                                                                                                                                          { BoardMetaDataFlags.StraightFlushComplete, BoardMetaDataFlags.StraightFlushComplete },
                                                                                                                                                                          { BoardMetaDataFlags.FlushComplete, BoardMetaDataFlags.StraightFlushComplete },
                                                                                                                                                                          { BoardMetaDataFlags.OneCardFlushPossible, BoardMetaDataFlags.FlushComplete },
                                                                                                                                                                          { BoardMetaDataFlags.FlushPossible, BoardMetaDataFlags.OneCardFlushPossible },
                                                                                                                                                                          { BoardMetaDataFlags.FlushDrawPossible, BoardMetaDataFlags.FlushPossible },

                                                                                                                                                                          { BoardMetaDataFlags.None, BoardMetaDataFlags.None } };

        public static HashSet<BoardMetaDataFlags> PLstAllFlopBoardTypes = new HashSet<BoardMetaDataFlags>();
        public static HashSet<BoardMetaDataFlags> PLstAllTurnBoardTypes = new HashSet<BoardMetaDataFlags>();
        public static HashSet<BoardMetaDataFlags> PLstAllRiverBoardTypes = new HashSet<BoardMetaDataFlags>();

        public static Dictionary<int, (ulong, BoardMetaDataFlags)> PDicAllBoardsByID = new Dictionary<int, (ulong, BoardMetaDataFlags)>(CC_ALL_BOARDS_COUNT);
        public static Dictionary<ulong, (int, BoardMetaDataFlags)> PDicAllBoardsByBoardMask = new Dictionary<ulong, (int, BoardMetaDataFlags)>(CC_ALL_BOARDS_COUNT); // First item of tuple = ID of board
        public static Dictionary<Street, List<(int, BoardMetaDataFlags)>> PDicAllBoardsByStreet = new Dictionary<Street, List<(int, BoardMetaDataFlags)>>(3);
        public static Dictionary<BoardMetaDataFlags, Dictionary<BoardMetaDataFlags, Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>>> PDicAllRiverBoardTypesByGroupType = new Dictionary<BoardMetaDataFlags, Dictionary<BoardMetaDataFlags, Dictionary<BoardMetaDataFlags, BoardMetaDataFlags>>>();

        public static List<Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, HandTypes)>>> PLstAllBoardsInfos = new List<Dictionary<ulong, Dictionary<ulong, (double, sbyte, byte, HandTypes)>>>(10);

        private static SQLiteConnection FFReadOnlyConnection = GetConnectionInReadOnlyMode().OpenAndReturn();

        public static void LoadAllBoards()
        {
            PDicAllBoardsByStreet.Add(Street.Flop, new List<(int, BoardMetaDataFlags)>(CC_ALL_BOARDS_COUNT));
            PDicAllBoardsByStreet.Add(Street.Turn, new List<(int, BoardMetaDataFlags)>(CC_ALL_BOARDS_COUNT));
            PDicAllBoardsByStreet.Add(Street.River, new List<(int, BoardMetaDataFlags)>(CC_ALL_BOARDS_COUNT));

            string sql = "SELECT TBoardMask.ID, TBoardMask.Mask, TBoardMetaData.Value FROM TBoardMask INNER JOIN TBoardMetaData ON TBoardMask.BoardMetaData=TBoardMetaData.ID;";
            using (SQLiteCommand command = new SQLiteCommand(sql, FFReadOnlyConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int boardMaskID = reader.GetFieldData<int>(0);
                        ulong boardMask = reader.GetFieldData<ulong>(1);
                        BoardMetaDataFlags boardType = reader.GetFieldData<BoardMetaDataFlags>(2);

                        PDicAllBoardsByID.Add(boardMaskID, (boardMask, boardType));
                        PDicAllBoardsByBoardMask.Add(boardMask, (boardMaskID, boardType));

                        switch (Hand.BitCount(boardMask))
                        {
                            case 3:
                                PDicAllBoardsByStreet[Street.Flop].Add((boardMaskID, boardType));
                                break;
                            case 4:
                                PDicAllBoardsByStreet[Street.Turn].Add((boardMaskID, boardType));
                                break;
                            case 5:
                                PDicAllBoardsByStreet[Street.River].Add((boardMaskID, boardType));
                                break;
                        }

                        int numberOfCardsOnBoard = Hand.Cards(boardMask).Count();

                        switch (numberOfCardsOnBoard)
                        {
                            case 3:
                                if (!PLstAllFlopBoardTypes.Contains(boardType))
                                    PLstAllFlopBoardTypes.Add(boardType);
                                break;
                            case 4:
                                if (!PLstAllTurnBoardTypes.Contains(boardType))
                                    PLstAllTurnBoardTypes.Add(boardType);
                                break;
                            case 5:
                                if (!PLstAllRiverBoardTypes.Contains(boardType))
                                    PLstAllRiverBoardTypes.Add(boardType);
                                break;
                            default:
                                throw new InvalidOperationException("Invalid board! The number of cards on the board should be 3, 4 or 5.");
                        }
                    }
                }
            }
        }

        public static void GenerateAllPossibleBoardsByFlopBoard()
        {
            throw new NotImplementedException();
            if (PDicAllBoardsByBoardMask.Count == 0)
                throw new InvalidOperationException("Must call LoadAllBoards before calling this method!");

            string sqlFlop = "CREATE TABLE IF NOT EXISTS TAllPossibleBoardsByFlopBoardMask (FlopBoardMaskID INTEGER(3) NOT NULL, BoardMaskID INTEGER(3) NOT NULL,PRIMARY KEY(FlopBoardMaskID, BoardMaskID),FOREIGN KEY(FlopBoardMaskID) REFERENCES TBoardMask(ID), FOREIGN KEY (BoardMaskID) REFERENCES TBoardMask(ID));SELECT count(*) FROM TAllPossibleBoardsByFlopBoardMask LIMIT 1;";

            if (FFReadOnlyConnection.State == System.Data.ConnectionState.Closed)
                FFReadOnlyConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sqlFlop, FFReadOnlyConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        void InsertBoards(ulong _flopBoard, IEnumerable<ulong> _boards)
                        {
                            using (SQLiteTransaction transaction = FFReadOnlyConnection.BeginTransaction())
                            {
                                foreach (var infos in _boards)
                                {
                                    string sql = "INSERT INTO TAllPossibleBoardsByFlopBoardMask (FlopBoardMaskID, BoardMaskID) VALUES (?, ?);";

                                    using (SQLiteCommand insertCommand = new SQLiteCommand(sql, FFReadOnlyConnection))
                                    {
                                        insertCommand.CommandText = sql;
                                        insertCommand.Parameters.Add(new SQLiteParameter("", PDicAllBoardsByBoardMask[_flopBoard].Item1));
                                        insertCommand.Parameters.Add(new SQLiteParameter("", PDicAllBoardsByBoardMask[infos].Item1));

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                            }
                        }
                        void CreateData()
                        {
                            List<(ulong, (IEnumerable<ulong>, IEnumerable<ulong>))> allBoards = new List<(ulong, (IEnumerable<ulong>, IEnumerable<ulong>))>(3000000);

                            foreach (var flopBoard in Hand.Hands(3))
                            {
                                var allTurnBoards = Hand.Hands(flopBoard, 0L, 4);
                                var allRiverBoards = Hand.Hands(flopBoard, 0L, 5);

                                allBoards.Add((flopBoard, (allTurnBoards, allRiverBoards)));
                            }

                            foreach (var board in allBoards)
                            {
                                InsertBoards(board.Item1, board.Item2.Item1);
                                InsertBoards(board.Item1, board.Item2.Item2);
                            }
                        }

                        if ((long)reader[0] == 0)
                            CreateData();
                    }
                    else
                        throw new Exception("Unable to read the table! Was the table created?");
                }
            }
        }
        public static void LoadBoardInfosAsync(ulong _flopBoardMask, ulong _dead, int _indexPlayer)
        {
            Console.WriteLine("Enter");
            Stopwatch ms = new Stopwatch();
            ms.Start();
            PLstAllBoardsInfos[_indexPlayer].Clear();

            var allTurnCards = Hand.Hands(0L, _flopBoardMask | _dead, 1);
            var allRiverCards = Hand.Hands(0L, _flopBoardMask | _dead, 2);

            object lockOperation = new object();

            List<ulong> allTurnBoardsMask = new List<ulong>(50);
            List<ulong> allRiverBoardsMask = new List<ulong>(1300);

            foreach (var turnCard in allTurnCards)
                allTurnBoardsMask.Add(_flopBoardMask | turnCard);

            foreach (var turnRiverCard in allRiverCards)
                allRiverBoardsMask.Add(_flopBoardMask | turnRiverCard);
            
            Action flopIteration = new Action(() =>
            {
                PLstAllBoardsInfos[_indexPlayer].Add(_flopBoardMask, new Dictionary<ulong, (double, sbyte, byte, HandTypes)>(1326));

                foreach (var tupleInfos in ReadData(_flopBoardMask, _indexPlayer))
                {
                    lock (lockOperation)
                    {
                        PLstAllBoardsInfos[_indexPlayer][_flopBoardMask].Add(tupleInfos.Item1, (tupleInfos.Item2, tupleInfos.Item3, tupleInfos.Item4, tupleInfos.Item5));
                    }
                }                    
            });

            Action turnIteration = new Action(() =>
            {
                Parallel.ForEach(allTurnBoardsMask, (currentBoardMask) =>
                {
                    lock (lockOperation)
                    {
                        PLstAllBoardsInfos[_indexPlayer].Add(currentBoardMask, new Dictionary<ulong, (double, sbyte, byte, HandTypes)>(1326));
                    }
                    Parallel.ForEach(ReadData(currentBoardMask, _indexPlayer), (tupleInfos) =>
                    {
                        lock (lockOperation)
                        {
                            PLstAllBoardsInfos[_indexPlayer][currentBoardMask].Add(tupleInfos.Item1, (tupleInfos.Item2, tupleInfos.Item3, tupleInfos.Item4, tupleInfos.Item5));
                        }
                    });
                });
            });

            Action riverIteration = new Action(() =>
            {
                Parallel.ForEach(allRiverBoardsMask, (currentBoardMask) =>
                {
                    lock (lockOperation)
                    {
                        PLstAllBoardsInfos[_indexPlayer].Add(currentBoardMask, new Dictionary<ulong, (double, sbyte, byte, HandTypes)>(1326));
                    }
                    Parallel.ForEach(ReadData(currentBoardMask, _indexPlayer), (tupleInfos) =>
                    {
                        lock (lockOperation)
                        {
                            PLstAllBoardsInfos[_indexPlayer][currentBoardMask].Add(tupleInfos.Item1, (tupleInfos.Item2, tupleInfos.Item3, tupleInfos.Item4, tupleInfos.Item5));
                        }
                    });
                });
            });

            Parallel.Invoke(flopIteration, turnIteration, riverIteration);
            ms.Stop();
            Console.WriteLine(ms.ElapsedMilliseconds);
        }

        public static IEnumerable<(ulong, double, sbyte, byte, HandTypes)> ReadData(ulong _currentBoardMask, int _indexPlayer)
        {
            using (SQLiteConnection connection = GetConnectionInReadOnlyMode().OpenAndReturn())
            {
                string sql = "SELECT TPocketMask.Mask, THandStrength.Value, THand.NbOuts, THand.MetaData FROM ((THand INNER JOIN TPocketMask ON THand.PocketMask=TPocketMask.ID) INNER JOIN THandStrength ON THand.HandStrengths = THandStrength.ID) WHERE THand.BoardMask = ?";

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue(null, CDBHelperHandInfos.PDicAllBoardsByBoardMask[_currentBoardMask].Item1);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pocketMask = reader.GetInt64(0);
                            var hs = reader.GetDouble(1);
                            var outs = reader.GetInt64(2);
                            var metaData = reader.GetInt64(3);
                            HandTypes handType = Hand.GetHandTypeExcludingBoard((ulong)pocketMask, _currentBoardMask);

                            yield return ((ulong)pocketMask, hs, (sbyte)outs, (byte)metaData, handType);
                        }
                    }
                }
            }
        }

        /*
        private static SQLiteConnection GetConnection(string _dbPath = @"D:\\BigData\\HandInfosDBV4.db")
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);
            
            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;Pooling=True;Max Pool Size=1500;");

            return dbConnection;
        }*/

        private static SQLiteConnection GetConnectionInReadOnlyMode(string _dbPath = @"C:\Users\admin\Desktop\HandInfosDBV4.db")
        {
                if (!File.Exists(_dbPath))
                    SQLiteConnection.CreateFile(_dbPath);

                SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
                connBuilder.DataSource = _dbPath;
                connBuilder.Version = 3;
                connBuilder.Pooling = false;                
                connBuilder.DefaultTimeout = 100;
                connBuilder.ReadOnly = true;
                connBuilder.SyncMode = SynchronizationModes.Off;
                connBuilder.JournalMode = SQLiteJournalModeEnum.Off;
                connBuilder.DefaultIsolationLevel = System.Data.IsolationLevel.Serializable;                

                SQLiteConnection dbConnection = new SQLiteConnection(connBuilder.ConnectionString);

                return dbConnection;
        }
    }
}//Pooling=True;Min Pool Size=5;Max Pool Size=100
