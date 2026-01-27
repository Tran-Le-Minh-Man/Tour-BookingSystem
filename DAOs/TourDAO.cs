using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TourBookingSystem.Models;
using TourBookingSystem.Database;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Tour entity using LINQ
     */
    public class TourDAO
    {
        private readonly ApplicationDbContext _context;

        public TourDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        // Default constructor for compatibility (not recommended with DI)
        public TourDAO()
        {
        }

        /**
         * Get all tours
         * @return list of all tours
         */
        public List<Tour> getAllTours()
        {
            return _context.Tours.OrderByDescending(t => t.TourId).ToList();
        }

        /**
         * Get tours by status
         * @param status the status to filter (ACTIVE, INACTIVE)
         * @return list of tours with specified status
         */
        public List<Tour> getToursByStatus(string status)
        {
            return _context.Tours
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.TourId)
                .ToList();
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
            return _context.Tours.FirstOrDefault(t => t.TourId == id);
        }

        /**
         * Search tours by name or destination
         * @param keyword the search keyword
         * @return list of matching tours
         */
        public List<Tour> searchTours(string keyword)
        {
            return _context.Tours
                .Where(t => (t.Name != null && t.Name.Contains(keyword)) ||
                            (t.Description != null && t.Description.Contains(keyword)) ||
                            (t.Destination != null && t.Destination.Contains(keyword)))
                .OrderByDescending(t => t.TourId)
                .ToList();
        }

        /**
         * Insert a new tour
         * @param tour the tour to insert
         * @return true if insertion successful, false otherwise
         */
        public bool insert(Tour tour)
        {
            try
            {
                _context.Tours.Add(tour);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting tour: " + ex.Message);
                return false;
            }
        }

        /**
         * Update an existing tour
         * @param tour the tour to update
         * @return true if update successful, false otherwise
         */
        public bool update(Tour tour)
        {
            try
            {
                _context.Tours.Update(tour);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating tour: " + ex.Message);
                return false;
            }
        }

        /**
         * Delete a tour by ID
         * @param id the tour ID to delete
         * @return true if deletion successful, false otherwise
         */
        public bool delete(int id)
        {
            try
            {
                var tour = _context.Tours.FirstOrDefault(t => t.TourId == id);
                if (tour != null)
                {
                    _context.Tours.Remove(tour);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting tour: " + ex.Message);
                return false;
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
            try
            {
                var tour = _context.Tours.FirstOrDefault(t => t.TourId == id);
                if (tour != null)
                {
                    tour.Status = status;
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating tour status: " + ex.Message);
                return false;
            }
        }

        /**
         * Get tour count by status
         * @param status the status to count
         * @return number of tours with specified status
         */
        public int countByStatus(string status)
        {
            return _context.Tours.Count(t => t.Status == status);
        }

        /**
         * Get total tour count
         * @return total number of tours
         */
        public int getTotalCount()
        {
            return _context.Tours.Count();
        }

        /**
         * Get filtered tours based on various criteria including keyword search
         */
        public List<Tour> getFilteredTours(string keyword, string destination, string minPrice,
                                            string maxPrice, string duration, string departureDate, string sortBy = "")
        {
            var query = _context.Tours.Where(t => t.Status == "ACTIVE");

            if (!string.IsNullOrEmpty(keyword))
            {
                string trimmedKeyword = keyword.Trim();
                query = query.Where(t => (t.Name != null && t.Name.Contains(trimmedKeyword)) ||
                                         (t.Description != null && t.Description.Contains(trimmedKeyword)) ||
                                         (t.Destination != null && t.Destination.Contains(trimmedKeyword)));
            }

            if (!string.IsNullOrEmpty(destination) && destination != "all")
            {
                string trimmedDest = destination.Trim();
                query = query.Where(t => t.Destination != null && t.Destination.Contains(trimmedDest));
            }

            if (decimal.TryParse(minPrice, out decimal minP))
            {
                query = query.Where(t => t.Price >= minP);
            }

            if (decimal.TryParse(maxPrice, out decimal maxP))
            {
                query = query.Where(t => t.Price <= maxP);
            }

            if (int.TryParse(duration, out int dur) && duration != "all")
            {
                if (dur == 7) // "Above 5 days" case in original code
                {
                    query = query.Where(t => t.Duration > 5);
                }
                else
                {
                    query = query.Where(t => t.Duration == dur);
                }
            }

            if (DateTime.TryParse(departureDate, out DateTime departure))
            {
                DateTime dateOnly = departure.Date;
                query = query.Where(t => t.DepartureDate >= dateOnly);
            }

            // Handle Sorting
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(t => t.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(t => t.Price);
                    break;
                case "date_asc":
                    query = query.OrderBy(t => t.DepartureDate);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(t => t.DepartureDate);
                    break;
                case "duration_asc":
                    query = query.OrderBy(t => t.Duration);
                    break;
                case "duration_desc":
                    query = query.OrderByDescending(t => t.Duration);
                    break;
                default:
                    query = query.OrderByDescending(t => t.TourId);
                    break;
            }

            return query.ToList();
        }

        /**
         * Get all unique destinations
         * @return list of unique destination names
         */
        public List<string> getAllDestinations()
        {
            return _context.Tours
                .Where(t => t.Status == "ACTIVE")
                .Select(t => t.Destination)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        /**
         * Get random tours for featured section
         * @param limit maximum number of tours to return
         * @return list of random tours
         */
        public List<Tour> getRandomTours(int limit)
        {
            // Shuffling in memory as per original logic because Access doesn't support easy RAND()
            return _context.Tours
                .Where(t => t.Status == "ACTIVE")
                .ToList()
                .OrderBy(x => Guid.NewGuid())
                .Take(limit)
                .ToList();
        }

        /**
         * Get related tours by destination (excluding current tour)
         */
        public List<Tour> getRelatedTours(string destination, int currentTourId, int limit)
        {
            if (string.IsNullOrEmpty(destination)) return new List<Tour>();

            string trimmedDest = destination.Trim();
            return _context.Tours
                .Where(t => t.Status == "ACTIVE" && t.TourId != currentTourId && t.Destination != null && t.Destination.Contains(trimmedDest))
                .OrderByDescending(t => t.TourId)
                .Take(limit)
                .ToList();
        }
    }
}
