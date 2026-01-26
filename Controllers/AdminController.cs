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
     * Controller for admin dashboard and management
     */
    public class AdminController : Controller
    {
        private UserDAO userDAO;
        private TourDAO tourDAO;
        private BookingDAO bookingDAO;
        
        public AdminController()
        {
            userDAO = new UserDAO();
            tourDAO = new TourDAO();
            bookingDAO = new BookingDAO();
        }
        
        /**
         * GET: /Admin
         */
        [HttpGet]
        public IActionResult Index()
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string role = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(role, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            
            try
            {
                // Get dashboard statistics
                ViewData["totalUsers"] = userDAO.getTotalCount();
                ViewData["totalTours"] = tourDAO.getTotalCount();
                ViewData["totalBookings"] = bookingDAO.getTotalCount();
                ViewData["totalRevenue"] = bookingDAO.getTotalRevenue();
                
                // Get recent bookings
                List<Booking> recentBookings = bookingDAO.getRecentBookings(10);
                ViewData["recentBookings"] = recentBookings;
                
                // Get user statistics
                ViewData["adminCount"] = userDAO.countByRole("ADMIN");
                ViewData["userCount"] = userDAO.countByRole("USER");
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Admin Index error: " + e.Message);
            }
            
            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = role;
            
            return View();
        }
        
        /**
         * GET: /Admin/Users
         */
        [HttpGet]
        public IActionResult Users(string search, string role)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            
            List<User> users = new List<User>();
            
            try
            {
                if (!string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(role))
                {
                    if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(role))
                    {
                        users = userDAO.filterUsersByRoleAndSearch(role, search);
                    }
                    else if (!string.IsNullOrEmpty(role))
                    {
                        users = userDAO.getUsersByRole(role);
                    }
                    else
                    {
                        users = userDAO.searchUsers(search);
                    }
                }
                else
                {
                    users = userDAO.getAllUsers();
                }
                
                ViewData["users"] = users;
                ViewData["search"] = search;
                ViewData["roleFilter"] = role;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Admin Users error: " + e.Message);
                ViewData["users"] = new List<User>();
            }
            
            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = sessionRole;
            
            return View();
        }
        
        /**
         * GET: /Admin/Tours
         */
        [HttpGet]
        public IActionResult Tours(string search, string status)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            
            List<Tour> tours = new List<Tour>();
            
            try
            {
                if (!string.IsNullOrEmpty(status))
                {
                    tours = tourDAO.getToursByStatus(status);
                }
                else if (!string.IsNullOrEmpty(search))
                {
                    tours = tourDAO.searchTours(search);
                }
                else
                {
                    tours = tourDAO.getAllTours();
                }
                
                ViewData["tours"] = tours;
                ViewData["search"] = search;
                ViewData["statusFilter"] = status;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Admin Tours error: " + e.Message);
                ViewData["tours"] = new List<Tour>();
            }
            
            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = sessionRole;
            
            return View();
        }
        
        /**
         * GET: /Admin/Bookings
         */
        [HttpGet]
        public IActionResult Bookings(string search, string status)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            
            List<Booking> bookings = new List<Booking>();
            
            try
            {
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(status))
                {
                    bookings = bookingDAO.filterBookingsByStatusAndSearch(status, search);
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    bookings = bookingDAO.getBookingsByStatus(status);
                }
                else if (!string.IsNullOrEmpty(search))
                {
                    bookings = bookingDAO.searchBookings(search);
                }
                else
                {
                    bookings = bookingDAO.getAllBookings();
                }
                
                ViewData["bookings"] = bookings;
                ViewData["search"] = search;
                ViewData["statusFilter"] = status;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Admin Bookings error: " + e.Message);
                ViewData["bookings"] = new List<Booking>();
            }
            
            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = sessionRole;
            
            return View();
        }
        
        /**
         * POST: /Admin/UpdateBookingStatus
         */
        [HttpPost]
        public IActionResult UpdateBookingStatus(int bookingId, string status)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            try
            {
                bool success = bookingDAO.updateStatus(bookingId, status);
                
                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Cập nhật thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateBookingStatus error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        /**
         * POST: /Admin/DeleteBooking
         */
        [HttpPost]
        public IActionResult DeleteBooking(int bookingId)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            try
            {
                bool success = bookingDAO.delete(bookingId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Xóa thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Xóa thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteBooking error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        /**
         * POST: /Admin/DeleteUser
         */
        [HttpPost]
        public IActionResult DeleteUser(int userIdToDelete)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            // Prevent deleting yourself
            if (userIdToDelete == userId.Value)
            {
                return Json(new { success = false, message = "Bạn không thể xóa chính mình" });
            }
            
            try
            {
                bool success = userDAO.delete(userIdToDelete);
                
                if (success)
                {
                    return Json(new { success = true, message = "Xóa người dùng thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Xóa người dùng thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteUser error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        /**
         * POST: /Admin/UpdateUserRole
         */
        [HttpPost]
        public IActionResult UpdateUserRole(int userIdToUpdate, string role)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            try
            {
                bool success = userDAO.updateRole(userIdToUpdate, role);
                
                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật quyền thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Cập nhật quyền thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateUserRole error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        /**
         * POST: /Admin/DeleteTour
         */
        [HttpPost]
        public IActionResult DeleteTour(int tourId)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            try
            {
                bool success = tourDAO.delete(tourId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Xóa tour thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Xóa tour thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteTour error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
        
        /**
         * POST: /Admin/UpdateTourStatus
         */
        [HttpPost]
        public IActionResult UpdateTourStatus(int tourId, string status)
        {
            // Check if user is logged in and is admin
            int? userId = HttpContext.Session.GetInt32("userId");
            string sessionRole = HttpContext.Session.GetString("role");
            
            if (!userId.HasValue || !"ADMIN".Equals(sessionRole, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            try
            {
                bool success = tourDAO.updateStatus(tourId, status);
                
                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Cập nhật trạng thái thất bại" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateTourStatus error: " + e.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}
