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
        private static readonly string TABLE_NAME = "[user_favorites]";

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
            string sql = "SELECT f.favorite_id, f.user_id, f.tour_id, f.created_at, " +
                         "t.name as tour_name, t.destination as tour_destination, " +
                         "t.image_url as tour_image, t.price as tour_price, t.duration as tour_duration " +
                         "FROM " + TABLE_NAME + " f " +
                         "LEFT JOIN [tours] t ON f.tour_id = t.id " +
                         "WHERE f.user_id = ? " +
                         "ORDER BY f.favorite_id DESC";

            List<Dictionary<string, object>> favorites = new List<Dictionary<string, object>>();

            try
            {
                using (OleDbConnection conn = DBConnection.getConnection())
                {
                    using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                    {
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                        using (OleDbDataReader rs = stmt.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                try
                                {
                                    Dictionary<string, object> fav = new Dictionary<string, object>();
                                    fav["id"] = rs["favorite_id"] != DBNull.Value ? Convert.ToInt32(rs["favorite_id"]) : 0;
                                    fav["user_id"] = rs["user_id"] != DBNull.Value ? Convert.ToInt32(rs["user_id"]) : 0;
                                    fav["tour_id"] = rs["tour_id"] != DBNull.Value ? Convert.ToInt32(rs["tour_id"]) : 0;
                                    fav["created_at"] = rs["created_at"] != DBNull.Value ? Convert.ToDateTime(rs["created_at"]) : DateTime.MinValue;
                                    fav["tour_name"] = rs["tour_name"] != DBNull.Value ? rs["tour_name"].ToString() : "";
                                    fav["tour_destination"] = rs["tour_destination"] != DBNull.Value ? rs["tour_destination"].ToString() : "";
                                    fav["tour_image"] = rs["tour_image"] != DBNull.Value ? rs["tour_image"].ToString() : "";
                                    fav["tour_price"] = rs["tour_price"] != DBNull.Value ? Convert.ToDecimal(rs["tour_price"]) : 0m;
                                    fav["tour_duration"] = rs["tour_duration"] != DBNull.Value ? Convert.ToInt32(rs["tour_duration"]) : 0;
                                    favorites.Add(fav);
                                }
                                catch (Exception inner)
                                {
                                    Console.WriteLine("Error mapping favorite row: " + inner.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.getFavoritesByUserId error: " + e.Message);
                throw new FavoritesDAOException("getFavoritesByUserId", "Error retrieving favorites", e);
            }

            return favorites;
        }

        /**
         * Check if tour is in favorites
         */
        public bool isFavorite(int userId, int tourId)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE user_id = ? AND tour_id = ?";

            try
            {
                using (OleDbConnection conn = DBConnection.getConnection())
                {
                    using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                    {
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tourId });

                        object result = stmt.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result) > 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.isFavorite error: " + e.Message);
            }

            return false;
        }

        /**
         * Add tour to favorites
         */
        public bool addFavorite(int userId, int tourId)
        {
            try
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
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tourId });
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = DateTime.Now });

                        int rowsAffected = stmt.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.addFavorite error: " + e.Message);
                throw new FavoritesDAOException("addFavorite", "Error adding favorite", e);
            }
        }

        /**
         * Remove tour from favorites
         */
        public bool removeFavorite(int userId, int tourId)
        {
            string sql = "DELETE FROM " + TABLE_NAME + " WHERE user_id = ? AND tour_id = ?";

            try
            {
                using (OleDbConnection conn = DBConnection.getConnection())
                {
                    using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                    {
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tourId });

                        int rowsAffected = stmt.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.removeFavorite error: " + e.Message);
                throw new FavoritesDAOException("removeFavorite", "Error removing favorite", e);
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
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

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
