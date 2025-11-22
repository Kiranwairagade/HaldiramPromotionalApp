using HaldiramPromotionalApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HaldiramPromotionalApp.Controllers
{
    public class SalesController : Controller
    {
        private readonly ILogger<SalesController> _logger;
        private readonly ApplicationDbContext _context;

        public SalesController(ILogger<SalesController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Sales or Sales/Index
        [Route("Home/SalesHome")] // Backward compatibility
        [Route("Sales")]
        [Route("Sales/Index")]
        public IActionResult Index()
        {
            // Check if user is logged in and is a Sales
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Sales")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View("~/Views/Sales/Index.cshtml");
        }
    }
}
