using HaldiramPromotionalApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HaldiramPromotionalApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDbContext _context;

        public CustomerController(ILogger<CustomerController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Customer or Customer/Index
        [Route("Home/CustomerHome")] // Backward compatibility
        [Route("Customer")]
        [Route("Customer/Index")]
        public IActionResult Index()
        {
            // Check if user is logged in and is a Customer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Customer")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View("~/Views/Customer/Index.cshtml");
        }
    }
}
