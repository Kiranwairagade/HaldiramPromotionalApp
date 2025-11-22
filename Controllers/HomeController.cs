using HaldiramPromotionalApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Main landing page - routes users to appropriate dashboard based on their role
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") != null)
            {
                // Get user role
                var userRole = HttpContext.Session.GetString("role");

                // Route to appropriate dashboard based on role
                return userRole switch
                {
                    "Dealer" => RedirectToAction("Index", "Dealer"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Shopkeeper" => RedirectToAction("Index", "Shopkeeper"),
                    "Sales" => RedirectToAction("Index", "Sales"),
                    "Customer" => RedirectToAction("Index", "Customer"),
                    _ => RedirectToAction("Login", "Auth")
                };
            }

            // If not logged in, show landing page with some basic info
            // Check if there are any materials in the database
            var materialCount = await _context.MaterialMaster.CountAsync();
            ViewBag.MaterialCount = materialCount;

            // Get first 5 materials for testing/display
            var materials = await _context.MaterialMaster.Take(5).ToListAsync();
            ViewBag.TestMaterials = materials;

            return View();
        }

        /// <summary>
        /// Privacy policy page
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }
    }
}