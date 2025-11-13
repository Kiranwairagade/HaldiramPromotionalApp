using System.Diagnostics;
using System.Text.Json;
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
        
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> PointsDetails()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login");
            }

            try
            {
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

                if (dealer == null)
                {
                    // If no dealer found, redirect to login
                    return RedirectToAction("Login");
                }

                // Get all order items for this dealer with related data
                var orderItems = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Material)
                    .Where(oi => oi.Order.DealerId == dealer.Id)
                    .OrderByDescending(oi => oi.Order.OrderDate)
                    .ToListAsync();

                // Pass the order items to the view
                // The view will calculate the summary statistics
                return View("~/Views/Home/PointsDetails.cshtml", orderItems);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in PointsDetails action: {ex.Message}");
                // Redirect to dealer home page if there's an error
                return RedirectToAction("DealerHome");
            }
        }

        public async Task<IActionResult> ViewCampaigns()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Fetch campaigns with related data
                var pointsToCashCampaigns = await _context.PointsToCashCampaigns.ToListAsync();
                var pointsRewardCampaigns = await _context.PointsRewardCampaigns
                    .Include(p => p.RewardProduct)
                    .ToListAsync();
                var freeProductCampaigns = await _context.FreeProductCampaigns.ToListAsync();
                var amountReachGoalCampaigns = await _context.AmountReachGoalCampaigns.ToListAsync();
                var sessionDurationRewardCampaigns = await _context.SessionDurationRewardCampaigns.ToListAsync();

                // Fetch all materials for reference
                var allMaterials = await _context.MaterialMaster.Where(m => m.isactive).ToDictionaryAsync(m => m.Id, m => m);
                var allProducts = await _context.Products.Where(p => p.IsActive).ToDictionaryAsync(p => p.Id, p => p);

                // Process PointsToCashCampaigns to include material names
                var detailedPointsToCashCampaigns = new List<DetailedPointsToCashCampaign>();
                foreach (var campaign in pointsToCashCampaigns)
                {
                    var detailedCampaign = new DetailedPointsToCashCampaign
                    {
                        Id = campaign.Id,
                        CampaignName = campaign.CampaignName,
                        StartDate = campaign.StartDate,
                        EndDate = campaign.EndDate,
                        VoucherGenerationThreshold = campaign.VoucherGenerationThreshold,
                        VoucherValue = campaign.VoucherValue,
                        VoucherValidity = campaign.VoucherValidity,
                        Description = campaign.Description,
                        IsActive = campaign.IsActive,
                        ImagePath = campaign.ImagePath
                    };

                    // Process materials
                    if (!string.IsNullOrEmpty(campaign.Materials))
                    {
                        try
                        {
                            var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                            var materialPoints = !string.IsNullOrEmpty(campaign.MaterialPoints) ?
                                JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialPoints) :
                                new Dictionary<int, int>();

                            foreach (var materialId in materialIds)
                            {
                                if (allMaterials.ContainsKey(materialId))
                                {
                                    detailedCampaign.MaterialDetails.Add(new MaterialDetail
                                    {
                                        MaterialId = materialId,
                                        MaterialName = allMaterials[materialId].Materialname,
                                        Points = materialPoints.ContainsKey(materialId) ? materialPoints[materialId] : 0
                                    });
                                }
                            }
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

                    detailedPointsToCashCampaigns.Add(detailedCampaign);
                }

                // Process PointsRewardCampaigns to include material names
                var detailedPointsRewardCampaigns = new List<DetailedPointsRewardCampaign>();
                foreach (var campaign in pointsRewardCampaigns)
                {
                    var detailedCampaign = new DetailedPointsRewardCampaign
                    {
                        Id = campaign.Id,
                        CampaignName = campaign.CampaignName,
                        StartDate = campaign.StartDate,
                        EndDate = campaign.EndDate,
                        VoucherGenerationThreshold = campaign.VoucherGenerationThreshold,
                        VoucherValidity = campaign.VoucherValidity,
                        Description = campaign.Description,
                        IsActive = campaign.IsActive,
                        RewardProductId = campaign.RewardProductId,
                        RewardProduct = campaign.RewardProduct,
                        ImagePath = campaign.ImagePath
                    };

                    // Process materials
                    if (!string.IsNullOrEmpty(campaign.Materials))
                    {
                        try
                        {
                            var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                            var materialPoints = !string.IsNullOrEmpty(campaign.MaterialPoints) ?
                                JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialPoints) :
                                new Dictionary<int, int>();

                            foreach (var materialId in materialIds)
                            {
                                if (allMaterials.ContainsKey(materialId))
                                {
                                    detailedCampaign.MaterialDetails.Add(new MaterialDetail
                                    {
                                        MaterialId = materialId,
                                        MaterialName = allMaterials[materialId].Materialname,
                                        Points = materialPoints.ContainsKey(materialId) ? materialPoints[materialId] : 0
                                    });
                                }
                            }
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

                    detailedPointsRewardCampaigns.Add(detailedCampaign);
                }

                // Process FreeProductCampaigns to include material names and free product details
                var detailedFreeProductCampaigns = new List<DetailedFreeProductCampaign>();
                foreach (var campaign in freeProductCampaigns)
                {
                    var detailedCampaign = new DetailedFreeProductCampaign
                    {
                        Id = campaign.Id,
                        CampaignName = campaign.CampaignName,
                        StartDate = campaign.StartDate,
                        EndDate = campaign.EndDate,
                        Description = campaign.Description,
                        IsActive = campaign.IsActive,
                        ImagePath = campaign.ImagePath
                    };

                    // Process materials
                    if (!string.IsNullOrEmpty(campaign.Materials))
                    {
                        try
                        {
                            var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                            var materialQuantities = !string.IsNullOrEmpty(campaign.MaterialQuantities) ?
                                JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialQuantities) :
                                new Dictionary<int, int>();
                            var freeProducts = !string.IsNullOrEmpty(campaign.FreeProducts) ?
                                JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.FreeProducts) :
                                new Dictionary<int, int>();
                            var freeQuantities = !string.IsNullOrEmpty(campaign.FreeQuantities) ?
                                JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.FreeQuantities) :
                                new Dictionary<int, int>();

                            foreach (var materialId in materialIds)
                            {
                                if (allMaterials.ContainsKey(materialId))
                                {
                                    detailedCampaign.MaterialDetails.Add(new MaterialDetail
                                    {
                                        MaterialId = materialId,
                                        MaterialName = allMaterials[materialId].Materialname,
                                        Quantity = materialQuantities.ContainsKey(materialId) ? materialQuantities[materialId] : 0
                                    });
                                }
                            }

                            foreach (var kvp in freeProducts)
                            {
                                var materialId = kvp.Key;
                                var freeProductId = kvp.Value;

                                if (allProducts.ContainsKey(freeProductId))
                                {
                                    detailedCampaign.FreeProductDetails[materialId] = new MaterialFreeProductDetail
                                    {
                                        FreeProductId = freeProductId,
                                        FreeProductName = allProducts[freeProductId].ProductName,
                                        FreeQuantity = freeQuantities.ContainsKey(materialId) ? freeQuantities[materialId] : 0
                                    };
                                }
                            }
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

                    detailedFreeProductCampaigns.Add(detailedCampaign);
                }

                var viewModel = new ViewCampaignsViewModel
                {
                    DetailedPointsToCashCampaigns = detailedPointsToCashCampaigns,
                    DetailedPointsRewardCampaigns = detailedPointsRewardCampaigns,
                    DetailedFreeProductCampaigns = detailedFreeProductCampaigns,
                    AmountReachGoalCampaigns = amountReachGoalCampaigns.Select(c => new AmountReachGoalCampaign
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
                    }).ToList(),
                    SessionDurationRewardCampaigns = sessionDurationRewardCampaigns.Select(c => new SessionDurationRewardCampaign
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
                    }).ToList()
                };

                return View("~/Views/Home/Dealer/ViewCampaigns.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in ViewCampaigns action: {ex.Message}");
                // Redirect to dealer home page if there's an error
                return RedirectToAction("DealerHome");
            }
        }

    }
}