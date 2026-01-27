using System;
using System.Data.OleDb;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TourBookingSystem.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // Check if Dual or #Dual tables exist
                    bool dualExists = false;
                    bool hashDualExists = false;
                    var schema = connection.GetSchema("Tables");
                    foreach (System.Data.DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (tableName.Equals("Dual", StringComparison.OrdinalIgnoreCase))
                            dualExists = true;
                        if (tableName.Equals("#Dual", StringComparison.OrdinalIgnoreCase))
                            hashDualExists = true;
                    }

                    if (!dualExists)
                    {
                        Console.WriteLine("Creating 'Dual' table...");
                        ExecuteCommand(connection, "CREATE TABLE [Dual] (Dummy VARCHAR(1))");
                        ExecuteCommand(connection, "INSERT INTO [Dual] (Dummy) VALUES ('X')");
                    }

                    if (!hashDualExists)
                    {
                        Console.WriteLine("Creating '#Dual' table...");
                        ExecuteCommand(connection, "CREATE TABLE [#Dual] (Dummy VARCHAR(1))");
                        ExecuteCommand(connection, "INSERT INTO [#Dual] (Dummy) VALUES ('X')");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing database: " + ex.Message);
            }
        }

        private static void ExecuteCommand(OleDbConnection connection, string sql)
        {
            using (var command = new OleDbCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
