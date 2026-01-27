using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using TourBookingSystem.Models;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for User entity
     * Handles all database operations for users
     */
    public class UserDAO
    {
        private static readonly string TABLE_NAME = "[users]";

        /**
         * Custom exception for database operations
         */
        public class UserDAOException : Exception
        {
            private readonly string operation;

            public UserDAOException(string operation, string message, Exception cause) : base(message, cause)
            {
                this.operation = operation;
            }

            public string getOperation() { return operation; }
        }

        /**
         * Register a new user with password hashing
         * @param user the user object (without password set)
         * @param plainPassword the plain text password
         * @return true if registration successful, false otherwise
         */
        public bool register(User user, string plainPassword)
        {
            // Hash the password
            string hashedPassword = hashPassword(plainPassword);

            // Set the hashed password on user object
            user.setPassword(hashedPassword);

            // Insert user into database
            return insert(user);
        }

        /**
         * Hash password
         * @param plainPassword the plain text password
         * @return hashed password
         */
        private string hashPassword(string plainPassword)
        {
            try
            {
                return PasswordUtil.hashPassword(plainPassword);
            }
            catch (Exception e)
            {
                throw new UserDAOException("hashPassword", "Error hashing password", e);
            }
        }

        /**
         * Find user by ID
         * @param id the user ID
         * @return User object if found, null otherwise
         */
        public User findById(int id)
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return mapResultSetToUser(rs);
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Find user by email
         * @param email the email to search
         * @return User object if found, null otherwise
         */
        public User findByEmail(string email)
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE email = ?";

            Console.WriteLine("=== USERDAO FIND BY EMAIL ===");
            Console.WriteLine("Searching for email: " + email.ToLower().Trim());
            Console.WriteLine("Database URL: " + DBConnection.getDatabaseUrl());

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = email.ToLower().Trim() });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            Console.WriteLine("User found! ID: " + rs["id"] + ", Email: " + rs["email"]);
                            return mapResultSetToUser(rs);
                        }
                        else
                        {
                            Console.WriteLine("User NOT found in database");
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Verify user login credentials
         * @param email user email
         * @param password user password (plain text, will be hashed)
         * @return User object if credentials valid, null otherwise
         */
        public User verifyLogin(string email, string password)
        {
            Console.WriteLine("=== USERDAO VERIFY LOGIN ===");
            Console.WriteLine("Email: " + email);
            Console.WriteLine("Database URL: " + DBConnection.getDatabaseUrl());

            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE email = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = email.ToLower().Trim() });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            User user = mapResultSetToUser(rs);

                            // Get stored password hash
                            string storedHash = rs["password_hash"].ToString();
                            Console.WriteLine("Found user: " + user.getEmail());
                            Console.WriteLine("Stored hash: " + (storedHash != null ? storedHash.Substring(0, Math.Min(10, storedHash.Length)) + "..." : "NULL"));

                            // Verify password
                            if (checkPassword(password, storedHash))
                            {
                                Console.WriteLine("Password verified successfully!");
                                return user;
                            }
                            else
                            {
                                Console.WriteLine("Password verification FAILED");
                            }
                        }
                        else
                        {
                            Console.WriteLine("User NOT found in database");
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Check if password matches hashed password
         * @param plainPassword the plain text password
         * @param hashedPassword the hashed password
         * @return true if password matches
         */
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

        /**
         * Get all users
         * @return list of all users
         */
        public List<User> getAllUsers()
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " ORDER BY id DESC";
            List<User> users = new List<User>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                using (OleDbDataReader rs = stmt.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        users.Add(mapResultSetToUser(rs));
                    }
                }
            }

            return users;
        }

        /**
         * Get users by role
         * @param role the role to filter (ADMIN, USER)
         * @return list of users with specified role
         */
        public List<User> getUsersByRole(string role)
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE role = ? ORDER BY id DESC";
            List<User> users = new List<User>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = role });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            users.Add(mapResultSetToUser(rs));
                        }
                    }
                }
            }

            return users;
        }

        /**
         * Filter users by role AND search by keyword (name or email)
         * @param role the role to filter (ADMIN, USER)
         * @param keyword the search keyword for name or email
         * @return list of matching users
         */
        public List<User> filterUsersByRoleAndSearch(string role, string keyword)
        {
            string sql = "SELECT * FROM " + TABLE_NAME +
                         " WHERE role = ? AND (full_name LIKE ? OR email LIKE ?) " +
                         "ORDER BY id DESC";
            List<User> users = new List<User>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = role });
                    string searchPattern = "%" + keyword + "%";
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            users.Add(mapResultSetToUser(rs));
                        }
                    }
                }
            }

            return users;
        }

        /**
         * Insert a new user
         * @param user the user to insert (password should already be hashed)
         * @return true if insertion successful, false otherwise
         */
        public bool insert(User user)
        {
            string sql = "INSERT INTO " + TABLE_NAME +
                         " (email, password_hash, full_name, phone, role, created_at, remember_token, token_expiry) " +
                         "VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

            Console.WriteLine("=== USERDAO INSERT DEBUG ===");
            Console.WriteLine("Database URL: " + DBConnection.getDatabaseUrl());
            Console.WriteLine("UserDAO.insert - Email: " + user.getEmail());
            Console.WriteLine("UserDAO.insert - Password hash: " + (user.getHashedPassword() != null ? user.getHashedPassword().Substring(0, Math.Min(10, user.getHashedPassword().Length)) + "..." : "NULL"));
            Console.WriteLine("UserDAO.insert - FullName: " + user.getFullName());

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    // Use explicit OleDbParameter with types to avoid type mismatch
                    // AddWithValue infers DBNull.Value as VarWChar, causing errors with Date columns

                    var p1 = new OleDbParameter("?", OleDbType.VarWChar, 100) { Value = user.getEmail().ToLower().Trim() };
                    var p2 = new OleDbParameter("?", OleDbType.VarWChar, 255) { Value = user.getHashedPassword() };
                    var p3 = new OleDbParameter("?", OleDbType.VarWChar, 100) { Value = sanitizeString(user.getFullName()) };

                    // Handle phone number: if empty, send DBNull.Value
                    string phone = sanitizeString(user.getPhone());
                    var p4 = new OleDbParameter("?", OleDbType.VarWChar, 20)
                    {
                        Value = string.IsNullOrEmpty(phone) ? DBNull.Value : phone
                    };

                    var p5 = new OleDbParameter("?", OleDbType.VarWChar, 20) { Value = user.getRole() != null ? user.getRole() : "USER" };
                    var p6 = new OleDbParameter("?", OleDbType.Date) { Value = DateTime.Now };
                    var p7 = new OleDbParameter("?", OleDbType.VarWChar, 255) { Value = DBNull.Value };
                    var p8 = new OleDbParameter("?", OleDbType.Date) { Value = DBNull.Value };

                    stmt.Parameters.Add(p1);
                    stmt.Parameters.Add(p2);
                    stmt.Parameters.Add(p3);
                    stmt.Parameters.Add(p4);
                    stmt.Parameters.Add(p5);
                    stmt.Parameters.Add(p6);
                    stmt.Parameters.Add(p7);
                    stmt.Parameters.Add(p8);

                    Console.WriteLine("Executing insert...");
                    int rowsAffected = stmt.ExecuteNonQuery();
                    Console.WriteLine("Rows affected: " + rowsAffected);

                    Console.WriteLine("=== INSERT SUCCESS ===");
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Update user
         * @param user the user to update
         * @return true if update successful, false otherwise
         */
        public bool update(User user)
        {
            string sql = "UPDATE " + TABLE_NAME +
                         " SET full_name = ?, phone = ?, role = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(user.getFullName()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(user.getPhone()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = user.getRole() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = user.getUserId() });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Update user password
         * @param userId the user ID
         * @param newPlainPassword the new plain text password
         * @return true if update successful
         */
        public bool updatePassword(int userId, string newPlainPassword)
        {
            string hashedPassword = hashPassword(newPlainPassword);

            string sql = "UPDATE " + TABLE_NAME + " SET password_hash = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = hashedPassword });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Update user role
         * @param userId the user ID
         * @param role the new role (ADMIN or USER)
         * @return true if update successful
         */
        public bool updateRole(int userId, string role)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET role = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = role });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Delete user by ID
         * @param userId the user ID to delete
         * @return true if deletion successful, false otherwise
         */
        public bool delete(int userId)
        {
            string sql = "DELETE FROM " + TABLE_NAME + " WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Check if email already exists
         * @param email the email to check
         * @return true if email exists, false otherwise
         */
        public bool emailExists(string email)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE email = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = email.ToLower().Trim() });

                    object result = stmt.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }

            return false;
        }

        /**
         * Get user count by role
         * @param role the role to count
         * @return number of users with specified role
         */
        public int countByRole(string role)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE UCASE(role) = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = role.ToUpper().Trim() });

                    object result = stmt.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 0;
        }

        /**
         * Get total user count
         * @return total number of users
         */
        public int getTotalCount()
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME;

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    object result = stmt.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 0;
        }

        /**
         * Search users by name or email
         * @param keyword the search keyword
         * @return list of matching users
         */
        public List<User> searchUsers(string keyword)
        {
            string sql = "SELECT * FROM " + TABLE_NAME +
                         " WHERE full_name LIKE ? OR email LIKE ? " +
                         "ORDER BY id DESC";
            List<User> users = new List<User>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    string searchPattern = "%" + keyword + "%";
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            users.Add(mapResultSetToUser(rs));
                        }
                    }
                }
            }

            return users;
        }

        /**
         * Map ResultSet to User object
         */
        private User mapResultSetToUser(OleDbDataReader rs)
        {
            User user = new User();
            user.setUserId(rs["id"] != DBNull.Value ? Convert.ToInt32(rs["id"]) : 0);
            user.setEmail(rs["email"] != DBNull.Value ? rs["email"].ToString() : "");
            user.setPassword(null);
            user.setFullName(rs["full_name"] != DBNull.Value ? rs["full_name"].ToString() : "");
            user.setPhone(rs["phone"] != DBNull.Value ? rs["phone"].ToString() : "");
            user.setRole(rs["role"] != DBNull.Value ? rs["role"].ToString() : "USER");

            if (rs["created_at"] != DBNull.Value)
            {
                user.setCreatedAt(Convert.ToDateTime(rs["created_at"]).ToString());
            }
            else
            {
                user.setCreatedAt("");
            }

            user.setRememberToken(rs["remember_token"] != DBNull.Value ? rs["remember_token"].ToString() : null);

            if (rs["token_expiry"] != DBNull.Value)
            {
                user.setTokenExpiry(Convert.ToDateTime(rs["token_expiry"]).ToString());
            }
            else
            {
                user.setTokenExpiry(null);
            }

            return user;
        }

        /**
         * Update user remember token
         * @param userId the user ID
         * @param token the remember token
         * @param expiryDate the expiry date for the token
         * @return true if update successful
         */
        public bool updateRememberToken(int userId, string token, DateTime expiryDate)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET remember_token = ?, token_expiry = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    var p1 = new OleDbParameter("?", OleDbType.VarWChar, 255) { Value = token };
                    var p2 = new OleDbParameter("?", OleDbType.Date) { Value = expiryDate };
                    var p3 = new OleDbParameter("?", OleDbType.Integer) { Value = userId };

                    stmt.Parameters.Add(p1);
                    stmt.Parameters.Add(p2);
                    stmt.Parameters.Add(p3);

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Clear user remember token (logout)
         * @param userId the user ID
         * @return true if update successful
         */
        public bool clearRememberToken(int userId)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET remember_token = NULL, token_expiry = NULL WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Find user by remember token
         * @param token the remember token
         * @return User object if found and token valid, null otherwise
         */
        public User findByRememberToken(string token)
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE remember_token = ? AND token_expiry > ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    var p1 = new OleDbParameter("?", OleDbType.VarWChar, 255) { Value = token };
                    var p2 = new OleDbParameter("?", OleDbType.Date) { Value = DateTime.Now };

                    stmt.Parameters.Add(p1);
                    stmt.Parameters.Add(p2);

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return mapResultSetToUser(rs);
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Sanitize string for database queries
         */
        private string sanitizeString(string str)
        {
            return (str != null) ? str.Trim() : "";
        }
    }
}
