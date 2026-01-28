using Microsoft.AspNetCore.Mvc;
using TourBookingSystem.DAOs;
using TourBookingSystem.Database;
using TourBookingSystem.Models;

namespace TourBookingSystem.Controllers
{
    
    public class CheckoutController : Controller
    {
        private readonly BookingDAO bookingDAO;
        private readonly OrderDAO orderDAO;
        private readonly ApplicationDbContext _context;
        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
            bookingDAO = new BookingDAO(_context);
            orderDAO = new OrderDAO(_context);
        }

        // ================= GET =================
        [HttpGet]
        public IActionResult Index(int bookingId)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            Console.WriteLine("===== SESSION CHECK =====");
            Console.WriteLine("userId = " + userId);
            Console.WriteLine("fullName = " + HttpContext.Session.GetString("fullName"));
            Console.WriteLine("email = " + HttpContext.Session.GetString("email"));
            Console.WriteLine("phone = " + HttpContext.Session.GetString("phone"));
            Console.WriteLine("=========================");


            if (userId == null)
                return RedirectToAction("Login", "Account");

            Booking booking = bookingDAO.findById(bookingId);

            if (booking == null || booking.getUserId() != userId)
                return NotFound();

            if (orderDAO.HasPaidOrder(bookingId))
            {
                TempData["msg"] = "Đơn này đã được thanh toán!";
                return RedirectToAction("Index", "Booking");
            }
            // CHỈ CHO THANH TOÁN KHI CONFIRMED
            if (!booking.getStatus().Equals("CONFIRMED"))
                return RedirectToAction("Index", "Booking");


            CheckoutViewModel vm = new CheckoutViewModel
            {
                BookingId = booking.getBookingId(),
                TourId = booking.getTourId(),
                TourName = booking.getTourName(),
                Destination = booking.getTourDestination(),
                ImageUrl = booking.getTourImage(),
                Quantity = booking.getNumParticipants(),
                Price = booking.getTourPrice(),
                Total= booking.getTotalPrice(),
                FullName = HttpContext.Session.GetString("fullName"),
                Email = HttpContext.Session.GetString("email"),
                Phone = HttpContext.Session.GetString("phone"),
                Note = booking.getNotes()
            };

            return View(vm);
        }



        // ================= POST =================
        [HttpPost]
        public IActionResult Index(CheckoutViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
                return RedirectToAction("Login", "Account");


            Booking booking = bookingDAO.findById(model.BookingId);

            decimal unitPrice = booking.getTotalPrice() / booking.getNumParticipants();

            decimal total = unitPrice * model.Quantity;

            if (booking == null || booking.getUserId() != userId)
                return NotFound();


            Order order = new Order
            {
                user_id = userId.Value,
                booking_id = model.BookingId,
                tour_id = model.TourId,

                quantity = Math.Max(1, model.Quantity),
                total_price = model.Quantity * booking.getTourPrice(),

                note = model.Note ?? "",

                payment_method = model.PaymentMethod ?? "COD",
                payment_provider = model.PaymentProvider ?? "",

                status = "PAID",

                created_at = DateTime.Now
            };


            bool success = orderDAO.Insert(order);

            if (success)
            {
                
                TempData["success"] = "Thanh toán thành công!";
                return RedirectToAction("Index", "Booking");
            }

            ViewBag.Error = "Thanh toán thất bại";
            return View(model);
        }



        
    } 
}
