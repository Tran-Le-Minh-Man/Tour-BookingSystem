using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourBookingSystem.Models;
using TourBookingSystem.DAOs;

namespace TourBookingSystem.Controllers
{
    /**
     * Controller for home page and tour listing
     */
    public class HomeController : Controller
    {
        private TourDAO tourDAO;
        private BookingDAO bookingDAO;
        
        public HomeController()
        {
            tourDAO = new TourDAO();
            bookingDAO = new BookingDAO();
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
                                       string maxPrice, string duration, string departureDate)
        {
            try
            {
                // Get filtered tours
                List<Tour> tours = tourDAO.getFilteredTours(keyword, destination, minPrice, maxPrice, duration, departureDate);
                ViewData["tours"] = tours;
                
                // Get all destinations for filter
                List<string> destinations = tourDAO.getAllDestinations();
                ViewData["destinations"] = destinations;
                
                // Pass filter parameters
                ViewData["keyword"] = keyword;
                ViewData["destination"] = destination;
                ViewData["minPrice"] = minPrice;
                ViewData["maxPrice"] = maxPrice;
                ViewData["duration"] = duration;
                ViewData["departureDate"] = departureDate;
                
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
                ViewData["tour"] = tour;
                
                // Get related tours (same destination)
                if (tour != null)
                {
                    List<Tour> relatedTours = tourDAO.getRelatedTours(tour.getDestination(), id, 4);
                    ViewData["relatedTours"] = relatedTours;
                }
                else
                {
                    ViewData["relatedTours"] = new List<Tour>();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("TourDetail error: " + e.Message);
                ViewData["tour"] = null;
                ViewData["relatedTours"] = new List<Tour>();
            }
            
            // Get user info from session
            ViewData["userId"] = HttpContext.Session.GetInt32("userId");
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");
            
            return View();
        }
    }
}
