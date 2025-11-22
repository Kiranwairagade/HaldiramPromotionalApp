using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(ILogger<AuthController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            // If user is already logged in, redirect based on role
            if (HttpContext.Session.GetString("UserName") != null)
            {
                var userRole = HttpContext.Session.GetString("role");

                return userRole switch
                {
                    "Dealer" => RedirectToAction("Index", "Dealer"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Shopkeeper" => RedirectToAction("Index", "Shopkeeper"),
                    "Sales" => RedirectToAction("Index", "Sales"),
                    "Customer" => RedirectToAction("Index", "Customer"),
                    _ => View()
                };
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string phoneno, string password)
        {
            if (string.IsNullOrEmpty(phoneno) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Phone number and password are required.";
                return View();
            }

            // Find user by phone number
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == phoneno);

            if (user == null)
            {
                ViewBag.Message = "Invalid phone number or password.";
                return View();
            }

            // Verify password
            if (user.Password != password)
            {
                ViewBag.Message = "Invalid phone number or password.";
                return View();
            }

            // Set session variables
            HttpContext.Session.SetString("UserName", user.phoneno);
            HttpContext.Session.SetString("role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Redirect based on role
            return user.Role switch
            {
                "Dealer" => RedirectToAction("Index", "Dealer"),
                "Admin" => RedirectToAction("Index", "Admin"),
                "Shopkeeper" => RedirectToAction("Index", "Shopkeeper"),
                "Sales" => RedirectToAction("Index", "Sales"),
                "Customer" => RedirectToAction("Index", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Auth/ChangePassword
        public IActionResult ChangePassword()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Auth/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            // Validate inputs
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Message = "All fields are required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "New password and confirm password do not match.";
                return View();
            }

            // Get current user
            var userName = HttpContext.Session.GetString("UserName");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

            if (user == null)
            {
                ViewBag.Message = "User not found.";
                return View();
            }

            // Verify old password
            if (user.Password != oldPassword)
            {
                ViewBag.Message = "Old password is incorrect.";
                return View();
            }

            // Update password
            user.Password = newPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Password changed successfully.";
            return View();
        }
    }
}
