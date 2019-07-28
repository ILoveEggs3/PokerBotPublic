using Silence.Macro;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace AmigoDB
{
    public static class CDBHelper
    {
        private static SQLiteConnection FFConnection = GetConnection();
        private const string FFDBDefaultPath = @"../../../DB.amigo";
        private const string FFTableName = "MouseMovementMacros";
        private const string FFCreateTableQueryStatement =
            "CREATE TABLE IF NOT EXISTS " + FFTableName + " " +
            "(ID INTEGER PRIMARY KEY ASC,"+
            "XCoordsBegin INTEGER," +
            "YCoordsBegin INTEGER," +
            "XCoordsEnd INTEGER," +
            "YCoordsEnd INTEGER," +
            "TrueDistance DOUBLE," +
            "EuclideDistance DOUBLE," +
            "Orientation DOUBLE," +
            "ClickAtTheEnd BOOLEAN," +
            "Ticks BIGINT," +
            "Movement TEXT);";
        private const string FFInsertQueryStatement = 
            "INSERT INTO " + FFTableName + " " +
            "(XCoordsBegin, " +
            "YCoordsBegin, " +
            "XCoordsEnd, " +
            "YCoordsEnd, " +
            "TrueDistance, " +
            "EuclideDistance, " +
            "Orientation, " +
            "ClickAtTheEnd, " +
            "Ticks, " +
            "Movement) " +
            "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";

        public static void InsertMovementRecursive(Macro _mouseMovementObject)
        {

            CreateMouseMovementsTableIfNotExist();
            FFConnection.Open();

            try
            {
                while ((Math.Abs(_mouseMovementObject.PInitialCoord.X - _mouseMovementObject.PEndCoord.X) + Math.Abs(_mouseMovementObject.PInitialCoord.Y - _mouseMovementObject.PEndCoord.Y)) > 0.999d)
                {
                    string sql = FFInsertQueryStatement;
                    using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    {
                        command.Parameters.Add(new SQLiteParameter("XCoordsBegin", _mouseMovementObject.PInitialCoord.X));
                        command.Parameters.Add(new SQLiteParameter("YCoordsBegin", _mouseMovementObject.PInitialCoord.Y));
                        command.Parameters.Add(new SQLiteParameter("XCoordsEnd", _mouseMovementObject.PEndCoord.X));
                        command.Parameters.Add(new SQLiteParameter("YCoordsEnd", _mouseMovementObject.PEndCoord.Y));
                        command.Parameters.Add(new SQLiteParameter("TrueDistance", _mouseMovementObject.PTrueDistance));
                        command.Parameters.Add(new SQLiteParameter("EuclideDistance", _mouseMovementObject.PEuclideDist));
                        command.Parameters.Add(new SQLiteParameter("Orientation", _mouseMovementObject.POrientation));
                        command.Parameters.Add(new SQLiteParameter("ClickAtTheEnd", _mouseMovementObject.PClickAtTheEnd));
                        command.Parameters.Add(new SQLiteParameter("Ticks", _mouseMovementObject.PTicks));
                        command.Parameters.Add(new SQLiteParameter("Movement", _mouseMovementObject.ToXml()));

                        command.ExecuteNonQuery();
                        _mouseMovementObject = _mouseMovementObject.RemoveFirstCoord();
                    }
                }
            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static void InsertMovement(Macro _mouseMovementObject)
        {
            CreateMouseMovementsTableIfNotExist();
            FFConnection.Open();

            try
            {
                string sql = FFInsertQueryStatement;
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter("XCoordsBegin", _mouseMovementObject.PInitialCoord.X));
                    command.Parameters.Add(new SQLiteParameter("YCoordsBegin", _mouseMovementObject.PInitialCoord.Y));
                    command.Parameters.Add(new SQLiteParameter("XCoordsEnd", _mouseMovementObject.PEndCoord.X));
                    command.Parameters.Add(new SQLiteParameter("YCoordsEnd", _mouseMovementObject.PEndCoord.Y));
                    command.Parameters.Add(new SQLiteParameter("TrueDistance", _mouseMovementObject.PTrueDistance));
                    command.Parameters.Add(new SQLiteParameter("EuclideDistance", _mouseMovementObject.PEuclideDist));
                    command.Parameters.Add(new SQLiteParameter("Orientation", _mouseMovementObject.POrientation));
                    command.Parameters.Add(new SQLiteParameter("ClickAtTheEnd", _mouseMovementObject.PClickAtTheEnd));
                    command.Parameters.Add(new SQLiteParameter("Ticks", _mouseMovementObject.PTicks));
                    command.Parameters.Add(new SQLiteParameter("Movement", _mouseMovementObject.ToXml()));

                    command.ExecuteNonQuery();
                }

            }
            finally
            {
                FFConnection.Close();
            }
        }

        public static List<CMouseMovement> GetMouseMovements()
        {
            List<CMouseMovement> lstMouseMovements = new List<CMouseMovement>();

            CreateMouseMovementsTableIfNotExist();
            FFConnection.Open();
            List<int> qwe = new List<int>();
            try
            {               
                string sql = "SELECT * FROM MouseMovementMacros;";
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            CMouseMovement newMouseMovement = new CMouseMovement(Convert.ToInt32(reader[1]),
                                                                                 Convert.ToInt32(reader[2]),
                                                                                 Convert.ToInt32(reader[3]),
                                                                                 Convert.ToInt32(reader[4]),
                                                                                 Convert.ToDouble(reader[5]),
                                                                                 Convert.ToDouble(reader[6]),
                                                                                 Convert.ToDouble(reader[7]),
                                                                                 Convert.ToBoolean(reader[8]),
                                                                                 Convert.ToUInt64(reader[9]),
                                                                                 Convert.ToString(reader[10]));
                            lstMouseMovements.Add(newMouseMovement);

                        }
                    }
                }          
            }
            finally
            {
                FFConnection.Close();
            }

            return lstMouseMovements;
        }
        public static List<CMouseMovement> GetShortMouseMovements()
        {
            List<CMouseMovement> lstMouseMovements = new List<CMouseMovement>();

            CreateMouseMovementsTableIfNotExist();
            FFConnection.Open();

            try
            {                
                string sql = "SELECT * FROM MouseMovementMacros WHERE Ticks < ?;"; //2.5sec
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                {
                    command.Parameters.Add(new SQLiteParameter("Ticks", 25 * 1000 * 1000)); //2.5sec

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CMouseMovement newMouseMovement = new CMouseMovement(Convert.ToInt32(reader[1]),
                                                                                 Convert.ToInt32(reader[2]),
                                                                                 Convert.ToInt32(reader[3]),
                                                                                 Convert.ToInt32(reader[4]),
                                                                                 Convert.ToDouble(reader[5]),
                                                                                 Convert.ToDouble(reader[6]),
                                                                                 Convert.ToDouble(reader[7]),
                                                                                 Convert.ToBoolean(reader[8]),
                                                                                 Convert.ToUInt64(reader[9]),
                                                                                 Convert.ToString(reader[10]));
                            lstMouseMovements.Add(newMouseMovement);
                        }
                    }
                }
            }
            finally
            {
                FFConnection.Close();
            }

            return lstMouseMovements;
        }
        
        private static SQLiteConnection GetConnection(string _dbPath = FFDBDefaultPath)
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);
            

            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;");

            return dbConnection;
        }

        private static void CreateMouseMovementsTableIfNotExist()
        {
            string sql = FFCreateTableQueryStatement;

            FFConnection.Open();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, FFConnection))
                    command.ExecuteNonQuery();
            }
            finally
            {
                FFConnection?.Close();
            }
        }
    }
}
