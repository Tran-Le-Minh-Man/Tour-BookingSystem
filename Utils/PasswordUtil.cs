using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TourBookingSystem.Utils
{
    /**
     * Password utility class for hashing and validation
     */
    public class PasswordUtil
    {
        private static readonly int SALT_WORK_FACTOR = 10;
        
        /**
         * Hash a password using BCrypt-like algorithm
         * For simplicity, using a basic hash - in production, use BCrypt.NET
         */
        public static string hashPassword(string plainPassword)
        {
            if (plainPassword == null)
            {
                return null;
            }
            
            // Simple hash for demo purposes
            // In production, use BCrypt or similar
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword));
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        
        /**
         * Check if password matches hashed password
         */
        public static bool checkPassword(string plainPassword, string hashedPassword)
        {
            if (plainPassword == null || hashedPassword == null)
            {
                return false;
            }
            
            string newHash = hashPassword(plainPassword);
            return hashedPassword.Equals(newHash);
        }
        
        /**
         * Generate a random salt
         */
        public static string generateSalt()
        {
            var random = new Random();
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
