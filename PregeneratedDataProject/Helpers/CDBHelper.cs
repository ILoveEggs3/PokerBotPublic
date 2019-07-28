using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;
using Shared.Models.Database;

namespace PregeneratedDataProject.Helpers
{
    public static class CDBHelper
    {
        public static readonly string POCKETS_TABLE_NAME = "Pockets";
        public static readonly string POCKETS_TABLE_FIELDS_POCKETMASK_NAME = "PocketMask";

        public static readonly string BOARDS_TABLE_NAME = "Boards";
        public static readonly string BOARDS_TABLE_FIELDS_BOARDMASK_NAME = "BoardMask";
        public static readonly string BOARDS_TABLE_FIELDS_BOARDHEAT_NAME = "BoardHeat";
        public static readonly string BOARDS_TABLE_FIELDS_METADATA_NAME = "BoardMetaData";

        public static readonly string HANDS_TABLE_NAME = "Hands";
        public static readonly string HANDS_TABLE_FIELDS_HANDSTRENGTH_NAME = "HandStrength";
        public static readonly string HANDS_TABLE_FIELDS_OUTSMASK_NAME = "OutsMask";
        public static readonly string HANDS_TABLE_FIELDS_METADATA_NAME = "MetaData";


        private static SQLiteConnection FFConnection = GetConnection();

        public static void InsertPocket(CPocketModel _pocket)
        {
            CreatePocketsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            try
            {
                string sql = String.Format("INSERT INTO {0} ({1}) values (?);", POCKETS_TABLE_NAME, POCKETS_TABLE_FIELDS_POCKETMASK_NAME);
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter(POCKETS_TABLE_FIELDS_POCKETMASK_NAME, _pocket.PPocketMask));
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static void InsertBoards(List<CBoardModel> _boardList)
        {
            CreateBoardsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            try
            {
                const string sqlFormat = "INSERT INTO {0} ({1}, {2}, {3}) values ";

                int ind = 0;
                while (ind < _boardList.Count)
                {
                    int maxInd = Math.Min(ind + 999, _boardList.Count);
                    string sql = String.Format(sqlFormat, BOARDS_TABLE_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, BOARDS_TABLE_FIELDS_BOARDHEAT_NAME, BOARDS_TABLE_FIELDS_METADATA_NAME);
                    for (int i = ind; i < maxInd; i++)
                    {
                        sql += String.Format("({0}, {1}, {2}), ", _boardList[i].PBoardMask, _boardList[i].PHeat, _boardList[i].PMetaDataMask);
                    }
                    sql = sql.Remove(sql.Length - 2, 2);
                    sql += ";";
                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        using (var transaction = FFConnection.BeginTransaction())
                        {
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    ind = maxInd;
                    Console.WriteLine("Inserting boards: {0}%", 1.0d * ind / _boardList.Count * 100);
                }

            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static void InsertBoard(CBoardModel _board)
        {
            CreateBoardsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            try
            {
                string sql = String.Format("INSERT INTO {0} ({1}, {2}, {3}) values (?, ?, ?);", BOARDS_TABLE_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, BOARDS_TABLE_FIELDS_BOARDHEAT_NAME, BOARDS_TABLE_FIELDS_METADATA_NAME);
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter(BOARDS_TABLE_FIELDS_BOARDMASK_NAME, _board.PBoardMask));
                    command.Parameters.Add(new SQLiteParameter(BOARDS_TABLE_FIELDS_BOARDHEAT_NAME, _board.PHeat));
                    command.Parameters.Add(new SQLiteParameter(BOARDS_TABLE_FIELDS_METADATA_NAME, _board.PMetaDataMask));
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static void InsertHand(CHandModel _hand)
        {
            CreateHandsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            try
            {
                string sql = String.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) values (?, ?, ?, ?, ?);", HANDS_TABLE_NAME, POCKETS_TABLE_FIELDS_POCKETMASK_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, HANDS_TABLE_FIELDS_HANDSTRENGTH_NAME, HANDS_TABLE_FIELDS_OUTSMASK_NAME, HANDS_TABLE_FIELDS_METADATA_NAME);
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter(BOARDS_TABLE_FIELDS_BOARDMASK_NAME, _hand.PRefPocketMask));
                    command.Parameters.Add(new SQLiteParameter(POCKETS_TABLE_FIELDS_POCKETMASK_NAME, _hand.PRefBoardMask));
                    command.Parameters.Add(new SQLiteParameter(HANDS_TABLE_FIELDS_HANDSTRENGTH_NAME, _hand.PHandStrength));
                    command.Parameters.Add(new SQLiteParameter(HANDS_TABLE_FIELDS_OUTSMASK_NAME, _hand.POutsMask));
                    command.Parameters.Add(new SQLiteParameter(HANDS_TABLE_FIELDS_METADATA_NAME, _hand.PMetaData));
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static void InsertHands(List<CHandModel> _handList)
        {
            CreateHandsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            try
            {
                const string sqlFormat = "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) values ";

                using (var transaction = FFConnection.BeginTransaction())
                {

                    //Parallel.For(0, (_handList.Count / 499) + 1, x =>
                    for (int x = 0; x < (_handList.Count / 499) + 1; x++)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(FFConnection))
                        {
                            int ind = 499 * x;

                            if (ind < _handList.Count)
                            {
                                string sql = String.Format(sqlFormat, HANDS_TABLE_NAME, POCKETS_TABLE_FIELDS_POCKETMASK_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, HANDS_TABLE_FIELDS_HANDSTRENGTH_NAME, HANDS_TABLE_FIELDS_OUTSMASK_NAME, HANDS_TABLE_FIELDS_METADATA_NAME);

                                int maxInd = Math.Min(ind + 499, _handList.Count);
                                for (int i = ind; i < maxInd; i++)
                                {
                                    sql += String.Format("({0}, {1}, {2}, {3}, {4}), ", _handList[i].PRefPocketMask, _handList[i].PRefBoardMask, _handList[i].PHandStrength.ToString().Replace(',', '.'), _handList[i].POutsMask, _handList[i].PMetaData);
                                }
                                sql = sql.Remove(sql.Length - 2, 2);
                                sql += ";";

                                //Console.WriteLine("Inserting hands: {0}%", 1.0d * ind / _handList.Count * 100);
                                lock (command)
                                {
                                    command.CommandText = sql;
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        // });
                    }

                    transaction.Commit();
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }


        public static bool IsBoardHandInsideHandsTable(ulong _boardMask)
        {
            CreateHandsTableIfNotExists();
            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();
            bool ret = false;
            try
            {
                string sql = String.Format("SELECT * FROM Hands WHERE (BoardMask == {0} AND PocketMask == 3) OR (BoardMask == {0} AND PocketMask == 12) OR (BoardMask == {0} AND PocketMask == 48) OR (BoardMask == {0} AND PocketMask == 192) OR (BoardMask == {0} AND PocketMask == 768) OR (BoardMask == {0} AND PocketMask == 3072);", _boardMask);
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.CommandText = sql;
                    ret = command.ExecuteScalar() != null;
                }
            }
            finally
            {
                FFConnection.Close();
            }
            return ret;
        }

        private static void CreatePocketsTableIfNotExists()
        {
            string sql = String.Format("CREATE TABLE IF NOT EXISTS {0} ({1} INTEGER(8) PRIMARY KEY);", POCKETS_TABLE_NAME, POCKETS_TABLE_FIELDS_POCKETMASK_NAME);

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }
        }

        private static void CreateBoardsTableIfNotExists()
        {
            string sql = String.Format("CREATE TABLE IF NOT EXISTS {0} ({1} INTEGER(8) PRIMARY KEY, {2} REAL, {3} INTEGER(2));", BOARDS_TABLE_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, BOARDS_TABLE_FIELDS_BOARDHEAT_NAME, BOARDS_TABLE_FIELDS_METADATA_NAME);

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }
        }

        private static void CreateHandsTableIfNotExists()
        {
            string sql = String.Format("CREATE TABLE IF NOT EXISTS {0} ({3} INTEGER(8), {4} INTEGER(8), {5} REAL, {6} INTEGER(8), {7} INTEGER(8), FOREIGN KEY({3}) REFERENCES {1}({3}), FOREIGN KEY({4}) REFERENCES {2}({4}), PRIMARY KEY ({3}, {4}));", HANDS_TABLE_NAME, POCKETS_TABLE_NAME, BOARDS_TABLE_NAME, POCKETS_TABLE_FIELDS_POCKETMASK_NAME, BOARDS_TABLE_FIELDS_BOARDMASK_NAME, HANDS_TABLE_FIELDS_HANDSTRENGTH_NAME, HANDS_TABLE_FIELDS_OUTSMASK_NAME, HANDS_TABLE_FIELDS_METADATA_NAME);

            if (FFConnection.State == System.Data.ConnectionState.Closed)
                FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection.Close();
            }
        }

        private static SQLiteConnection GetConnection(string _dbPath = @"C:\Users\admin\Desktop\HandInfosDB.db")
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;");

            return dbConnection;
        }
    }
}
