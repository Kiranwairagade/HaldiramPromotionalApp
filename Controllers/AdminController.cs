using HaldiramPromotionalApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HaldiramPromotionalApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Admin or Admin/Index
        [Route("Home/AdminDashboard")] // Backward compatibility
        [Route("Admin")]
        [Route("Admin/Index")]
        public IActionResult Index()
        {
            // Check if user is logged in and is an Admin
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View("~/Views/Admin/Index.cshtml");
        }
    }
}
