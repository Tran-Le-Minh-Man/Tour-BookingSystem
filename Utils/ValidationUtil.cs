using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TourBookingSystem.Utils
{
    /**
     * Validation utility class
     */
    public class ValidationUtil
    {
        private static readonly string EMAIL_PATTERN = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private static readonly string PHONE_PATTERN = @"^[0-9]{10,11}$";
        
        /**
         * Validate email format
         */
        public static bool isValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }
        
        /**
         * Validate phone number format
         */
        public static bool isValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }
            
            return Regex.IsMatch(phone, PHONE_PATTERN);
        }
        
        /**
         * Validate password strength
         */
        public static bool isValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            
            // At least 6 characters
            if (password.Length < 6)
            {
                return false;
            }
            
            return true;
        }
        
        /**
         * Sanitize string for output (prevent XSS)
         */
        public static string sanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            
            // Remove HTML tags
            return Regex.Replace(input, "<[^>]*>", string.Empty);
        }
        
        /**
         * Truncate string to max length
         */
        public static string truncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }
    }
}
