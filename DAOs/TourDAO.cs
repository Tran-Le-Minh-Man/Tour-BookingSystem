using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using TourBookingSystem.Models;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Tour entity
     */
    public class TourDAO
    {
        private static readonly string TABLE_NAME = "[tours]";

        /**
         * Custom exception for database operations
         */
        public class TourDAOException : Exception
        {
            private readonly string operation;

            public TourDAOException(string operation, string message, Exception cause) : base(message, cause)
            {
                this.operation = operation;
            }

            public string getOperation() { return operation; }
        }

        /**
         * Get all tours
         * @return list of all tours
         */
        public List<Tour> getAllTours()
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " ORDER BY id DESC";
            List<Tour> tours = new List<Tour>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                using (OleDbDataReader rs = stmt.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        tours.Add(mapResultSetToTour(rs));
                    }
                }
            }

            return tours;
        }

        /**
         * Get tours by status
         * @param status the status to filter (ACTIVE, INACTIVE)
         * @return list of tours with specified status
         */
        public List<Tour> getToursByStatus(string status)
        {
            string sql = "SELECT * FROM " + TABLE_NAME + " WHERE status = ? ORDER BY id DESC";
            List<Tour> tours = new List<Tour>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            tours.Add(mapResultSetToTour(rs));
                        }
                    }
                }
            }

            return tours;
        }

        /**
         * Get active tours for customers
         * @return list of active tours
         */
        public List<Tour> getActiveTours()
        {
            return getToursByStatus("ACTIVE");
        }

        /**
         * Find tour by ID
         * @param id the tour ID
         * @return Tour object if found, null otherwise
         */
        public Tour findById(int id)
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
                            return mapResultSetToTour(rs);
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Search tours by name or destination
         * @param keyword the search keyword
         * @return list of matching tours
         */
        public List<Tour> searchTours(string keyword)
        {
            string sql = "SELECT * FROM " + TABLE_NAME +
                         " WHERE name LIKE ? OR description LIKE ? OR destination LIKE ? " +
                         "ORDER BY id DESC";
            List<Tour> tours = new List<Tour>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    string searchPattern = "%" + keyword + "%";
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            tours.Add(mapResultSetToTour(rs));
                        }
                    }
                }
            }

            return tours;
        }

        /**
         * Insert a new tour
         * @param tour the tour to insert
         * @return true if insertion successful, false otherwise
         */
        public bool insert(Tour tour)
        {
            string sql = "INSERT INTO " + TABLE_NAME +
                         " (name, description, destination, departure_date, duration, price, " +
                         "max_participants, current_participants, image_url, status) " +
                         "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getName()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getDescription()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getDestination()) });

                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = (object)tour.getDepartureDate() ?? DBNull.Value });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getDuration() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Decimal) { Value = tour.getPrice() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getMaxParticipants() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getCurrentParticipants() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getImageUrl()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = tour.getStatus() != null ? tour.getStatus() : "ACTIVE" });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Update an existing tour
         * @param tour the tour to update
         * @return true if update successful, false otherwise
         */
        public bool update(Tour tour)
        {
            string sql = "UPDATE " + TABLE_NAME +
                         " SET name = ?, description = ?, destination = ?, " +
                         "departure_date = ?, duration = ?, price = ?, " +
                         "max_participants = ?, current_participants = ?, " +
                         "image_url = ?, status = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getName()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getDescription()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getDestination()) });

                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = (object)tour.getDepartureDate() ?? DBNull.Value });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getDuration() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Decimal) { Value = tour.getPrice() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getMaxParticipants() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getCurrentParticipants() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sanitizeString(tour.getImageUrl()) });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = tour.getStatus() });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = tour.getTourId() });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Delete a tour by ID
         * @param id the tour ID to delete
         * @return true if deletion successful, false otherwise
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
         * Update tour status
         * @param id the tour ID
         * @param status the new status
         * @return true if update successful
         */
        public bool updateStatus(int id, string status)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET status = ? WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    int rowsAffected = stmt.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        /**
         * Get tour count by status
         * @param status the status to count
         * @return number of tours with specified status
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
         * Get total tour count
         * @return total number of tours
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
         * Get filtered tours based on various criteria including keyword search
         * @param keyword the keyword to search in tour name and description (null for all)
         * @param destination the destination to filter (null for all)
         * @param minPrice the minimum price (null for no minimum)
         * @param maxPrice the maximum price (null for no maximum)
         * @param duration the duration in days (null for all)
         * @param departureDate the departure date to filter (null for all)
         * @return list of filtered tours
         */
        public List<Tour> getFilteredTours(string keyword, string destination, string minPrice,
                                            string maxPrice, string duration, string departureDate, string sortBy = "")
        {
            List<string> conditions = new List<string>();
            List<OleDbParameter> parameters = new List<OleDbParameter>();

            // Filter by keyword (search in name and description)
            if (!string.IsNullOrEmpty(keyword) && !keyword.Trim().Equals(""))
            {
                conditions.Add("(name LIKE ? OR description LIKE ? OR destination LIKE ?)");
                string searchPattern = "%" + keyword.Trim() + "%";
                parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
                parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = searchPattern });
            }

            // Filter by destination
            if (!string.IsNullOrEmpty(destination) && !destination.Trim().Equals("") && !destination.Equals("all"))
            {
                conditions.Add("destination LIKE ?");
                parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = "%" + destination.Trim() + "%" });
            }

            // Filter by minimum price
            if (!string.IsNullOrEmpty(minPrice) && !minPrice.Trim().Equals(""))
            {
                try
                {
                    decimal minPriceValue = decimal.Parse(minPrice.Trim());
                    conditions.Add("price >= ?");
                    parameters.Add(new OleDbParameter("?", OleDbType.Decimal) { Value = minPriceValue });
                }
                catch (FormatException)
                {
                    // Ignore invalid price values
                }
            }

            // Filter by maximum price
            if (!string.IsNullOrEmpty(maxPrice) && !maxPrice.Trim().Equals(""))
            {
                try
                {
                    decimal maxPriceValue = decimal.Parse(maxPrice.Trim());
                    conditions.Add("price <= ?");
                    parameters.Add(new OleDbParameter("?", OleDbType.Decimal) { Value = maxPriceValue });
                }
                catch (FormatException)
                {
                    // Ignore invalid price values
                }
            }

            // Filter by duration
            if (!string.IsNullOrEmpty(duration) && !duration.Trim().Equals("") && !duration.Equals("all"))
            {
                try
                {
                    int durationValue = int.Parse(duration.Trim());
                    if (durationValue == 7) // "Above 5 days" case
                    {
                        conditions.Add("duration > 5");
                    }
                    else
                    {
                        conditions.Add("duration = ?");
                        parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = durationValue });
                    }
                }
                catch (FormatException)
                {
                    // Ignore invalid duration values
                }
            }

            // Filter by departure date
            if (!string.IsNullOrEmpty(departureDate) && !departureDate.Trim().Equals(""))
            {
                try
                {
                    DateTime dateValue = DateTime.Parse(departureDate.Trim());
                    conditions.Add("departure_date >= ?");
                    parameters.Add(new OleDbParameter("?", OleDbType.Date) { Value = dateValue.Date });
                }
                catch (FormatException)
                {
                    // Ignore invalid date format
                }
            }

            // Build SQL query
            string sql = "SELECT * FROM " + TABLE_NAME;
            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
                sql += " AND status = 'ACTIVE'";
            }
            else
            {
                sql += " WHERE status = 'ACTIVE'";
            }

            // Handle Sorting
            string orderBy = "id DESC"; // Default
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "price_asc":
                        orderBy = "price ASC";
                        break;
                    case "price_desc":
                        orderBy = "price DESC";
                        break;
                    case "date_asc":
                        orderBy = "departure_date ASC";
                        break;
                    case "date_desc":
                        orderBy = "departure_date DESC";
                        break;
                    case "duration_asc":
                        orderBy = "duration ASC";
                        break;
                    case "duration_desc":
                        orderBy = "duration DESC";
                        break;
                }
            }
            sql += " ORDER BY " + orderBy;

            List<Tour> tours = new List<Tour>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    foreach (var param in parameters)
                    {
                        stmt.Parameters.Add(param);
                    }

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            tours.Add(mapResultSetToTour(rs));
                        }
                    }
                }
            }

            return tours;
        }

        /**
         * Get all unique destinations
         * @return list of unique destination names
         */
        public List<string> getAllDestinations()
        {
            string sql = "SELECT DISTINCT destination FROM " + TABLE_NAME + " WHERE status = 'ACTIVE' ORDER BY destination";
            List<string> destinations = new List<string>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                using (OleDbDataReader rs = stmt.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        string dest = rs["destination"].ToString();
                        if (!string.IsNullOrEmpty(dest) && !dest.Trim().Equals(""))
                        {
                            destinations.Add(dest.Trim());
                        }
                    }
                }
            }

            return destinations;
        }

        /**
         * Get random tours for featured section
         * @param limit maximum number of tours to return
         * @return list of random tours
         */
        public List<Tour> getRandomTours(int limit)
        {
            // Note: Access doesn't support ORDER BY RAND() like MySQL
            // We'll get all active tours and shuffle in memory
            List<Tour> allTours = getActiveTours();

            // Shuffle
            var random = new Random();
            int n = allTours.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = allTours[i];
                allTours[i] = allTours[j];
                allTours[j] = temp;
            }

            // Return limited results
            return allTours.Take(limit).ToList();
        }

        /**
         * Get related tours by destination (excluding current tour)
         * @param destination the destination to match
         * @param currentTourId the current tour ID to exclude
         * @param limit maximum number of related tours to return
         * @return list of related tours
         */
        public List<Tour> getRelatedTours(string destination, int currentTourId, int limit)
        {
            if (string.IsNullOrEmpty(destination) || destination.Trim().Equals(""))
            {
                return new List<Tour>();
            }

            string sql = "SELECT TOP " + limit + " * FROM " + TABLE_NAME +
                         " WHERE destination LIKE ? AND id <> ? AND status = 'ACTIVE' " +
                         "ORDER BY id DESC";
            List<Tour> tours = new List<Tour>();

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                {
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = "%" + destination.Trim() + "%" });
                    stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = currentTourId });

                    using (OleDbDataReader rs = stmt.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            tours.Add(mapResultSetToTour(rs));
                        }
                    }
                }
            }

            return tours;
        }

        /**
         * Map ResultSet to Tour object
         */
        private Tour mapResultSetToTour(OleDbDataReader rs)
        {
            Tour tour = new Tour();
            tour.setTourId(rs["id"] != DBNull.Value ? Convert.ToInt32(rs["id"]) : 0);
            tour.setName(rs["name"] != DBNull.Value ? rs["name"].ToString() : "");
            tour.setDescription(rs["description"] != DBNull.Value ? rs["description"].ToString() : "");
            tour.setDestination(rs["destination"] != DBNull.Value ? rs["destination"].ToString() : "");

            if (rs["departure_date"] != DBNull.Value)
            {
                tour.setDepartureDate(Convert.ToDateTime(rs["departure_date"]));
            }

            tour.setDuration(rs["duration"] != DBNull.Value ? Convert.ToInt32(rs["duration"]) : 0);
            tour.setPrice(rs["price"] != DBNull.Value ? Convert.ToDecimal(rs["price"]) : 0m);
            tour.setMaxParticipants(rs["max_participants"] != DBNull.Value ? Convert.ToInt32(rs["max_participants"]) : 0);
            tour.setCurrentParticipants(rs["current_participants"] != DBNull.Value ? Convert.ToInt32(rs["current_participants"]) : 0);
            tour.setImageUrl(rs["image_url"] != DBNull.Value ? rs["image_url"].ToString() : "");
            tour.setStatus(rs["status"] != DBNull.Value ? rs["status"].ToString() : "ACTIVE");

            return tour;
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
