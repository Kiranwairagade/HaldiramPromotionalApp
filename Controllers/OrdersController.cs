using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public OrdersController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's information from session
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");
            
            // Validate that user is logged in and is a dealer
            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Unauthorized("You must be logged in as a dealer to access orders.");
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            
            // Find the dealer associated with this user by matching phone numbers
            var dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.PhoneNo == user.phoneno);
            if (dealer == null)
            {
                return BadRequest("Dealer not found for this user");
            }
            
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Material)
                .Where(o => o.DealerId == dealer.Id) // Filter by dealer ID
                .ToListAsync();
            return View("~/Views/Dealer/Orders/Index.cshtml", orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Material)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }

            return View("~/Views/Dealer/Orders/Details.cshtml", order);
        }

        // GET: Orders/GetOrderDetails/5
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            // Get the logged-in user's information from session
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");
            
            // Validate that user is logged in and is a dealer
            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Unauthorized("You must be logged in as a dealer to access order details.");
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Material)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);
                
            if (order == null)
            {
                return NotFound("Order not found or you don't have permission to access it.");
            }
            
            return Ok(new { 
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderItems = order.OrderItems.Select(oi => new {
                    MaterialId = oi.MaterialId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Material = new {
                        Id = oi.Material.Id,
                        Materialname = oi.Material.Materialname,
                        material3partycode = oi.Material.material3partycode,
                        dealerprice = oi.Material.dealerprice
                    }
                }).ToList()
            });
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] OrderRequest orderRequest)
        {
            // Get the logged-in user's information from session
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");
            var fullName = HttpContext.Session.GetString("name");
            
            // Validate that user is logged in and is a dealer
            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Unauthorized("You must be logged in as a dealer to place orders.");
            }
            
            // If order request is null, create a minimal one
            if (orderRequest == null)
            {
                orderRequest = new OrderRequest();
            }
            
            if (orderRequest.Items == null || !orderRequest.Items.Any())
            {
                return BadRequest("Order request is invalid or empty");
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                // Get user details from database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return BadRequest("User not found");
                }
                
                // Find the dealer associated with this user by matching phone numbers
                var dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.PhoneNo == user.phoneno);
                if (dealer == null)
                {
                    return BadRequest("Dealer not found for this user");
                }
                
                // Create the order with user information and dealer ID
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    UserId = user.Id,
                    DealerId = dealer.Id, // Set the DealerId from the DealerMaster record
                    OrderStatus = "Pending",
                    TotalAmount = orderRequest.Items.Sum(i => i.Quantity * i.Price)
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var item in orderRequest.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        MaterialId = item.MaterialId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.Quantity * item.Price,
                        Points = item.Points // Set the Points property
                    };
                    
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Create notification for the new order
                await _notificationService.CreateOrderNotificationAsync(user.Id, order.Id);

                return Ok(new { OrderId = order.Id, Message = "Order created successfully" });
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging
                Console.WriteLine($"Exception in Create Order: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // DTO for order creation
    public class OrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; } // Add Points property
    }
}