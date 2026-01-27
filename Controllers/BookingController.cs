using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourBookingSystem.Models;
using TourBookingSystem.DAOs;
using TourBookingSystem.Database;
using System.Text.Json;

namespace TourBookingSystem.Controllers
{
    /**
     * Controller for handling tour booking operations
     */
    public class BookingController : Controller
    {
        private BookingDAO bookingDAO;
        private TourDAO tourDAO;
        
        public BookingController()
        {
            bookingDAO = new BookingDAO();
            tourDAO = new TourDAO();
            orderDAO = new OrderDAO();
        }

        /**
         * GET: /Booking
         */
        [HttpGet]
        public IActionResult Index(string action)
        {
            // Check if user is logged in
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            User user = new User();
            user.setUserId(userId.Value);
            user.setFullName(HttpContext.Session.GetString("fullName"));

            // Default action: show user's bookings/cart
            if (string.IsNullOrEmpty(action))
            {
                action = "list";
            }

            switch (action.ToLower())
            {
                case "list":
                    showUserBookings(user);
                    break;
                case "cancel":
                    return cancelBooking(user);
                default:
                    showUserBookings(user);
                    break;
            }

            return View();
        }

        /**
         * POST: /Booking
         */
        [HttpPost]
        public IActionResult Index(string action, string tourId, string quantity, string notes)
        {
            // Set content type for JSON response
            Response.ContentType = "application/json";

            // Check if user is logged in
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return Json(new { status = "error", message = "Vui lòng đăng nhập để tiếp tục" });
            }

            User user = new User();
            user.setUserId(userId.Value);
            user.setFullName(HttpContext.Session.GetString("fullName"));

            // Validate tourId
            if (string.IsNullOrEmpty(tourId) || tourId.Trim().Equals(""))
            {
                return Json(new { status = "error", message = "Tour không hợp lệ" });
            }

            int tourIdInt;
            try
            {
                tourIdInt = int.Parse(tourId.Trim());
            }
            catch
            {
                return Json(new { status = "error", message = "Tour không hợp lệ" });
            }

            // Validate quantity
            int quantityInt = 1;
            if (!string.IsNullOrEmpty(quantity) && !quantity.Trim().Equals(""))
            {
                try
                {
                    quantityInt = int.Parse(quantity.Trim());
                    if (quantityInt < 1) quantityInt = 1;
                }
                catch
                {
                    quantityInt = 1;
                }
            }

            try
            {
                Console.WriteLine("=== BookingController ===");
                Console.WriteLine("Action: " + action);
                Console.WriteLine("TourId: " + tourIdInt);
                Console.WriteLine("Quantity: " + quantityInt);
                Console.WriteLine("User: " + userId);

                if ("create".Equals(action))
                {
                    return createBooking(user, tourIdInt, quantityInt, notes);
                }
                else
                {
                    return Json(new { status = "error", message = "Hành động không hợp lệ" });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("BookingController Error: " + e.Message);
                return Json(new { status = "error", message = "Có lỗi xảy ra: " + e.Message });
            }
        }

        /**
         * Show user's bookings/cart page
         */
        private void showUserBookings(User user)
        {
            // Get all user bookings
            List<Booking> bookings = bookingDAO.getBookingsByUserId(user.getUserId());

            // Separate by status
            List<Booking> pendingBookings = new List<Booking>();
            List<Booking> confirmedBookings = new List<Booking>();

            foreach (Booking booking in bookings)
            {
                bool paid = orderDAO.hasPaidOrder(booking.getBookingId());
                booking.setIsPaid(paid);
                if ("PENDING".Equals(booking.getStatus(), StringComparison.OrdinalIgnoreCase))
                {
                    pendingBookings.Add(booking);
                }
                else if ("CONFIRMED".Equals(booking.getStatus(), StringComparison.OrdinalIgnoreCase))
                {
                    confirmedBookings.Add(booking);
                }
            }

            ViewData["bookings"] = bookings;
            ViewData["pendingBookings"] = pendingBookings;
            ViewData["confirmedBookings"] = confirmedBookings;
            ViewData["totalBookings"] = bookings.Count;

            // Get user info
            ViewData["userId"] = user.getUserId();
            ViewData["fullName"] = user.getFullName();
            ViewData["role"] = HttpContext.Session.GetString("role");
        }

        /**
         * Create a new booking
         */
        private IActionResult createBooking(User user, int tourId, int quantity, string notes)
        {
            // Get tour details
            Tour tour = tourDAO.findById(tourId);

            if (tour == null)
            {
                return Json(new { status = "error", message = "Tour không tồn tại" });
            }

            if (!"ACTIVE".Equals(tour.getStatus(), StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { status = "error", message = "Tour không còn hoạt động" });
            }

            if (quantity > tour.getAvailableSlots())
            {
                return Json(new { status = "error", message = "Số lượng không đủ. Chỉ còn " + tour.getAvailableSlots() + " chỗ" });
            }

            // Calculate total price
            decimal totalPrice = tour.getPrice() * quantity;

            // Create booking
            Booking booking = new Booking();
            booking.setUserId(user.getUserId());
            booking.setTourId(tourId);
            booking.setStatus("PENDING");
            booking.setNumParticipants(quantity);
            booking.setTotalPrice(totalPrice);
            booking.setNotes(notes);

            int bookingId = bookingDAO.createBooking(booking);

            if (bookingId > 0)
            {
                Console.WriteLine("Booking created successfully with ID: " + bookingId);
                return Json(new { status = "success", message = "Đặt tour thành công!", bookingId = bookingId });
            }
            else
            {
                return Json(new { status = "error", message = "Không thể tạo đơn đặt tour" });
            }
        }

        /**
         * Cancel a booking
         */
        //private void cancelBooking(User user)
        //{
        //    string bookingIdStr = Request.Query["bookingId"];

        //    if (string.IsNullOrEmpty(bookingIdStr) || bookingIdStr.Trim().Equals(""))
        //    {
        //        ViewData["error"] = "Booking ID không hợp lệ";
        //        showUserBookings(user);
        //        return;
        //    }

        //    int bookingId;
        //    try
        //    {
        //        bookingId = int.Parse(bookingIdStr.Trim());
        //    }
        //    catch
        //    {
        //        ViewData["error"] = "Booking ID không hợp lệ";
        //        showUserBookings(user);
        //        return;
        //    }

        //    // Verify booking belongs to user
        //    Booking booking = bookingDAO.findById(bookingId);

        //    if (booking == null)
        //    {
        //        ViewData["error"] = "Đơn đặt tour không tồn tại";
        //        showUserBookings(user);
        //        return;
        //    }

        //    if (booking.getUserId() != user.getUserId())
        //    {
        //        ViewData["error"] = "Bạn không có quyền hủy đơn này";
        //        showUserBookings(user);
        //        return;
        //    }

        //    // Only allow cancel PENDING bookings
        //    if (!"PENDING".Equals(booking.getStatus(), StringComparison.OrdinalIgnoreCase))
        //    {
        //        ViewData["error"] = "Chỉ có thể hủy đơn đang chờ xác nhận";
        //        showUserBookings(user);
        //        return;
        //    }

        //    // Cancel the booking
        //    bool success = bookingDAO.cancelBooking(bookingId);

        //    if (success)
        //    {
        //        ViewData["success"] = "Hủy đặt tour thành công!";
        //    }
        //    else
        //    {
        //        ViewData["error"] = "Không thể hủy đặt tour. Vui lòng thử lại.";
        //    }

        //    showUserBookings(user);
        //}
        private IActionResult cancelBooking(User user)
        {
            string bookingIdStr = Request.Query["bookingId"];

            if (string.IsNullOrEmpty(bookingIdStr) || bookingIdStr.Trim().Equals(""))
            {
                TempData["error"] = "Booking ID không hợp lệ";
                return RedirectToAction("Index");
            }

            int bookingId;

            if (!int.TryParse(bookingIdStr, out bookingId))
            {
                TempData["error"] = "Booking ID không hợp lệ";
                return RedirectToAction("Index");
            }

            Booking booking = bookingDAO.findById(bookingId);

            if (booking == null)
            {
                TempData["error"] = "Đơn không tồn tại";
                return RedirectToAction("Index");
            }

            if (booking.getUserId() != user.getUserId())
            {
                TempData["error"] = "Không có quyền";
                return RedirectToAction("Index");
            }

            if (!"PENDING".Equals(booking.getStatus(), StringComparison.OrdinalIgnoreCase))
            {
                TempData["error"] = "Không thể hủy";
                return RedirectToAction("Index");
            }

            bool success = bookingDAO.cancelBooking(bookingId);

            if (success)
            {
                ViewData["success"] = "Hủy đặt tour thành công!";
            }
            else
            {
                ViewData["error"] = "Không thể hủy đặt tour. Vui lòng thử lại.";
            }

            showUserBookings(user);
        }
    }
}
