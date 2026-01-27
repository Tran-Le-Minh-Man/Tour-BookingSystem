using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using TourBookingSystem.Models;
using TourBookingSystem.DAOs;
using TourBookingSystem.Database;

namespace TourBookingSystem.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly FavoritesDAO favoritesDAO;
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
            favoritesDAO = new FavoritesDAO(_context);
        }

        [HttpGet]
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                List<Dictionary<string, object>> favorites = favoritesDAO.getFavoritesByUserId(userId.Value);
                ViewData["favorites"] = favorites;
            }
            catch (Exception e)
            {
                Console.WriteLine("FavoritesController.Index ERROR: " + e.Message);
                TempData["error"] = "Không thể tải danh sách yêu thích: " + e.Message;
                ViewData["favorites"] = new List<Dictionary<string, object>>();
            }

            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");

            return View();
        }

        [HttpPost]
        public IActionResult Toggle(int tourId)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                bool isFav = favoritesDAO.isFavorite(userId.Value, tourId);
                bool success;
                string message;

                if (isFav)
                {
                    success = favoritesDAO.removeFavorite(userId.Value, tourId);
                    message = "Đã bỏ khỏi danh sách yêu thích";
                }
                else
                {
                    success = favoritesDAO.addFavorite(userId.Value, tourId);
                    message = "Đã thêm vào danh sách yêu thích";
                }

                return Json(new { success = success, message = message, isFavorite = !isFav });
            }
            catch (Exception e)
            {
                string detail = e.Message;
                if (e.InnerException != null) detail += " -> " + e.InnerException.Message;
                Console.WriteLine("FavoritesController.Toggle ERROR: " + detail);
                return Json(new { success = false, message = "Lỗi hệ thống: " + detail });
            }
        }
    }
}
