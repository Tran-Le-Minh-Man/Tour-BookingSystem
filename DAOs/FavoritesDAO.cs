using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TourBookingSystem.Models;
using TourBookingSystem.Database;

namespace TourBookingSystem.DAOs
{
    /**
     * Data Access Object for Favorites entity using LINQ
     */
    public class FavoritesDAO
    {
        private readonly ApplicationDbContext _context;

        public FavoritesDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public FavoritesDAO()
        {
        }

        /**
         * Get all favorites for a user
         */
        public List<Dictionary<string, object>> getFavoritesByUserId(int userId)
        {
            try
            {
                var favs = _context.Favorites
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.Id)
                    .ToList();

                var tourIds = favs.Select(f => f.TourId).ToList();
                var tours = _context.Tours.Where(t => tourIds.Contains(t.TourId)).ToDictionary(t => t.TourId);

                var result = new List<Dictionary<string, object>>();

                foreach (var f in favs)
                {
                    var dict = new Dictionary<string, object>();
                    dict["id"] = f.Id;
                    dict["user_id"] = f.UserId;
                    dict["tour_id"] = f.TourId;
                    dict["created_at"] = f.CreatedAt;

                    if (tours.TryGetValue(f.TourId, out var tour))
                    {
                        dict["tour_name"] = tour.Name ?? "";
                        dict["tour_destination"] = tour.Destination ?? "";
                        dict["tour_image"] = tour.ImageUrl ?? "";
                        dict["tour_price"] = tour.Price;
                        dict["tour_duration"] = tour.Duration;
                    }

                    result.Add(dict);
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.getFavoritesByUserId error: " + e.Message);
                return new List<Dictionary<string, object>>();
            }
        }

        /**
         * Check if tour is in favorites
         */
        public bool isFavorite(int userId, int tourId)
        {
            return _context.Favorites.Any(f => f.UserId == userId && f.TourId == tourId);
        }

        /**
         * Add tour to favorites
         */
        public bool addFavorite(int userId, int tourId)
        {
            try
            {
                if (isFavorite(userId, tourId)) return true;

                var fav = new ApplicationDbContext.Favorite
                {
                    UserId = userId,
                    TourId = tourId,
                    CreatedAt = DateTime.Now
                };

                _context.Favorites.Add(fav);
                return _context.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.addFavorite error: " + e.Message);
                return false;
            }
        }

        /**
         * Remove tour from favorites
         */
        public bool removeFavorite(int userId, int tourId)
        {
            try
            {
                var fav = _context.Favorites.FirstOrDefault(f => f.UserId == userId && f.TourId == tourId);
                if (fav != null)
                {
                    _context.Favorites.Remove(fav);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesDAO.removeFavorite error: " + e.Message);
                return false;
            }
        }

        /**
         * Get favorite count for user
         */
        public int countByUserId(int userId)
        {
            return _context.Favorites.Count(f => f.UserId == userId);
        }
    }
}
