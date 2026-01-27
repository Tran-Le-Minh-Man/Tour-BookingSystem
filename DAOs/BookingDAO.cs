using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using TourBookingSystem.Models;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Booking entity
     */
    public class BookingDAO
    {
        private static readonly string TABLE_NAME = "[bookings]";

        /**
         * Custom exception for database operations
         */
        public class BookingDAOException : Exception
        {
            private readonly string operation;

            public BookingDAOException(string operation, string message, Exception cause) : base(message, cause)
            {
                this.operation = operation;
            }

            public string getOperation() { return operation; }
        }

        /**
         * Get all bookings with user and tour info
         */
        public List<Booking> getAllBookings()
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "ORDER BY b.id DESC";
            List<Booking> bookings = new List<Booking>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                using (OleDbDataReader rs = stmt.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        bookings.Add(mapResultSetToBooking(rs));
                    }
                }
            }

            return bookings;
        }

        /**
         * Search bookings by keyword (user name, email, tour name, destination)
         */
        public List<Booking> searchBookings(string keyword)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE u.full_name LIKE ? OR u.email LIKE ? " +
                         "OR t.name LIKE ? OR t.destination LIKE ? " +
                         "ORDER BY b.id DESC";

            List<Booking> bookings = new List<Booking>();
            string searchPattern = "%" + keyword + "%";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            bookings.Add(mapResultSetToBooking(rs));
                        }
                    }
                }
            }

            return bookings;
        }

        /**
         * Get bookings by status
         */
        public List<Booking> getBookingsByStatus(string status)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE b.status = ? " +
                         "ORDER BY b.id DESC";

            List<Booking> bookings = new List<Booking>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            bookings.Add(mapResultSetToBooking(rs));
                        }
                    }
                }
            }

            return bookings;
        }

        /**
         * Filter bookings by both status and search keyword
         */
        public List<Booking> filterBookingsByStatusAndSearch(string status, string keyword)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE b.status = ? " +
                         "AND (u.full_name LIKE ? OR u.email LIKE ? " +
                         "OR t.name LIKE ? OR t.destination LIKE ?) " +
                         "ORDER BY b.id DESC";

            List<Booking> bookings = new List<Booking>();
            string searchPattern = "%" + keyword + "%";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            bookings.Add(mapResultSetToBooking(rs));
                        }
                    }
                }
            }

            return bookings;
        }

        /**
         * Find booking by ID
         */
        public Booking findById(int id)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE b.id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return mapResultSetToBooking(rs);
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Get bookings by user ID
         */
        public List<Booking> getBookingsByUserId(int userId)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE b.user_id = ? " +
                         "ORDER BY b.id DESC";

            List<Booking> bookings = new List<Booking>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            bookings.Add(mapResultSetToBooking(rs));
                        }
                    }
                }
            }

            return bookings;
        }

        /**
         * Get bookings by user ID and status
         */
        public List<Booking> getBookingsByUserIdAndStatus(int userId, string status)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                         "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                         "FROM (" + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id) " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "WHERE b.user_id = ? AND b.status = ? " +
                         "ORDER BY b.id DESC";

            List<Booking> bookings = new List<Booking>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = userId });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            bookings.Add(mapResultSetToBooking(rs));
                        }
                    }
                }
            }

            return bookings;
        }

        /**
         * Create a new booking
         */
        public int createBooking(Booking booking)
        {
            string sql = "INSERT INTO " + TABLE_NAME + " (user_id, tour_id, booking_date, status, num_participants, total_price, notes) " +
                         "VALUES (?, ?, ?, ?, ?, ?, ?)";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = booking.getUserId() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = booking.getTourId() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = DateTime.Now });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = booking.getStatus() != null ? booking.getStatus() : "PENDING" });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = booking.getNumParticipants() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Decimal) { Value = booking.getTotalPrice() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = booking.getNotes() != null ? booking.getNotes() : "" });

                    int rowsAffected = stmt.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Get the generated ID
                        string idSql = "SELECT @@IDENTITY";
                        using (OleDbCommand idStmt = new OleDbCommand(idSql, conn))
                        {
                            object result = idStmt.ExecuteScalar();
                            if (result != null)
                            {
                                return Convert.ToInt32(result);
                            }
                        }
                    }
                }
            }

            return 0;
        }

        /**
         * Get count of user bookings
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

        /**
         * Update booking status
         */
        public bool updateStatus(int id, string status)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET status = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status.ToUpper() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Confirm a booking
         */
        public bool confirmBooking(int id)
        {
            return updateStatus(id, "CONFIRMED");
        }

        /**
         * Cancel a booking
         */
        public bool cancelBooking(int id)
        {
            return updateStatus(id, "CANCELLED");
        }

        /**
         * Complete a booking
         */
        public bool completeBooking(int id)
        {
            return updateStatus(id, "COMPLETED");
        }

        /**
         * Delete a booking
         */
        public bool delete(int id)
        {
            string sql = "DELETE FROM " + TABLE_NAME + " WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Delete all bookings (Reset functionality)
         */
        public bool deleteAllBookings()
        {
            string sql = "DELETE FROM " + TABLE_NAME;

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /**
         * Get booking count by status
         */
        public int countByStatus(string status)
        {
            string sql = "SELECT COUNT(*) FROM " + TABLE_NAME + " WHERE status = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });

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
         * Get total booking count
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
         * Get total revenue from confirmed/completed bookings
         */
        public decimal getTotalRevenue()
        {
            string sql = "SELECT SUM(total_price) FROM " + TABLE_NAME +
                         " WHERE status IN ('CONFIRMED', 'COMPLETED')";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    object result = stmt.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                }
            }

            return 0;
        }

        /**
         * Get revenue by date range
         */
        public decimal getRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            string sql = "SELECT SUM(total_price) FROM " + TABLE_NAME +
                         " WHERE status IN ('CONFIRMED', 'COMPLETED') " +
                         " AND booking_date BETWEEN ? AND ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = startDate });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = endDate });

                    object result = stmt.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                }
            }

            return 0;
        }

        /**
         * Get recent bookings
         */
        public List<Booking> getRecentBookings(int limit)
        {
            string sql = "SELECT b.*, u.full_name as user_name, u.email as user_email, " +
                         "t.name as tour_name, t.destination as tour_destination " +
                         "FROM " + TABLE_NAME + " b " +
                         "LEFT JOIN [users] u ON b.user_id = u.id " +
                         "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                         "ORDER BY b.id DESC";

            // For MS Access, use TOP clause
            if (limit > 0)
            {
                sql = "SELECT TOP " + limit + " b.*, u.full_name as user_name, u.email as user_email, " +
                      "t.name as tour_name, t.destination as tour_destination, t.image_url as tour_image, " +
                      "t.departure_date as tour_departure, t.duration as tour_duration, t.price as tour_price " +
                      "FROM (" + TABLE_NAME + " b " +
                      "LEFT JOIN [users] u ON b.user_id = u.id) " +
                      "LEFT JOIN [tours] t ON b.tour_id = t.id " +
                      "ORDER BY b.id DESC";
            }

            List<Booking> bookings = new List<Booking>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                using (OleDbDataReader rs = stmt.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        bookings.Add(mapResultSetToBooking(rs));
                    }
                }
            }

            return bookings;
        }

        /**
         * Map ResultSet to Booking object
         */
        private Booking mapResultSetToBooking(OleDbDataReader rs)
        {
            Booking booking = new Booking();
            
            // Safe mapping for required fields
            booking.setBookingId(rs["id"] != DBNull.Value ? Convert.ToInt32(rs["id"]) : 0);
            booking.setUserId(rs["user_id"] != DBNull.Value ? Convert.ToInt32(rs["user_id"]) : 0);
            booking.setTourId(rs["tour_id"] != DBNull.Value ? Convert.ToInt32(rs["tour_id"]) : 0);

            if (rs["booking_date"] != DBNull.Value)
            {
                booking.setBookingDate(Convert.ToDateTime(rs["booking_date"]));
            }

            booking.setStatus(rs["status"] != DBNull.Value ? rs["status"].ToString() : "PENDING");
            booking.setNumParticipants(rs["num_participants"] != DBNull.Value ? Convert.ToInt32(rs["num_participants"]) : 0);

            if (rs["total_price"] != DBNull.Value)
            {
                booking.setTotalPrice(Convert.ToDecimal(rs["total_price"]));
            }
            else
            {
                booking.setTotalPrice(0m);
            }

            booking.setNotes(rs["notes"] != DBNull.Value ? rs["notes"].ToString() : "");

            // Additional display fields from JOINs
            booking.setUserName(getColumnValueSafe(rs, "user_name"));
            booking.setUserEmail(getColumnValueSafe(rs, "user_email"));
            booking.setTourName(getColumnValueSafe(rs, "tour_name"));
            booking.setTourDestination(getColumnValueSafe(rs, "tour_destination"));

            // Tour detail fields (via try-catch for flexible query support)
            try
            {
                booking.setTourImage(getColumnValueSafe(rs, "tour_image"));
                if (rs["tour_departure"] != DBNull.Value)
                    booking.setTourDeparture(Convert.ToDateTime(rs["tour_departure"]).ToString());
                if (rs["tour_duration"] != DBNull.Value)
                    booking.setTourDuration(Convert.ToInt32(rs["tour_duration"]));
                if (rs["tour_price"] != DBNull.Value)
                    booking.setTourPrice(Convert.ToDecimal(rs["tour_price"]));
            }
            catch { /* Ignore missing columns */ }

            return booking;
        }

        private string getColumnValueSafe(OleDbDataReader rs, string columnName)
        {
            try
            {
                int ordinal = rs.GetOrdinal(columnName);
                return rs.IsDBNull(ordinal) ? "" : rs.GetValue(ordinal).ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
