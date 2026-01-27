using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourBookingSystem.Models;
using TourBookingSystem.DAOs;
using TourBookingSystem.Utils;
using TourBookingSystem.Database;
namespace TourBookingSystem.Controllers
{
    /**
     * Controller for handling user authentication
     */
    public class AccountController : Controller
    {
        private readonly UserDAO userDAO;
        private readonly ApplicationDbContext _context;

        private static readonly int MAX_LOGIN_ATTEMPTS = 5;
        private static readonly int LOCKOUT_TIME = 15 * 60; // 15 minutes in seconds

        // Rate limiting storage
        private static readonly Dictionary<string, LoginAttempt> loginAttempts = new Dictionary<string, LoginAttempt>();

        /**
         * Inner class to track login attempts
         */
        private class LoginAttempt
        {
            public int count;
            public long lastAttempt;

            public LoginAttempt()
            {
                this.count = 0;
                this.lastAttempt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            public void increment()
            {
                this.count++;
                this.lastAttempt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            public bool isLocked()
            {
                if (count >= MAX_LOGIN_ATTEMPTS)
                {
                    long lockoutEnd = lastAttempt + (LOCKOUT_TIME * 1000L);
                    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < lockoutEnd;
                }
                return false;
            }

            public int getRemainingAttempts()
            {
                return Math.Max(0, MAX_LOGIN_ATTEMPTS - count);
            }
        }

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            userDAO = new UserDAO(_context);
        }

        /**
         * GET: /Account/Login
         */
        [HttpGet]
        public IActionResult Login()
        {

            // Check if user is already logged in
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            // Check for remember me cookie and auto-login
            string rememberToken = HttpContext.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(rememberToken))
            {
                User user = userDAO.findByRememberToken(rememberToken);
                if (user != null)
                {
                    createUserSession(user);
                    return RedirectToAction("Index", "Home");
                }
            }

            // Check for success message from registration
            if (TempData["success"] != null)
            {
                ViewData["success"] = TempData["success"];
            }

            // Check for lockout status
            string clientIP = getClientIP();
            if (loginAttempts.TryGetValue(clientIP, out LoginAttempt attempt) && attempt.isLocked())
            {
                long remainingTime = (attempt.lastAttempt + (LOCKOUT_TIME * 1000) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000;
                ViewData["error"] = "Tài khoản tạm khóa. Vui lòng thử lại sau " + remainingTime + " giây.";
            }

            return View();
        }

        /**
         * POST: /Account/Login
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, string remember)
        {
            string clientIP = getClientIP();

            // Check for rate limiting
            if (!loginAttempts.TryGetValue(clientIP, out LoginAttempt attempt))
            {
                attempt = new LoginAttempt();
                loginAttempts[clientIP] = attempt;
            }

            if (attempt.isLocked())
            {
                long remainingTime = (attempt.lastAttempt + (LOCKOUT_TIME * 1000) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000;
                ViewData["error"] = "Tài khoản tạm khóa. Vui lòng thử lại sau " + remainingTime + " giây.";
                return View();
            }

            try
            {
                // Get and validate CSRF token - HANDLED BY [ValidateAntiForgeryToken]
                // string sessionCsrfToken = HttpContext.Session.GetString("_csrf_token");
                // if (sessionCsrfToken == null || !sessionCsrfToken.Equals(Request.Form["_csrf_token"]))
                // {
                //    attempt.increment();
                //    ViewData["error"] = "Yêu cầu không hợp lệ. Vui lòng thử lại.";
                //    return View();
                // }

                // Basic validation
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    attempt.increment();
                    ViewData["error"] = "Vui lòng nhập đầy đủ email và mật khẩu.";
                    ViewData["email"] = email;
                    return View();
                }

                // Verify login credentials
                User user = userDAO.verifyLogin(email.Trim().ToLower(), password);

                if (user == null)
                {
                    // Login failed - increment attempt counter
                    attempt.increment();
                    ViewData["error"] = "Email hoặc mật khẩu không đúng.";
                    ViewData["email"] = email;
                    return View();
                }

                // Login successful - reset attempt counter
                loginAttempts.Remove(clientIP);

                // Create new session to prevent session fixation
                HttpContext.Session.Clear();

                // Store user in session
                createUserSession(user);

                // Handle remember me functionality with secure token storage
                if ("on".Equals(remember))
                {
                    string token = generateSecureToken();

                    // Store token in database with expiry
                    DateTime expiryDate = DateTime.Now.AddDays(7);
                    userDAO.updateRememberToken(user.getUserId(), token, expiryDate);

                    // Create secure cookie
                    CookieOptions cookieOptions = new CookieOptions();
                    cookieOptions.Expires = DateTime.Now.AddDays(7);
                    cookieOptions.Path = "/";
                    cookieOptions.HttpOnly = true;
                    cookieOptions.Secure = Request.IsHttps;
                    Response.Cookies.Append("auth_token", token, cookieOptions);
                }

                // Redirect based on role
                if ("ADMIN".Equals(user.getRole(), StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                Console.WriteLine("Login error: " + e.Message);
                ViewData["error"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View();
            }
        }

        /**
         * GET: /Account/Register
         */
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /**
         * POST: /Account/Register
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullName, string email, string phone, string password, string confirmPassword)
        {
            System.Text.StringBuilder errorMessages = new System.Text.StringBuilder();
            // Sanitize inputs (basic trim)
            fullName = fullName?.Trim();
            email = email?.Trim();
            phone = phone?.Trim();
            // Validate full name
            if (string.IsNullOrEmpty(fullName) || fullName.Length < 2 || fullName.Length > 100)
            {
                errorMessages.Append("Họ tên không hợp lệ. Vui lòng nhập từ 2-100 ký tự.");
            }
            // Validate email
            if (!ValidationUtil.isValidEmail(email))
            {
                if (errorMessages.Length > 0) errorMessages.Append("<br>");
                errorMessages.Append("Email không hợp lệ. Vui lòng nhập đúng định dạng email.");
            }
            // Validate phone (optional but if provided must be valid)
            if (!string.IsNullOrEmpty(phone) && !ValidationUtil.isValidPhone(phone))
            {
                if (errorMessages.Length > 0) errorMessages.Append("<br>");
                errorMessages.Append("Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam.");
            }
            // Validate password (Java placeholder says min 8 chars)
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                if (errorMessages.Length > 0) errorMessages.Append("<br>");
                errorMessages.Append("Mật khẩu phải có ít nhất 8 ký tự.");
            }
            // Validate password match
            if (password != confirmPassword)
            {
                if (errorMessages.Length > 0) errorMessages.Append("<br>");
                errorMessages.Append("Mật khẩu và xác nhận mật khẩu không khớp.");
            }
            // If there are validation errors, return view
            if (errorMessages.Length > 0)
            {
                ViewData["error"] = errorMessages.ToString();
                ViewData["fullName"] = fullName;
                ViewData["email"] = email;
                ViewData["phone"] = phone;
                return View();
            }

            // Check if email already exists
            if (userDAO.emailExists(email))
            {
                ViewData["error"] = "Email đã được đăng ký. Vui lòng sử dụng email khác.";
                ViewData["fullName"] = fullName;
                ViewData["email"] = email;
                ViewData["phone"] = phone;
                return View();
            }

            try
            {
                // Create new user
                User user = new User();
                user.setFullName(fullName);
                user.setEmail(email);
                user.setPhone(phone);
                user.setRole("USER");

                // Register user
                bool success = userDAO.register(user, password);

                if (success)
                {
                    TempData["success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ViewData["error"] = "Đăng ký thất bại. Vui lòng thử lại.";
                    ViewData["fullName"] = fullName;
                    ViewData["email"] = email;
                    ViewData["phone"] = phone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Registration error: " + e.Message);
                ViewData["error"] = "Lỗi chi tiết: " + e.Message;

                ViewData["fullName"] = fullName;
                ViewData["email"] = email;
                ViewData["phone"] = phone;
            }

            return View();
        }

        /**
         * GET: /Account/Logout
         */
        [HttpGet]
        public IActionResult Logout()
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId.HasValue)
            {
                // Clear remember token in database
                userDAO.clearRememberToken(userId.Value);
            }

            // Clear session
            HttpContext.Session.Clear();

            // Clear remember me cookie
            Response.Cookies.Delete("auth_token");

            return RedirectToAction("Index", "Home");
        }

        /**
         * GET: /Account/EditProfile
         */
        [HttpGet]
        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            User user = userDAO.findById(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            ViewData["userId"] = userId;
            ViewData["fullName"] = HttpContext.Session.GetString("fullName");
            ViewData["role"] = HttpContext.Session.GetString("role");

            return View(user);
        }

        /**
         * POST: /Account/UpdateProfile
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string fullName, string phone)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                User user = userDAO.findById(userId.Value);
                user.setFullName(fullName.Trim());
                user.setPhone(phone.Trim());

                bool success = userDAO.update(user);
                if (success)
                {
                    HttpContext.Session.SetString("fullName", user.getFullName());
                    TempData["success"] = "Cập nhật thông tin thành công!";
                }
                else
                {
                    TempData["error"] = "Không thể cập nhật thông tin.";
                }
            }
            catch (Exception e)
            {
                TempData["error"] = "Lỗi: " + e.Message;
            }

            return RedirectToAction("EditProfile");
        }

        /**
         * POST: /Account/ChangePassword
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (newPassword != confirmPassword)
            {
                TempData["error"] = "Mật khẩu mới không khớp.";
                return RedirectToAction("EditProfile");
            }

            if (newPassword.Length < 8)
            {
                TempData["error"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
                return RedirectToAction("EditProfile");
            }

            try
            {
                User user = userDAO.findById(userId.Value);
                // We need verifyLogin logic but for a specific user. 
                // Since userDAO doesn't have verifyPassword(userId, password), 
                // we'll use verifyLogin with the current user's email.
                User verifiedUser = userDAO.verifyLogin(user.getEmail(), oldPassword);

                if (verifiedUser == null)
                {
                    TempData["error"] = "Mật khẩu cũ không đúng.";
                    return RedirectToAction("EditProfile");
                }

                bool success = userDAO.updatePassword(userId.Value, newPassword);
                if (success)
                {
                    TempData["success"] = "Đổi mật khẩu thành công!";
                }
                else
                {
                    TempData["error"] = "Không thể đổi mật khẩu.";
                }
            }
            catch (Exception e)
            {
                TempData["error"] = "Lỗi: " + e.Message;
            }

            return RedirectToAction("EditProfile");
        }

        /**
         * Create user session
         */
        private void createUserSession(User user)
        {
            HttpContext.Session.SetInt32("userId", user.getUserId());
            HttpContext.Session.SetString("email", user.getEmail());
            HttpContext.Session.SetString("fullName", user.getFullName());
            HttpContext.Session.SetString("phone", user.getPhone());
            HttpContext.Session.SetString("role", user.getRole());
            // Note: Session timeout is configured in Program.cs (default: 30 minutes)
            // HttpContext.Session.Timeout cannot be set at runtime in ASP.NET Core
            // Generate new CSRF token after login
        }

        /**
         * Generate a secure remember token
         */
        private string generateSecureToken()
        {
            byte[] bytes = new byte[64]; // 512 bits
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
        }

        /**
         * Get client IP address
         */
        private string getClientIP()
        {
            string xForwardedFor = Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}