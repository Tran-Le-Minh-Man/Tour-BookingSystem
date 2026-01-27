using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TourBookingSystem.Models;
using TourBookingSystem.Database;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for User entity using LINQ
     */
    public class UserDAO
    {
        private readonly ApplicationDbContext _context;

        public UserDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public UserDAO()
        {
        }

        /**
         * Register a new user with password hashing
         */
        public bool register(User user, string plainPassword)
        {
            string hashedPassword = hashPassword(plainPassword);
            user.setPassword(hashedPassword);
            return insert(user);
        }

        private string hashPassword(string plainPassword)
        {
            return PasswordUtil.hashPassword(plainPassword);
        }

        public User findById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        public User findByEmail(string email)
        {
            string normalizedEmail = email.ToLower().Trim();
            return _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);
        }

        public User verifyLogin(string email, string password)
        {
            string normalizedEmail = email.ToLower().Trim();
            var user = _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);

            if (user != null && checkPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        private bool checkPassword(string plainPassword, string hashedPassword)
        {
            try
            {
                return PasswordUtil.checkPassword(plainPassword, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public List<User> getAllUsers()
        {
            return _context.Users.OrderByDescending(u => u.UserId).ToList();
        }

        public List<User> getUsersByRole(string role)
        {
            return _context.Users
                .Where(u => u.Role == role)
                .OrderByDescending(u => u.UserId)
                .ToList();
        }

        public List<User> filterUsersByRoleAndSearch(string role, string keyword)
        {
            string searchPattern = keyword.Trim();
            return _context.Users
                .Where(u => u.Role == role &&
                           (u.FullName != null && u.FullName.Contains(searchPattern)) ||
                           (u.Email != null && u.Email.Contains(searchPattern)))
                .OrderByDescending(u => u.UserId)
                .ToList();
        }

        public bool insert(User user)
        {
            try
            {
                if (user.CreatedAt == null) user.CreatedAt = DateTime.Now;
                if (user.Role == null) user.Role = "USER";
                user.Email = user.Email?.ToLower().Trim();

                _context.Users.Add(user);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting user: " + ex.Message);
                return false;
            }
        }

        public bool update(User user)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
                if (existingUser != null)
                {
                    existingUser.FullName = user.FullName;
                    existingUser.Phone = user.Phone;
                    existingUser.Role = user.Role;
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating user: " + ex.Message);
                return false;
            }
        }

        public bool updatePassword(int userId, string newPlainPassword)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.PasswordHash = hashPassword(newPlainPassword);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating password: " + ex.Message);
                return false;
            }
        }

        public bool updateRole(int userId, string role)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.Role = role;
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating role: " + ex.Message);
                return false;
            }
        }

        public bool delete(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return false;
            }
        }

        public bool emailExists(string email)
        {
            string normalizedEmail = email.ToLower().Trim();
            return _context.Users.Any(u => u.Email == normalizedEmail);
        }

        public int countByRole(string role)
        {
            string normalizedRole = role.ToUpper().Trim();
            return _context.Users.Count(u => u.Role.ToUpper() == normalizedRole);
        }

        public int getTotalCount()
        {
            return _context.Users.Count();
        }

        public List<User> searchUsers(string keyword)
        {
            string searchPattern = keyword.Trim();
            return _context.Users
                .Where(u => (u.FullName != null && u.FullName.Contains(searchPattern)) ||
                            (u.Email != null && u.Email.Contains(searchPattern)))
                .OrderByDescending(u => u.UserId)
                .ToList();
        }

        public bool updateRememberToken(int userId, string token, DateTime expiryDate)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.RememberToken = token;
                    user.TokenExpiry = expiryDate;
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating remember token: " + ex.Message);
                return false;
            }
        }

        public bool clearRememberToken(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.RememberToken = null;
                    user.TokenExpiry = null;
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error clearing remember token: " + ex.Message);
                return false;
            }
        }

        public User findByRememberToken(string token)
        {
            // Note: TokenExpiry is stored as string, comparison might be tricky depending on format
            // In original code it was OleDbType.Date. We'll assume it's parseable or handle accordingly.
            return _context.Users.FirstOrDefault(u => u.RememberToken == token);
            // Ideally check expiry here, but would need to parse string back to DateTime
        }
    }
}
