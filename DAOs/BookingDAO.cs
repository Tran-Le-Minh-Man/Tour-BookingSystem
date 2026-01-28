using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TourBookingSystem.Models;
using TourBookingSystem.Database;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Booking entity using LINQ
     */
    public class BookingDAO
    {
        private readonly ApplicationDbContext _context;

        public BookingDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public BookingDAO()
        {
        }

        /**
         * Get all bookings with user and tour info
         */
        public List<Booking> getAllBookings()
        {
            var bookings = _context.Bookings.OrderByDescending(b => b.BookingId).ToList();
            FillDisplayInfo(bookings);
            return bookings;
        }

        private void FillDisplayInfo(List<Booking> bookings)
        {
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();
            var tourIds = bookings.Select(b => b.TourId).Distinct().ToList();

            var users = _context.Users.Where(u => userIds.Contains(u.UserId)).ToDictionary(u => u.UserId);
            var tours = _context.Tours.Where(t => tourIds.Contains(t.TourId)).ToDictionary(t => t.TourId);

            foreach (var b in bookings)
            {
                if (users.TryGetValue(b.UserId, out var user))
                {
                    b.UserName = user.FullName;
                    b.UserEmail = user.Email;
                }
                if (tours.TryGetValue(b.TourId, out var tour))
                {
                    b.TourName = tour.Name;
                    b.TourDestination = tour.Destination;
                    b.TourImage = tour.ImageUrl;
                    b.TourDeparture = tour.DepartureDate?.ToString();
                    b.TourDuration = tour.Duration;
                    b.TourPrice = tour.Price;
                }
            }
        }

        /**
         * Search bookings by keyword
         */
        public List<Booking> searchBookings(string keyword)
        {
            string pattern = keyword.Trim();
            // Since we need to join for filtering, we'll do it in memory or with navigation properties if they existed
            // For now, let's get all and filter in memory to match original logic precisely
            var bookings = _context.Bookings.ToList();
            FillDisplayInfo(bookings);

            return bookings.Where(b =>
                (b.UserName != null && b.UserName.Contains(pattern)) ||
                (b.UserEmail != null && b.UserEmail.Contains(pattern)) ||
                (b.TourName != null && b.TourName.Contains(pattern)) ||
                (b.TourDestination != null && b.TourDestination.Contains(pattern)))
                .OrderByDescending(b => b.BookingId)
                .ToList();
        }

        /**
         * Get bookings by status
         */
        public List<Booking> getBookingsByStatus(string status)
        {
            var bookings = _context.Bookings
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.BookingId)
                .ToList();
            FillDisplayInfo(bookings);
            return bookings;
        }

        /**
         * Filter bookings by both status and search keyword
         */
        public List<Booking> filterBookingsByStatusAndSearch(string status, string keyword)
        {
            string pattern = keyword.Trim();
            var bookings = _context.Bookings
                .Where(b => b.Status == status)
                .ToList();
            FillDisplayInfo(bookings);

            return bookings.Where(b =>
                (b.UserName != null && b.UserName.Contains(pattern)) ||
                (b.UserEmail != null && b.UserEmail.Contains(pattern)) ||
                (b.TourName != null && b.TourName.Contains(pattern)) ||
                (b.TourDestination != null && b.TourDestination.Contains(pattern)))
                .OrderByDescending(b => b.BookingId)
                .ToList();
        }

        /**
         * Find booking by ID
         */
        public Booking findById(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking != null)
            {
                FillDisplayInfo(new List<Booking> { booking });
            }
            return booking;
        }

        /**
         * Get bookings by user ID
         */
        public List<Booking> getBookingsByUserId(int userId)
        {
            var bookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingId)
                .ToList();
            FillDisplayInfo(bookings);
            return bookings;
        }

        /**
         * Get bookings by user ID and status
         */
        public List<Booking> getBookingsByUserIdAndStatus(int userId, string status)
        {
            var bookings = _context.Bookings
                .Where(b => b.UserId == userId && b.Status == status)
                .OrderByDescending(b => b.BookingId)
                .ToList();
            FillDisplayInfo(bookings);
            return bookings;
        }

        /**
         * Create a new booking
         */
        public int createBooking(Booking booking)
        {
            try
            {
                if (booking.BookingDate == null) booking.BookingDate = DateTime.Now;
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return booking.BookingId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating booking: " + ex.Message);
                return 0;
            }
        }

        public int countByUserId(int userId)
        {
            return _context.Bookings.Count(b => b.UserId == userId);
        }

        /**
         * Update booking status
         */
        //public bool updateStatus(int id, string status)
        //{
        //    string sql = "UPDATE " + TABLE_NAME + " SET status = ? WHERE id = ?";

        //    using (OleDbConnection conn = DBConnection.getConnection())
        //    {
        //        using (OleDbCommand stmt = new OleDbCommand(sql, conn))
        //        {
        //            stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status.ToUpper() });
        //            stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

        //            int rowsAffected = stmt.ExecuteNonQuery();
        //            Console.WriteLine("Rows affected: " + rowsAffected);
        //            Console.WriteLine(">>> Using DB = " + conn.DataSource);
        //            return rowsAffected > 0;
        //        }
        //    }
        //}
        public bool updateStatus(int id, string status)
        {
            try
            {
                var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
                if (booking != null)
                {
                    booking.Status = status.ToUpper();
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating booking status: " + ex.Message);
                return false;
            }
        }

        /**
         * Confirm a booking
         */
       

        public bool confirmBooking(int id) => updateStatus(id, "CONFIRMED");
        public bool cancelBooking(int id) => updateStatus(id, "CANCELLED");
        public bool completeBooking(int id) => updateStatus(id, "COMPLETED");

        public bool delete(int id)
        {
            try
            {
                var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting booking: " + ex.Message);
                return false;
            }
        }

        public bool deleteAllBookings()
        {
            try
            {
                _context.Bookings.ExecuteDelete(); // EF Core 7+ feature
                return true;
            }
            catch
            {
                // Fallback for older EF Core if needed
                _context.Bookings.RemoveRange(_context.Bookings);
                return _context.SaveChanges() > 0;
            }
        }

        public int countByStatus(string status)
        {
            return _context.Bookings.Count(b => b.Status == status);
        }

        public int getTotalCount()
        {
            return _context.Bookings.Count();
        }

        public decimal getTotalRevenue()
        {
            return _context.Bookings
                .Where(b => b.Status == "CONFIRMED" || b.Status == "COMPLETED")
                .Sum(b => b.TotalPrice);
        }

        public decimal getRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Bookings
                .Where(b => (b.Status == "CONFIRMED" || b.Status == "COMPLETED") &&
                           b.BookingDate >= startDate && b.BookingDate <= endDate)
                .Sum(b => b.TotalPrice);
        }

        public List<Booking> getRecentBookings(int limit)
        {
            var bookings = _context.Bookings
                .OrderByDescending(b => b.BookingId)
                .Take(limit)
                .ToList();
            FillDisplayInfo(bookings);
            return bookings;
        }
    }
}
