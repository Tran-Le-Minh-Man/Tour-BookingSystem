using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourBookingSystem.Models;
using TourBookingSystem.DAOs;
using TourBookingSystem.Database;
namespace TourBookingSystem.Controllers
{
    /**
     * Controller for home page and tour listing
     */
    public class HomeController : Controller
    {
        private readonly TourDAO tourDAO;
        private readonly BookingDAO bookingDAO;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
            tourDAO = new TourDAO(_context);
            bookingDAO = new BookingDAO(_context);
        }

        /**
         * GET: /Home/Index (HomePage)
         */
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // Get featured tours (random)
                List<Tour> featuredTours = tourDAO.getRandomTours(6);
                ViewData["featuredTours"] = featuredTours;

                // Get all destinations for filter
                List<string> destinations = tourDAO.getAllDestinations();
                ViewData["destinations"] = destinations;

            }
            catch (Exception e)
            {
                Console.WriteLine("Home Index error: " + e.Message);
                ViewData["featuredTours"] = new List<Tour>();
                ViewData["destinations"] = new List<string>();
            }

            // Get user info from session
            ViewData["userId"] = HttpContext.Session.GetInt32("userId");
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");

            return View();
        }

        /**
         * GET: /Home/TourList
         */
        [HttpGet]
        public IActionResult TourList(string keyword, string destination, string minPrice,
                                       string maxPrice, string duration, string departure_date, string sortBy)
        {
            try
            {
                // Get filtered tours with sortBy and departure_date
                List<Tour> tours = tourDAO.getFilteredTours(keyword, destination, minPrice, maxPrice, duration, departure_date, sortBy);
                ViewData["tours"] = tours;

                // Get all destinations for filter
                List<string> destinations = tourDAO.getAllDestinations();
                ViewData["destinations"] = destinations;

                // Pass filter parameters with correct keys for view
                ViewData["searchKeyword"] = keyword;
                ViewData["selectedDestination"] = destination;
                ViewData["departureDate"] = departure_date;
                ViewData["selectedSortBy"] = sortBy;

                // Convert numeric filters for typed view access
                if (decimal.TryParse(minPrice, out decimal minP)) ViewData["minPrice"] = minP;
                if (decimal.TryParse(maxPrice, out decimal maxP)) ViewData["maxPrice"] = maxP;
                if (int.TryParse(duration, out int dur)) ViewData["duration"] = dur;

            }
            catch (Exception e)
            {
                Console.WriteLine("TourList error: " + e.Message);
                ViewData["tours"] = new List<Tour>();
                ViewData["destinations"] = new List<string>();
            }

            // Get user info from session
            ViewData["userId"] = HttpContext.Session.GetInt32("userId");
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");

            return View();
        }

        /**
         * GET: /Home/TourDetail?id=xxx
         */
        [HttpGet]
        public IActionResult TourDetail(int id)
        {
            Tour tour = null;
            try
            {
                tour = tourDAO.findById(id);

                // Get related tours (same destination)
                if (tour != null)
                {
                    try
                    {
                        List<Tour> relatedTours = tourDAO.getRelatedTours(tour.getDestination(), id, 4);
                        ViewData["relatedTours"] = relatedTours;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading related tours: " + ex.Message);
                        ViewData["relatedTours"] = new List<Tour>();
                    }

                    try
                    {
                        // Check if tour is in user's favorites
                        int? userId = HttpContext.Session.GetInt32("userId");
                        if (userId.HasValue)
                        {
                            FavoritesDAO favoritesDAO = new FavoritesDAO(_context);
                            ViewData["isFavorite"] = favoritesDAO.isFavorite(userId.Value, id);
                        }
                        else
                        {
                            ViewData["isFavorite"] = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error checking favorites: " + ex.Message);
                        ViewData["isFavorite"] = false;
                    }
                }
                else
                {
                    ViewData["relatedTours"] = new List<Tour>();
                    ViewData["isFavorite"] = false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("TourDetail critical error (findById): " + e.Message);
                ViewData["relatedTours"] = new List<Tour>();
                ViewData["isFavorite"] = false;
                tour = null;
            }

            // Get user info from session
            ViewData["userId"] = HttpContext.Session.GetInt32("userId");
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");

            return View(tour);
        }
    }
}