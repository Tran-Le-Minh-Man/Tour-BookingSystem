using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using TourBookingSystem.Models;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Favorites entity
     */
    public class FavoritesDAO
    {
        private static readonly string TABLE_NAME = "favorites";
        
        /**
         * Custom exception for database operations
         */
        public class FavoritesDAOException : Exception
        {
            private readonly string operation;
            
            public FavoritesDAOException(string operation, string message, Exception cause) : base(message, cause)
            {
                this.operation = operation;
            }
            
            public string getOperation() { return operation; }
        }
        
        /**
         * Get all favorites for a user
         */
        public List<Dictionary<string, object>> getFavoritesByUserId(int userId)
        {
            string sql = "SELECT f.*, t.name as tour_name, t.destination as tour_destination, " +
                         "t.image_url as tour_image, t.price as tour_price, t.duration as tour_duration " +
                         "FROM " + TABLE_NAME + " f " +
                         "LEFT JOIN tours t ON f.tour_id = t.id " +
                         "WHERE f.user_id = ? " +
                         "ORDER BY f.id DESC";
            
            List<Dictionary<string, object>> favorites = new List<Dictionary<string, object>>();
            
            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.AddWithValue("?", userId);
                    
                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            Dictionary<string, object> fav = new Dictionary<string, object>();
                            fav["id"] = Convert.ToInt32(rs["id"]);
                            fav["user_id"] = Convert.ToInt32(rs["user_id"]);
                            fav["tour_id"] = Convert.ToInt32(rs["tour_id"]);
                            fav["created_at"] = rs["created_at"] != DBNull.Value ? Convert.ToDateTime(rs["created_at"]) : DateTime.MinValue;
                            fav["tour_name"] = rs["tour_name"] != DBNull.Value ? rs["tour_name"].ToString() : null;
                            fav["tour_destination"] = rs["tour_destination"] != DBNull.Value ? rs["tour_destination"].ToString() : null;
                            fav["tour_image"] = rs["tour_image"] != DBNull.Value ? rs["tour_image"].ToString() : null;
                            fav["tour_price"] = rs["tour_price"] != DBNull.Value ? Convert.ToDecimal(rs["tour_price"]) : 0;
                            fav["tour_duration"] = rs["tour_duration"] != DBNull.Value ? Convert.ToInt32(rs["tour_duration"]) : 0;
                            favorites.Add(fav);
                        }
                    }
                }
            }
            
            return favorites;
        }
        
        /**
         * Check if tour is in favorites
         */
        public bool isFavorite(int userId, int tourId)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE user_id = ? AND tour_id = ?";
            
            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.AddWithValue("?", userId);
                    stmt.Parameters.AddWithValue("?", tourId);
                    
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
         * Add tour to favorites
         */
        public bool addFavorite(int userId, int tourId)
        {
            // Check if already exists
            if (isFavorite(userId, tourId))
            {
                return true; // Already in favorites
            }
            
            string sql = "INSERT INTO " + TABLE_NAME + " (user_id, tour_id, created_at) VALUES (?, ?, ?)";
            
            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.AddWithValue("?", userId);
                    stmt.Parameters.AddWithValue("?", tourId);
                    stmt.Parameters.AddWithValue("?", DateTime.Now);
                    
                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        
        /**
         * Remove tour from favorites
         */
        public bool removeFavorite(int userId, int tourId)
        {
            string sql = "DELETE FROM " + TABLE_NAME + " WHERE user_id = ? AND tour_id = ?";
            
            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.AddWithValue("?", userId);
                    stmt.Parameters.AddWithValue("?", tourId);
                    
                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        
        /**
         * Get favorite count for user
         */
        public int countByUserId(int userId)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE user_id = ?";
            
            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.AddWithValue("?", userId);
                    
                    object result = stmt.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            
            return 0;
        }
    }
}
