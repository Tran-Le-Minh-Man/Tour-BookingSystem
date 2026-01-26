using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Utils
{
    /**
     * Database connection utility class for Microsoft Access Database (ACCDB)
     */
    public class DBConnection
    {
        // Database URL
        private static string databaseUrl;

        // Store database path
        private static string databasePath;

        // Lock object for thread safety
        private static readonly object lockObject = new object();

        /**
         * Initialize with database path
         */
        public static void init(string path)
        {
            databasePath = path;
            databaseUrl = findDatabaseUrl();
            Console.WriteLine("Database initialized: " + databaseUrl);
        }

        /**
         * Get database URL
         */
        private static string findDatabaseUrl()
        {
            Console.WriteLine("=== Finding Database ===");
            Console.WriteLine("Database Path: " + databasePath);

            // Check if provided path exists
            if (!string.IsNullOrEmpty(databasePath) && File.Exists(databasePath))
            {
                string absolutePath = Path.GetFullPath(databasePath);
                Console.WriteLine("FOUND: " + absolutePath);
                return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={absolutePath};Persist Security Info=False;";
            }

            Console.WriteLine("ERROR: Database file not found at: " + databasePath);
            return "";
        }

        /**
         * Get database connection
         * Thread-safe implementation using lock
         */
        public static OleDbConnection getConnection()
        {
            lock (lockObject)
            {
                // Initialize on first use if not already done
                if (string.IsNullOrEmpty(databaseUrl))
                {
                    databaseUrl = findDatabaseUrl();
                }

                if (string.IsNullOrEmpty(databaseUrl))
                {
                    throw new Exception("Database not configured. Please ensure database file exists.");
                }

                OleDbConnection connection = new OleDbConnection(databaseUrl);
                try
                {
                    connection.Open();
                    Console.WriteLine("Database connection established successfully");
                    return connection;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Database Connection failed: " + e.Message);
                    throw;
                }
            }
        }

        /**
         * Test database connection
         */
        public static bool testConnection()
        {
            try
            {
                using (OleDbConnection conn = getConnection())
                {
                    return conn != null && conn.State == ConnectionState.Open;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection test failed: " + e.Message);
                return false;
            }
        }

        /**
         * Get current database URL (for debugging)
         */
        public static string getDatabaseUrl()
        {
            if (string.IsNullOrEmpty(databaseUrl))
            {
                databaseUrl = findDatabaseUrl();
            }
            return databaseUrl;
        }
    }
}
