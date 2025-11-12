using System.Diagnostics;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
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

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") != null)
            {
                // Get user role
                var userRole = HttpContext.Session.GetString("role");
                
                // If user is a Dealer, redirect to Dealer Home page
                if (userRole == "Dealer")
                {
                    return RedirectToAction("DealerHome");
                }
                
                // If user is an Admin, redirect to admin dashboard
                if (userRole == "Admin")
                {
                    return RedirectToAction("AdminDashboard");
                }
            }
            
            // Check if there are any materials in the database
            var materialCount = await _context.MaterialMaster.CountAsync();
            ViewBag.MaterialCount = materialCount;
            
            // Get first 5 materials for testing
            var materials = await _context.MaterialMaster.Take(5).ToListAsync();
            ViewBag.TestMaterials = materials;
            
            return View();
        }

        public async Task<IActionResult> DealerHome()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login");
            }
            
            // Get the logged-in user's information
            var userName = HttpContext.Session.GetString("UserName");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
            
            // Get the dealer information for this user
            DealerMaster dealer = null;
            if (user != null)
            {
                // Assuming the DealerMaster.PhoneNo corresponds to the User.phoneno for dealers
                dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.PhoneNo == user.phoneno);
            }
            
            // Calculate total points for the dealer
            var totalPoints = 0;
            if (dealer != null)
            {
                // Sum all points from order items for this dealer
                totalPoints = await _context.OrderItems
                    .Where(oi => oi.Order.DealerId == dealer.Id)
                    .SumAsync(oi => (int?)oi.Points) ?? 0;
            }
            
            var viewModel = new DealerDashboardViewModel
            {
                // Get all posters
                Posters = await _context.Posters.ToListAsync(),
                
                // Get all campaigns (reusing the logic from ViewCampaigns)
                Campaigns = new ViewCampaignsViewModel
                {
                    DetailedPointsToCashCampaigns = await _context.PointsToCashCampaigns
                        .Select(c => new DetailedPointsToCashCampaign
                        {
                            Id = c.Id,
                            CampaignName = c.CampaignName,
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            VoucherGenerationThreshold = c.VoucherGenerationThreshold,
                            VoucherValue = c.VoucherValue,
                            VoucherValidity = c.VoucherValidity,
                            Description = c.Description,
                            IsActive = c.IsActive,
                            ImagePath = c.ImagePath,
                            MaterialPoints = c.MaterialPoints // Include MaterialPoints JSON data
                            // MaterialDetails will be populated separately if needed
                        }).ToListAsync(),
                        
                    DetailedPointsRewardCampaigns = await _context.PointsRewardCampaigns
                        .Select(c => new DetailedPointsRewardCampaign
                        {
                            Id = c.Id,
                            CampaignName = c.CampaignName,
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            VoucherGenerationThreshold = c.VoucherGenerationThreshold,
                            VoucherValidity = c.VoucherValidity,
                            Description = c.Description,
                            IsActive = c.IsActive,
                            RewardProductId = c.RewardProductId,
                            ImagePath = c.ImagePath,
                            MaterialPoints = c.MaterialPoints // Include MaterialPoints JSON data
                            // MaterialDetails and RewardProduct will be populated separately if needed
                        }).ToListAsync(),
                        
                    DetailedFreeProductCampaigns = await _context.FreeProductCampaigns
                        .Select(c => new DetailedFreeProductCampaign
                        {
                            Id = c.Id,
                            CampaignName = c.CampaignName,
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            Description = c.Description,
                            IsActive = c.IsActive,
                            ImagePath = c.ImagePath
                            // MaterialDetails and FreeProductDetails will be populated separately if needed
                        }).ToListAsync(),
                        
                    AmountReachGoalCampaigns = await _context.AmountReachGoalCampaigns.Select(c => new AmountReachGoalCampaign
                    {
                        Id = c.Id,
                        CampaignName = c.CampaignName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Description = c.Description,
                        TargetAmount = c.TargetAmount,
                        VoucherValue = c.VoucherValue,
                        VoucherValidity = c.VoucherValidity,
                        IsActive = c.IsActive,
                        ImagePath = c.ImagePath
                    }).ToListAsync(),
                    SessionDurationRewardCampaigns = await _context.SessionDurationRewardCampaigns.Select(c => new SessionDurationRewardCampaign
                    {
                        Id = c.Id,
                        CampaignName = c.CampaignName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Description = c.Description,
                        SessionDuration = c.SessionDuration,
                        VoucherValue = c.VoucherValue,
                        VoucherValidity = c.VoucherValidity,
                        IsActive = c.IsActive,
                        ImagePath = c.ImagePath
                    }).ToListAsync()
                },
                
                // Get all materials and material images
                Materials = await _context.MaterialMaster.ToListAsync(),
                MaterialImages = await _context.MaterialImages.Include(m => m.MaterialMaster).ToListAsync(),
                
                // Get the most recent order for the logged-in dealer
                RecentOrder = dealer != null ? await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Material)
                    .Where(o => o.DealerId == dealer.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync() : null,
                    
                // Set the total points for the dealer
                TotalPoints = totalPoints
            };
            
            return View("~/Views/Home/Dealer/DealerHome.cshtml", viewModel);
        }

        public IActionResult AdminDashboard()
        {
            // Check if user is logged in and is an Admin
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToAction("Login");
            }
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> SeedMaterials()
        {
            // Check if materials already exist
            var existingMaterials = await _context.MaterialMaster.CountAsync();
            if (existingMaterials > 0)
            {
                ViewBag.Message = $"Database already contains {existingMaterials} materials.";
                return View("Index");
            }

            // Create test materials
            var testMaterials = new List<MaterialMaster>
            {
                new MaterialMaster
                {
                    ShortName = "SN001",
                    Materialname = "Test Product 1",
                    Unit = "KG",
                    Category = "Snacks",
                    subcategory = "Chips",
                    sequence = 1,
                    segementname = "Food",
                    material3partycode = "TP001",
                    price = 10.50m,
                    isactive = true,
                    CratesTypes = "Box",
                    dealerprice = 8.50m
                },
                new MaterialMaster
                {
                    ShortName = "SN002",
                    Materialname = "Test Product 2",
                    Unit = "PCS",
                    Category = "Sweets",
                    subcategory = "Cookies",
                    sequence = 2,
                    segementname = "Food",
                    material3partycode = "TP002",
                    price = 15.75m,
                    isactive = true,
                    CratesTypes = "Box",
                    dealerprice = 12.50m
                },
                new MaterialMaster
                {
                    ShortName = "SN003",
                    Materialname = "Test Product 3",
                    Unit = "LTR",
                    Category = "Beverages",
                    subcategory = "Juice",
                    sequence = 3,
                    segementname = "Drinks",
                    material3partycode = "TP003",
                    price = 20.00m,
                    isactive = true,
                    CratesTypes = "Bottle",
                    dealerprice = 16.00m
                }
            };

            _context.MaterialMaster.AddRange(testMaterials);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Successfully added 3 test materials to the database.";
            return View("Index");
        }

        public IActionResult Login()
        {
            // If user is already logged in, redirect based on role
            if (HttpContext.Session.GetString("UserName") != null)
            {
                var userRole = HttpContext.Session.GetString("role");
                if (userRole == "Dealer")
                {
                    return RedirectToAction("DealerHome");
                }
                else if (userRole == "Admin")
                {
                    return RedirectToAction("AdminDashboard");
                }
                // For other roles, stay on index
                return RedirectToAction("Index");
            }
            
            return View();
        }
        
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                var data = new User();
                data.phoneno = ViewBag.phoneno;
                return View(data);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public IActionResult change(User user)
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                var data = _context.Users.Where(a => a.phoneno == user.phoneno).AsNoTracking().FirstOrDefault();
                if (data != null)
                {
                    var modify = new User();
                    modify.Id = data.Id;
                    modify.Role = data.Role;
                    modify.phoneno = user.phoneno;
                    modify.Password = user.Password;
                    modify.name = data.name;
                    _context.Update(modify);
                    _context.SaveChanges();
                    return RedirectToAction("Index");

                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        
        //Post Action
        [HttpPost]
        public ActionResult Login(User u)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var obj = _context.Users.Where(a => a.phoneno.Equals(u.phoneno) && a.Password.Equals(u.Password)).FirstOrDefault();
                if (obj != null)
                {
                    HttpContext.Session.SetString("UserName", obj.phoneno?.ToString() ?? "");
                    HttpContext.Session.SetString("role", obj.Role?.ToString() ?? "");
                    HttpContext.Session.SetString("name", obj.name?.ToString() ?? "");
                    
                    // Redirect based on user role
                    if (obj.Role == "Dealer")
                    {
                        return RedirectToAction("DealerHome");
                    }
                    else if (obj.Role == "Admin")
                    {
                        // Admin gets access to admin dashboard
                        return RedirectToAction("AdminDashboard");
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    // Add error message
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(u);
                }
            }
            else
            {
                // Check user role and redirect accordingly after login
                var userRole = HttpContext.Session.GetString("role");
                if (userRole == "Dealer")
                {
                    return RedirectToAction("DealerHome");
                }
                else if (userRole == "Admin")
                {
                    return RedirectToAction("AdminDashboard");
                }
                return RedirectToAction("Index");
            }
        }
        
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            return RedirectToAction("Login");
        }

    }
}