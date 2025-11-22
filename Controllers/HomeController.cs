using System.Diagnostics;
using System.Text.Json;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.Services;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace HaldiramPromotionalApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, INotificationService notificationService)
        {
            _logger = logger;
            _context = context;
            _notificationService = notificationService;
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
                
                // If user is a Shopkeeper, redirect to shopkeeper dashboard
                if (userRole == "Shopkeeper")
                {
                    return RedirectToAction("ShopkeeperHome");
                }
                
                // If user is a Sales, redirect to sales dashboard
                if (userRole == "Sales")
                {
                    return RedirectToAction("SalesHome");
                }
                
                // If user is a Customer, redirect to customer dashboard
                if (userRole == "Customer")
                {
                    return RedirectToAction("CustomerHome");
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
            
            // Log dealer information for debugging
            System.Diagnostics.Debug.WriteLine($"Dealer: {dealer?.Id}, Total Points: {totalPoints}");
            
            // Check if dealer qualifies for any automatic voucher generation
            if (dealer != null && totalPoints > 0)
            {
                // Check PointsToCash campaigns for automatic voucher generation (allocate in multiples)
                var pointsToCashCampaigns = await _context.PointsToCashCampaigns
                    .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Found {pointsToCashCampaigns.Count} active PointsToCash campaigns");

                foreach (var campaign in pointsToCashCampaigns)
                {
                    System.Diagnostics.Debug.WriteLine($"Checking campaign {campaign.Id}: {campaign.CampaignName}, Threshold: {campaign.VoucherGenerationThreshold}, Dealer Points: {totalPoints}");

                    if (totalPoints >= campaign.VoucherGenerationThreshold)
                    {
                        int eligibleCount = totalPoints / campaign.VoucherGenerationThreshold;

                        var existingCount = await _context.Vouchers
                            .Where(v => v.DealerId == dealer.Id && v.CampaignId == campaign.Id && v.CampaignType == "PointsToCash")
                            .CountAsync();

                        int toCreate = eligibleCount - existingCount;
                        if (toCreate > 0)
                        {
                            var vouchers = new List<Voucher>();
                            for (int i = 0; i < toCreate; i++)
                            {
                                var voucherCode = $"PTC{dealer.Id}{campaign.Id}{DateTime.UtcNow:yyyyMMddHHmmss}{i}";
                                var voucher = new Voucher
                                {
                                    VoucherCode = voucherCode,
                                    DealerId = dealer.Id,
                                    CampaignType = "PointsToCash",
                                    CampaignId = campaign.Id,
                                    VoucherValue = campaign.VoucherValue,
                                    PointsUsed = campaign.VoucherGenerationThreshold,
                                    IssueDate = DateTime.UtcNow,
                                    ExpiryDate = DateTime.UtcNow.AddDays(campaign.VoucherValidity),
                                    QRCodeData = $"{voucherCode}|{dealer.Id}|{campaign.VoucherValue}|{DateTime.UtcNow.AddDays(campaign.VoucherValidity):yyyy-MM-dd}"
                                };

                                vouchers.Add(voucher);
                            }

                            _context.Vouchers.AddRange(vouchers);
                            await _context.SaveChangesAsync();

                            foreach (var v in vouchers)
                            {
                                await _notificationService.CreateVoucherNotificationAsync(user.Id, v.VoucherCode, v.VoucherValue, v.Id);
                            }

                            System.Diagnostics.Debug.WriteLine($"Created {toCreate} PointsToCash vouchers for dealer {dealer.Id} campaign {campaign.Id}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No new PointsToCash vouchers needed for dealer {dealer.Id} campaign {campaign.Id}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} has {totalPoints} points, but needs {campaign.VoucherGenerationThreshold} for campaign {campaign.Id}");
                    }
                }

                // Check PointsReward campaigns for automatic voucher generation
                var pointsRewardCampaigns = await _context.PointsRewardCampaigns
                    .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                    .ToListAsync();
                
                // Log campaign count for debugging
                System.Diagnostics.Debug.WriteLine($"Found {pointsRewardCampaigns.Count} active PointsReward campaigns");

                foreach (var campaign in pointsRewardCampaigns)
                {
                    System.Diagnostics.Debug.WriteLine($"Checking campaign {campaign.Id}: {campaign.CampaignName}, Threshold: {campaign.VoucherGenerationThreshold}, Dealer Points: {totalPoints}");

                    if (totalPoints >= campaign.VoucherGenerationThreshold)
                    {
                        int eligibleCount = totalPoints / campaign.VoucherGenerationThreshold;

                        var existingCount = await _context.Vouchers
                            .Where(v => v.DealerId == dealer.Id && v.CampaignId == campaign.Id && v.CampaignType == "PointsReward")
                            .CountAsync();

                        int toCreate = eligibleCount - existingCount;
                        if (toCreate > 0)
                        {
                            var vouchers = new List<Voucher>();
                            for (int i = 0; i < toCreate; i++)
                            {
                                var voucherCode = $"PTR{dealer.Id}{campaign.Id}{DateTime.UtcNow:yyyyMMddHHmmss}{i}";
                                var voucher = new Voucher
                                {
                                    VoucherCode = voucherCode,
                                    DealerId = dealer.Id,
                                    CampaignType = "PointsReward",
                                    CampaignId = campaign.Id,
                                    VoucherValue = 100,
                                    PointsUsed = campaign.VoucherGenerationThreshold,
                                    IssueDate = DateTime.UtcNow,
                                    ExpiryDate = DateTime.UtcNow.AddDays(campaign.VoucherValidity),
                                    QRCodeData = $"{voucherCode}|{dealer.Id}|100|{DateTime.UtcNow.AddDays(campaign.VoucherValidity):yyyy-MM-dd}"
                                };

                                vouchers.Add(voucher);
                            }

                            _context.Vouchers.AddRange(vouchers);
                            await _context.SaveChangesAsync();

                            foreach (var v in vouchers)
                            {
                                await _notificationService.CreateVoucherNotificationAsync(user.Id, v.VoucherCode, v.VoucherValue, v.Id);
                            }

                            System.Diagnostics.Debug.WriteLine($"Created {toCreate} PointsReward vouchers for dealer {dealer.Id} campaign {campaign.Id}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No new PointsReward vouchers needed for dealer {dealer.Id} campaign {campaign.Id}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} has {totalPoints} points, but needs {campaign.VoucherGenerationThreshold} for campaign {campaign.Id}");
                    }
                }

                // Check AmountReachGoal campaigns for automatic voucher generation
                var amountReachGoalCampaigns = await _context.AmountReachGoalCampaigns
                    .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                    .ToListAsync();

                // Log campaign count for debugging
                System.Diagnostics.Debug.WriteLine($"Found {amountReachGoalCampaigns.Count} active AmountReachGoal campaigns");

                foreach (var campaign in amountReachGoalCampaigns)
                {
                    // Calculate total order amount for this dealer
                    var totalOrderAmount = await _context.Orders
                        .Where(o => o.DealerId == dealer.Id)
                        .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                    // Log campaign information for debugging
                    System.Diagnostics.Debug.WriteLine($"Checking AmountReachGoal campaign {campaign.Id}: {campaign.CampaignName}, Target: {campaign.TargetAmount}, Dealer Total: {totalOrderAmount}");

                    // Check if dealer has reached the target amount for this campaign
                    if (totalOrderAmount >= campaign.TargetAmount)
                    {
                        // Check if dealer already has a voucher for this campaign
                        var existingVoucher = await _context.Vouchers
                            .AnyAsync(v => v.DealerId == dealer.Id && v.CampaignId == campaign.Id && v.CampaignType == "AmountReachGoal");

                        if (!existingVoucher)
                        {
                            // Generate voucher
                            var voucherCode = $"ARG{dealer.Id}{campaign.Id}{DateTime.UtcNow:yyyyMMddHHmmss}";
                            var voucher = new Voucher
                            {
                                VoucherCode = voucherCode,
                                DealerId = dealer.Id,
                                CampaignType = "AmountReachGoal",
                                CampaignId = campaign.Id,
                                VoucherValue = campaign.VoucherValue,
                                PointsUsed = 0, // No points used for this campaign type
                                IssueDate = DateTime.UtcNow,
                                ExpiryDate = DateTime.UtcNow.AddDays(campaign.VoucherValidity),
                                QRCodeData = $"{voucherCode}|{dealer.Id}|{campaign.VoucherValue}|{DateTime.UtcNow.AddDays(campaign.VoucherValidity):yyyy-MM-dd}"
                            };

                            _context.Vouchers.Add(voucher);
                            await _context.SaveChangesAsync();

                            // Log successful voucher creation for debugging
                            System.Diagnostics.Debug.WriteLine($"Created AmountReachGoal voucher {voucherCode} for dealer {dealer.Id} and campaign {campaign.Id}");

                            // Create notification for voucher generation
                            await _notificationService.CreateVoucherNotificationAsync(user.Id, voucherCode, campaign.VoucherValue, voucher.Id);
                        }
                        else
                        {
                            // Log that voucher already exists
                            System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} already has a voucher for AmountReachGoal campaign {campaign.Id}");
                        }
                    }
                    else
                    {
                        // Log insufficient order amount
                        System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} has {totalOrderAmount} total order amount, but needs {campaign.TargetAmount} for campaign {campaign.Id}");
                    }
                }

                // Check FreeProduct campaigns for automatic voucher generation
                var freeProductCampaigns = await _context.FreeProductCampaigns
                    .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                    .ToListAsync();

                // Log campaign count for debugging
                System.Diagnostics.Debug.WriteLine($"Found {freeProductCampaigns.Count} active FreeProduct campaigns");

                foreach (var campaign in freeProductCampaigns)
                {
                    // Check if dealer has ordered enough quantities of campaign materials
                    if (!string.IsNullOrEmpty(campaign.Materials) && !string.IsNullOrEmpty(campaign.MaterialQuantities))
                    {
                        try
                        {
                            var campaignMaterialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                            var materialQuantities = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialQuantities);

                            // Calculate total quantities of campaign materials ordered by this dealer
                            var orderedMaterialQuantities = await _context.OrderItems
                                .Where(oi => oi.Order.DealerId == dealer.Id && campaignMaterialIds.Contains(oi.MaterialId))
                                .GroupBy(oi => oi.MaterialId)
                                .ToDictionaryAsync(g => g.Key, g => g.Sum(oi => oi.Quantity));

                            bool hasSufficientQuantities = true;
                            foreach (var materialId in campaignMaterialIds)
                            {
                                if (materialQuantities.ContainsKey(materialId))
                                {
                                    var requiredQuantity = materialQuantities[materialId];
                                    var orderedQuantity = orderedMaterialQuantities.ContainsKey(materialId) ? orderedMaterialQuantities[materialId] : 0;

                                    if (orderedQuantity < requiredQuantity)
                                    {
                                        hasSufficientQuantities = false;
                                        break;
                                    }
                                }
                            }

                            // Log campaign information for debugging
                            System.Diagnostics.Debug.WriteLine($"Checking FreeProduct campaign {campaign.Id}: {campaign.CampaignName}, Sufficient Quantities: {hasSufficientQuantities}");

                            // Check if dealer has ordered sufficient quantities
                            if (hasSufficientQuantities)
                            {
                                // Determine how many full sets of required quantities dealer has ordered
                                int minMultiplier = int.MaxValue;
                                foreach (var materialId in campaignMaterialIds)
                                {
                                    if (materialQuantities.ContainsKey(materialId))
                                    {
                                        var requiredQuantity = materialQuantities[materialId];
                                        var orderedQuantity = orderedMaterialQuantities.ContainsKey(materialId) ? orderedMaterialQuantities[materialId] : 0;
                                        int multiplier = orderedQuantity / requiredQuantity;
                                        if (multiplier < minMultiplier) minMultiplier = multiplier;
                                    }
                                }

                                if (minMultiplier == int.MaxValue) minMultiplier = 0;

                                // Count existing vouchers for this campaign
                                var existingCount = await _context.Vouchers
                                    .Where(v => v.DealerId == dealer.Id && v.CampaignId == campaign.Id && v.CampaignType == "FreeProduct")
                                    .CountAsync();

                                int toCreate = Math.Max(0, minMultiplier - existingCount);

                                if (toCreate > 0)
                                {
                                    var vouchers = new List<Voucher>();
                                    for (int i = 0; i < toCreate; i++)
                                    {
                                        var voucherCode = $"FRP{dealer.Id}{campaign.Id}{DateTime.UtcNow:yyyyMMddHHmmss}{i}";
                                        var voucher = new Voucher
                                        {
                                            VoucherCode = voucherCode,
                                            DealerId = dealer.Id,
                                            CampaignType = "FreeProduct",
                                            CampaignId = campaign.Id,
                                            VoucherValue = 0,
                                            PointsUsed = 0,
                                            IssueDate = DateTime.UtcNow,
                                            ExpiryDate = DateTime.UtcNow.AddDays(30),
                                            QRCodeData = $"{voucherCode}|{dealer.Id}|0|{DateTime.UtcNow.AddDays(30):yyyy-MM-dd}"
                                        };

                                        vouchers.Add(voucher);
                                    }

                                    _context.Vouchers.AddRange(vouchers);
                                    await _context.SaveChangesAsync();

                                    foreach (var v in vouchers)
                                    {
                                        await _notificationService.CreateVoucherNotificationAsync(user.Id, v.VoucherCode, v.VoucherValue, v.Id);
                                    }

                                    System.Diagnostics.Debug.WriteLine($"Created {toCreate} FreeProduct vouchers for dealer {dealer.Id} campaign {campaign.Id}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"No new FreeProduct vouchers needed for dealer {dealer.Id} campaign {campaign.Id}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} has insufficient quantities for FreeProduct campaign {campaign.Id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log any errors during processing
                            System.Diagnostics.Debug.WriteLine($"Error processing FreeProduct campaign {campaign.Id}: {ex.Message}");
                        }
                    }
                }

                // Note: SessionDurationReward campaigns would require session tracking which is not implemented in the current codebase
                // This would typically involve tracking user sessions and comparing duration to campaign requirements
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
                
                // Get the most recent DealerBasicOrders for the logged-in dealer
                RecentDealerBasicOrders = dealer != null ? await _context.DealerBasicOrders
                    .Where(dbo => dbo.DealerId == dealer.Id)
                    .OrderByDescending(dbo => dbo.Id) // Assuming Id is sequential, otherwise add a date field
                    .Take(10) // Limit to 10 most recent orders
                    .ToListAsync() : new List<DealerBasicOrder>(),
                    
                // Set the total points for the dealer
                TotalPoints = totalPoints
            };
            
            return View("~/Views/Home/Dealer/DealerHome.cshtml", viewModel);
        }

        public async Task<IActionResult> ShopkeeperHome(bool showProductForm = false, int voucherId = 0)
        {
            // Check if user is logged in and is a Shopkeeper
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Shopkeeper")
            {
                return RedirectToAction("Login");
            }

            // Hide bottom navigation for ShopkeeperHome
            ViewData["HideLayoutBottomNav"] = "true";

            try
            {
                // Get the logged-in user's information
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                // Get the shopkeeper information for this user
                ShopkeeperMaster shopkeeper = null;
                if (user != null)
                {
                    // Assuming the ShopkeeperMaster.PhoneNumber corresponds to the User.phoneno for shopkeepers
                    shopkeeper = await _context.ShopkeeperMasters.FirstOrDefaultAsync(s => s.PhoneNumber == user.phoneno);
                }

                if (shopkeeper == null)
                {
                    // If no shopkeeper found, redirect to login
                    return RedirectToAction("Login");
                }

                // Get all vouchers that can be redeemed by this shopkeeper (not redeemed yet and not expired)
                var vouchers = await _context.Vouchers
                    .Where(v => !v.IsRedeemed && v.ExpiryDate > DateTime.UtcNow)
                    .OrderByDescending(v => v.IssueDate)
                    .ToListAsync();

                // Get redemption history (recently redeemed vouchers)
                // Include dealer information for better context
                var redemptionHistory = await _context.Vouchers
                    .Include(v => v.Dealer)
                    .Where(v => v.IsRedeemed)
                    .OrderByDescending(v => v.RedeemedDate)
                    .Take(20) // Limit to 20 most recent redemptions
                    .ToListAsync();

                // Generate QR code data for each voucher
                var voucherQRCodeData = new Dictionary<int, string>();
                foreach (var voucher in vouchers)
                {
                    if (string.IsNullOrEmpty(voucher.QRCodeData))
                    {
                        // Generate QR code data if not present
                        voucher.QRCodeData = $"{voucher.VoucherCode}|{voucher.DealerId}|{voucher.VoucherValue}|{voucher.ExpiryDate:yyyy-MM-dd}";
                        _context.Vouchers.Update(voucher);
                    }

                    // Generate QR code image
                    voucherQRCodeData[voucher.Id] = GenerateQRCodeBase64(voucher.QRCodeData);
                }

                // Save any changes to QR code data
                await _context.SaveChangesAsync();

                var viewModel = new VoucherViewModel
                {
                    Vouchers = vouchers,
                    VoucherQRCodeData = voucherQRCodeData,
                    Shopkeeper = shopkeeper,
                    RedemptionHistory = redemptionHistory, // Add redemption history to the view model
                    ShowProductForm = showProductForm,
                    VoucherId = voucherId
                };

                return View("~/Views/Home/Shopkeeper/ShopkeeperHome.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in ShopkeeperHome action: {ex.Message}");
                // Redirect to login if there's an error
                return RedirectToAction("Login");
            }
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

        // New Sales Dashboard
        public IActionResult SalesHome()
        {
            // Check if user is logged in and is a Sales
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Sales")
            {
                return RedirectToAction("Login");
            }
            
            return View("~/Views/Home/Sales/SalesHome.cshtml");
        }

        // New Customer Dashboard
        public IActionResult CustomerHome()
        {
            // Check if user is logged in and is a Customer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Customer")
            {
                return RedirectToAction("Login");
            }
            
            return View("~/Views/Home/Customer/CustomerHome.cshtml");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Test action to seed DealerBasicOrders data
        public async Task<IActionResult> SeedDealerBasicOrders()
        {
            // Check if user is logged in and is an Admin
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Get a test dealer
                var dealer = await _context.DealerMasters.FirstOrDefaultAsync();
                if (dealer == null)
                {
                    ViewBag.Message = "No dealer found in the database.";
                    return View("Index");
                }

                // Create test DealerBasicOrders
                var testOrders = new List<DealerBasicOrder>
                {
                    new DealerBasicOrder
                    {
                        DealerId = dealer.Id,
                        MaterialName = "Test Product 1",
                        SapCode = "TP001",
                        ShortCode = "TST1",
                        Quantity = 5,
                        Rate = 10.50m
                    },
                    new DealerBasicOrder
                    {
                        DealerId = dealer.Id,
                        MaterialName = "Test Product 2",
                        SapCode = "TP002",
                        ShortCode = "TST2",
                        Quantity = 3,
                        Rate = 15.75m
                    },
                    new DealerBasicOrder
                    {
                        DealerId = dealer.Id,
                        MaterialName = "Test Product 3",
                        SapCode = "TP003",
                        ShortCode = "TST3",
                        Quantity = 2,
                        Rate = 20.00m
                    }
                };

                _context.DealerBasicOrders.AddRange(testOrders);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Successfully added 3 test DealerBasicOrders to the database.";
                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error seeding DealerBasicOrders: {ex.Message}";
                return View("Index");
            }
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
                else if (userRole == "Sales")
                {
                    return RedirectToAction("SalesHome");
                }
                else if (userRole == "Customer")
                {
                    return RedirectToAction("CustomerHome");
                }
                else if (userRole == "Shopkeeper")
                {
                    return RedirectToAction("ShopkeeperHome");
                }
                // For other roles, stay on index
                return RedirectToAction("Index");
            }
            
            // Clear any existing session data to ensure clean login
            HttpContext.Session.Clear();
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
                    else if (obj.Role == "Shopkeeper")
                    {
                        // Shopkeeper gets access to shopkeeper dashboard
                        return RedirectToAction("ShopkeeperHome");
                    }
                    else if (obj.Role == "Sales")
                    {
                        // Sales gets access to sales dashboard
                        return RedirectToAction("SalesHome");
                    }
                    else if (obj.Role == "Customer")
                    {
                        // Customer gets access to customer dashboard
                        return RedirectToAction("CustomerHome");
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
                else if (userRole == "Shopkeeper")
                {
                    return RedirectToAction("ShopkeeperHome");
                }
                else if (userRole == "Sales")
                {
                    return RedirectToAction("SalesHome");
                }
                else if (userRole == "Customer")
                {
                    return RedirectToAction("CustomerHome");
                }
                return RedirectToAction("Index");
            }
        }
        
        public IActionResult Logout()
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

                // Calculate summary values server-side and pass to view via ViewData
                var totalPointsEarned = orderItems?.Sum(oi => oi.Points) ?? 0;
                var pointsUsed = await _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id)
                    .SumAsync(v => (int?)v.PointsUsed) ?? 0;
                var availablePoints = totalPointsEarned - pointsUsed;

                ViewData["TotalPointsEarned"] = totalPointsEarned;
                ViewData["AvailablePoints"] = availablePoints;
                ViewData["PointsUsed"] = pointsUsed;

                return View("~/Views/Home/Dealer/PointsDetails.cshtml", orderItems);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in PointsDetails action: {ex.Message}");
                // Redirect to dealer home page if there's an error
                return RedirectToAction("DealerHome");
            }
        }

        public async Task<IActionResult> History()
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

                // Get redemption history (recently redeemed vouchers) for this dealer
                // Include dealer information for better context
                var redemptionHistory = await _context.Vouchers
                    .Include(v => v.Dealer)
                    .Where(v => v.IsRedeemed && v.DealerId == dealer.Id)
                    .OrderByDescending(v => v.RedeemedDate)
                    .ToListAsync();

                var viewModel = new VoucherViewModel
                {
                    RedemptionHistory = redemptionHistory,
                    Dealer = dealer
                };

                return View("~/Views/Home/Dealer/History.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in History action: {ex.Message}");
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("role");
            HttpContext.Session.Remove("name");
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Vouchers()
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

                // Get all vouchers for this dealer and order so redeemed vouchers appear at the end
                var vouchers = await _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id)
                    .OrderBy(v => v.IsRedeemed)
                    .ThenByDescending(v => v.ExpiryDate)
                    .ThenByDescending(v => v.IssueDate)                   
                    .Take(10).ToListAsync();
                // Log voucher count for debugging
                System.Diagnostics.Debug.WriteLine($"Found {vouchers.Count} vouchers for dealer {dealer.Id}");

                // Ensure all vouchers have QR code data
                var voucherQRCodeData = new Dictionary<int, string>();
                foreach (var voucher in vouchers)
                {
                    if (string.IsNullOrEmpty(voucher.QRCodeData))
                    {
                        // Generate QR code data if not present
                        voucher.QRCodeData = $"{voucher.VoucherCode}|{dealer.Id}|{voucher.VoucherValue}|{voucher.ExpiryDate:yyyy-MM-dd}";
                        _context.Vouchers.Update(voucher);
                    }
                    
                    // Generate QR code image
                    voucherQRCodeData[voucher.Id] = GenerateQRCodeBase64(voucher.QRCodeData);
                }
                
                // Save any changes to QR code data
                await _context.SaveChangesAsync();

                // Calculate total points for the dealer
                var totalPoints = await _context.OrderItems
                    .Where(oi => oi.Order.DealerId == dealer.Id)
                    .SumAsync(oi => (int?)oi.Points) ?? 0;
                
                // Log total points for debugging
                System.Diagnostics.Debug.WriteLine($"Dealer {dealer.Id} has {totalPoints} total points");

                // Get campaign details for each voucher
                var voucherCampaignDetails = new Dictionary<int, VoucherCampaignDetails>();
                
                foreach (var voucher in vouchers)
                {
                    var campaignDetails = new VoucherCampaignDetails
                    {
                        CampaignType = voucher.CampaignType
                    };
                    
                    switch (voucher.CampaignType)
                    {
                        case "PointsToCash":
                            var pointsToCashCampaign = await _context.PointsToCashCampaigns
                                .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (pointsToCashCampaign != null)
                            {
                                campaignDetails.CampaignName = pointsToCashCampaign.CampaignName;
                                campaignDetails.VoucherValue = pointsToCashCampaign.VoucherValue;
                            }
                            break;
                            
                        case "PointsReward":
                            var pointsRewardCampaign = await _context.PointsRewardCampaigns
                                .Include(c => c.RewardProduct)
                                .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (pointsRewardCampaign != null)
                            {
                                campaignDetails.CampaignName = pointsRewardCampaign.CampaignName;
                                campaignDetails.RewardProductName = pointsRewardCampaign.RewardProduct?.ProductName ?? "Reward Product";
                            }
                            break;
                            
                        case "AmountReachGoal":
                            var amountReachGoalCampaign = await _context.AmountReachGoalCampaigns
                                .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (amountReachGoalCampaign != null)
                            {
                                campaignDetails.CampaignName = amountReachGoalCampaign.CampaignName;
                                campaignDetails.VoucherValue = amountReachGoalCampaign.VoucherValue;
                                campaignDetails.TargetAmount = amountReachGoalCampaign.TargetAmount;
                            }
                            break;
                            
                        case "SessionDurationReward":
                            var sessionDurationCampaign = await _context.SessionDurationRewardCampaigns
                                .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (sessionDurationCampaign != null)
                            {
                                campaignDetails.CampaignName = sessionDurationCampaign.CampaignName;
                                campaignDetails.VoucherValue = sessionDurationCampaign.VoucherValue;
                                campaignDetails.SessionDuration = sessionDurationCampaign.SessionDuration;
                            }
                            break;
                            
                        case "FreeProduct":
                            var freeProductCampaign = await _context.FreeProductCampaigns
                                .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (freeProductCampaign != null)
                            {
                                campaignDetails.CampaignName = freeProductCampaign.CampaignName;
                                
                                // Parse free product details if available
                                if (!string.IsNullOrEmpty(freeProductCampaign.FreeProducts) && 
                                    !string.IsNullOrEmpty(freeProductCampaign.FreeQuantities))
                                {
                                    try
                                    {
                                        var freeProducts = JsonSerializer.Deserialize<Dictionary<int, int>>(freeProductCampaign.FreeProducts);
                                        var freeQuantities = JsonSerializer.Deserialize<Dictionary<int, int>>(freeProductCampaign.FreeQuantities);
                                        
                                        // Get product names for the free products
                                        foreach (var kvp in freeProducts)
                                        {
                                            var productId = kvp.Value;
                                            var quantity = freeQuantities.ContainsKey(kvp.Key) ? freeQuantities[kvp.Key] : 0;
                                            
                                            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
                                            if (product != null)
                                            {
                                                campaignDetails.FreeProducts[product.ProductName] = quantity;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // Handle deserialization errors
                                    }
                                }
                            }
                            break;
                    }
                    
                    voucherCampaignDetails[voucher.Id] = campaignDetails;
                }

                var viewModel = new VoucherViewModel
                {
                    Vouchers = vouchers,
                    TotalPoints = totalPoints,
                    Dealer = dealer,
                    VoucherQRCodeData = voucherQRCodeData,
                    VoucherCampaignDetails = voucherCampaignDetails
                };

                return View("~/Views/Home/Dealer/Vouchers.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in Vouchers action: {ex.Message}");
                // Redirect to dealer home page if there's an error
                return RedirectToAction("DealerHome");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateVoucher(int campaignId, string campaignType, int pointsToUse)
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

                // Validate that the dealer has enough points
                var totalPoints = await _context.OrderItems
                    .Where(oi => oi.Order.DealerId == dealer.Id)
                    .SumAsync(oi => (int?)oi.Points) ?? 0;

                if (totalPoints < pointsToUse)
                {
                    TempData["ErrorMessage"] = "Insufficient points to generate this voucher.";
                    return RedirectToAction("Vouchers");
                }

                // Get campaign details to determine voucher value
                decimal voucherValue = 0;
                int voucherValidity = 30; // Default validity

                if (campaignType == "PointsToCash")
                {
                    var campaign = await _context.PointsToCashCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
                    if (campaign != null)
                    {
                        voucherValue = campaign.VoucherValue;
                        voucherValidity = campaign.VoucherValidity;
                    }
                }
                else if (campaignType == "PointsReward")
                {
                    var campaign = await _context.PointsRewardCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
                    if (campaign != null)
                    {
                        voucherValidity = campaign.VoucherValidity;
                        // For PointsReward campaigns, we might want to set a default value or calculate based on reward product
                        voucherValue = 100; // Default value
                    }
                }

                // Generate unique voucher code
                var voucherCode = $"V{dealer.Id}{DateTime.UtcNow:yyyyMMddHHmmss}";

                // Create voucher
                var voucher = new Voucher
                {
                    VoucherCode = voucherCode,
                    DealerId = dealer.Id,
                    CampaignType = campaignType,
                    CampaignId = campaignId,
                    VoucherValue = voucherValue,
                    PointsUsed = pointsToUse,
                    IssueDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(voucherValidity),
                    QRCodeData = $"{voucherCode}|{dealer.Id}|{voucherValue}|{DateTime.UtcNow.AddDays(voucherValidity):yyyy-MM-dd}"
                };

                _context.Vouchers.Add(voucher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Voucher generated successfully! Code: {voucherCode}";
                return RedirectToAction("Vouchers");
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in GenerateVoucher action: {ex.Message}");
                TempData["ErrorMessage"] = "Error generating voucher. Please try again.";
                return RedirectToAction("Vouchers");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVouchersPage(int page = 1, int pageSize = 10, string status = null, string campaignType = null)
        {
            // Returns paginated vouchers for the logged-in dealer as JSON
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return Unauthorized();
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                DealerMaster dealer = null;
                if (user != null)
                {
                    dealer = await _context.DealerMasters.FirstOrDefaultAsync(d => d.PhoneNo == user.phoneno);
                }

                if (dealer == null)
                {
                    return Unauthorized();
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id);

                // Apply optional filters
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status)
                    {
                        case "Redeemed":
                            query = query.Where(v => v.IsRedeemed);
                            break;
                        case "Expired":
                            query = query.Where(v => v.ExpiryDate < DateTime.UtcNow);
                            break;
                        case "Active":
                            query = query.Where(v => !v.IsRedeemed && v.ExpiryDate >= DateTime.UtcNow);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(campaignType))
                {
                    if (campaignType != "All")
                    {
                        query = query.Where(v => v.CampaignType == campaignType);
                    }
                }

                // Order so redeemed vouchers appear at the end
                query = query.OrderBy(v => v.IsRedeemed)
                    .ThenByDescending(v => v.ExpiryDate)
                    .ThenByDescending(v => v.IssueDate);

                var total = await query.CountAsync();
                var skip = (page - 1) * pageSize;
                var vouchers = await query.Skip(skip).Take(pageSize).ToListAsync();

                // Ensure QR code data for returned vouchers
                var updated = false;
                foreach (var voucher in vouchers)
                {
                    if (string.IsNullOrEmpty(voucher.QRCodeData))
                    {
                        voucher.QRCodeData = $"{voucher.VoucherCode}|{dealer.Id}|{voucher.VoucherValue}|{voucher.ExpiryDate:yyyy-MM-dd}";
                        _context.Vouchers.Update(voucher);
                        updated = true;
                    }
                }

                if (updated)
                {
                    await _context.SaveChangesAsync();
                }

                // Build response objects with campaign details and QR code
                var resultList = new List<object>();
                foreach (var voucher in vouchers)
                {
                    var campaignName = string.Empty;
                    var freeProducts = new Dictionary<string, int>();
                    var rewardProductName = string.Empty;

                    switch (voucher.CampaignType)
                    {
                        case "PointsToCash":
                            var ptc = await _context.PointsToCashCampaigns.FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (ptc != null) campaignName = ptc.CampaignName;
                            break;
                        case "PointsReward":
                            var pr = await _context.PointsRewardCampaigns.Include(c => c.RewardProduct).FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (pr != null)
                            {
                                campaignName = pr.CampaignName;
                                rewardProductName = pr.RewardProduct?.ProductName ?? string.Empty;
                            }
                            break;
                        case "AmountReachGoal":
                            var arg = await _context.AmountReachGoalCampaigns.FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (arg != null) campaignName = arg.CampaignName;
                            break;
                        case "SessionDurationReward":
                            var sdr = await _context.SessionDurationRewardCampaigns.FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (sdr != null) campaignName = sdr.CampaignName;
                            break;
                        case "FreeProduct":
                            var fp = await _context.FreeProductCampaigns.FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);
                            if (fp != null)
                            {
                                campaignName = fp.CampaignName;
                                try
                                {
                                    if (!string.IsNullOrEmpty(fp.FreeProducts) && !string.IsNullOrEmpty(fp.FreeQuantities))
                                    {
                                        var freeMap = JsonSerializer.Deserialize<Dictionary<int, int>>(fp.FreeProducts);
                                        var qtyMap = JsonSerializer.Deserialize<Dictionary<int, int>>(fp.FreeQuantities);
                                        if (freeMap != null)
                                        {
                                            foreach (var kv in freeMap)
                                            {
                                                var prod = await _context.Products.FirstOrDefaultAsync(p => p.Id == kv.Value);
                                                if (prod != null)
                                                {
                                                    var qty = qtyMap != null && qtyMap.ContainsKey(kv.Key) ? qtyMap[kv.Key] : 0;
                                                    freeProducts[prod.ProductName] = qty;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                            break;
                    }

                    resultList.Add(new
                    {
                        voucher.Id,
                        voucher.VoucherCode,
                        voucher.VoucherValue,
                        IssueDate = voucher.IssueDate.ToString("o"),
                        ExpiryDate = voucher.ExpiryDate.ToString("o"),
                        voucher.IsRedeemed,
                        RedeemedDate = voucher.RedeemedDate.HasValue ? voucher.RedeemedDate.Value.ToString("o") : null,
                        voucher.PointsUsed,
                        voucher.CampaignType,
                        CampaignName = campaignName,
                        FreeProducts = freeProducts,
                        RewardProductName = rewardProductName,
                        QRCodeBase64 = GenerateQRCodeBase64(voucher.QRCodeData)
                    });
                }

                var hasMore = skip + vouchers.Count < total;

                return Ok(new { vouchers = resultList, hasMore, nextPage = page + 1 });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetVouchersPage: {ex.Message}");
                return StatusCode(500);
            }
        }

        private string GenerateQRCodeBase64(string data)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new Base64QRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20); // 20 pixels per module
                }
            }
            catch
            {
                // Return a default/base64 encoded placeholder if QR generation fails
                return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="; // 1x1 transparent PNG
            }
        }

        [HttpPost]
        public async Task<IActionResult> RedeemVoucher(string voucherCode)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            // Get user role
            var userRole = HttpContext.Session.GetString("role");

            try
            {
                // Handle redemption based on user role
                if (userRole == "Dealer")
                {
                    // Existing dealer redemption logic
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

                    // Find the voucher
                    var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == voucherCode && v.DealerId == dealer.Id);

                    if (voucher == null)
                    {
                        TempData["ErrorMessage"] = "Voucher not found.";
                        return RedirectToAction("Vouchers");
                    }

                    // Check if voucher is already redeemed
                    if (voucher.IsRedeemed)
                    {
                        TempData["ErrorMessage"] = "This voucher has already been redeemed.";
                        return RedirectToAction("Vouchers");
                    }

                    // Check if voucher is expired
                    if (DateTime.UtcNow > voucher.ExpiryDate)
                    {
                        TempData["ErrorMessage"] = "This voucher has expired.";
                        return RedirectToAction("Vouchers");
                    }

                    // Redeem the voucher
                    voucher.IsRedeemed = true;
                    voucher.RedeemedDate = DateTime.UtcNow;

                    _context.Vouchers.Update(voucher);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully!";
                    return RedirectToAction("Vouchers");
                }
                else if (userRole == "Shopkeeper")
                {
                    // Shopkeeper redemption logic
                    // Get the logged-in user's information
                    var userName = HttpContext.Session.GetString("UserName");
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                    // Get the shopkeeper information for this user
                    ShopkeeperMaster shopkeeper = null;
                    if (user != null)
                    {
                        // Assuming the ShopkeeperMaster.PhoneNumber corresponds to the User.phoneno for shopkeepers
                        shopkeeper = await _context.ShopkeeperMasters.FirstOrDefaultAsync(s => s.PhoneNumber == user.phoneno);
                    }

                    if (shopkeeper == null)
                    {
                        // If no shopkeeper found, redirect to login
                        return RedirectToAction("Login");
                    }

                    // Find the voucher (shopkeeper can redeem any valid voucher)
                    var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == voucherCode);

                    if (voucher == null)
                    {
                        TempData["ErrorMessage"] = "Voucher not found.";
                        return RedirectToAction("ShopkeeperHome");
                    }

                    // Check if voucher is already redeemed
                    if (voucher.IsRedeemed)
                    {
                        TempData["ErrorMessage"] = "This voucher has already been redeemed.";
                        return RedirectToAction("ShopkeeperHome");
                    }

                    // Check if voucher is expired
                    if (DateTime.UtcNow > voucher.ExpiryDate)
                    {
                        TempData["ErrorMessage"] = "This voucher has expired.";
                        return RedirectToAction("ShopkeeperHome");
                    }

                    // Redeem the voucher
                    voucher.IsRedeemed = true;
                    voucher.RedeemedDate = DateTime.UtcNow;

                    _context.Vouchers.Update(voucher);
                    await _context.SaveChangesAsync();

                    // If this is a Points-to-Cash voucher, show product details form
                    if (voucher.CampaignType == "PointsToCash")
                    {
                        TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully! Please enter product details.";
                        return RedirectToAction("ShopkeeperHome", new { showProductForm = true, voucherId = voucher.Id });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully!";
                        return RedirectToAction("ShopkeeperHome");
                    }
                }
                else
                {
                    // For other roles, redirect to index
                    TempData["ErrorMessage"] = "You don't have permission to redeem vouchers.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in RedeemVoucher action: {ex.Message}");
                TempData["ErrorMessage"] = "Error redeeming voucher. Please try again.";

                // Redirect based on user role
                if (userRole == "Dealer")
                {
                    return RedirectToAction("Vouchers");
                }
                else if (userRole == "Shopkeeper")
                {
                    return RedirectToAction("ShopkeeperHome");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RedeemVoucherByQR([FromBody] QRScanModel model)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return Json(new { success = false, message = "You must be logged in to redeem vouchers." });
            }
            
            // Get user role
            var userRole = HttpContext.Session.GetString("role");
            
            if (userRole != "Shopkeeper")
            {
                return Json(new { success = false, message = "Only shopkeepers can redeem vouchers." });
            }
            
            try
            {
                // Parse the QR code data
                // Expected format (common): {VoucherCode}|{DealerId}|{VoucherValue}|{ExpiryDate}
                // But QR may sometimes contain only the voucher code or a URL/query containing the code.
                var raw = (model?.QrData ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(raw))
                {
                    return Json(new { success = false, message = "Invalid QR code data." });
                }

                string voucherCode = null;
                var qrDataParts = raw.Split('|');
                if (qrDataParts.Length >= 4)
                {
                    voucherCode = qrDataParts[0].Trim();
                }
                else if (qrDataParts.Length == 1)
                {
                    // raw may be a plain code, a URL with query, or a path
                    var match = Regex.Match(raw, @"(?:code|voucherCode|voucher)=([^&\s]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        voucherCode = Uri.UnescapeDataString(match.Groups[1].Value);
                    }
                    else if (raw.Contains('/'))
                    {
                        var segs = raw.TrimEnd('/').Split('/');
                        voucherCode = Uri.UnescapeDataString(segs.Last());
                    }
                    else
                    {
                        voucherCode = Uri.UnescapeDataString(raw);
                    }
                }
                else
                {
                    // Fallback: take the first segment as voucher code
                    voucherCode = qrDataParts[0].Trim();
                }
                
                // Find the voucher by code
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == voucherCode);
                
                if (voucher == null)
                {
                    return Json(new { success = false, message = "Voucher not found." });
                }
                
                // Check if voucher is already redeemed
                if (voucher.IsRedeemed)
                {
                    return Json(new { success = false, message = "This voucher has already been redeemed." });
                }

                // Check if voucher is expired
                if (DateTime.UtcNow > voucher.ExpiryDate)
                {
                    return Json(new { success = false, message = "This voucher has expired." });
                }

                // Redeem the voucher
                voucher.IsRedeemed = true;
                voucher.RedeemedDate = DateTime.UtcNow;

                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();

                // If this is a Points-to-Cash voucher, show product details form
                if (voucher.CampaignType == "PointsToCash")
                {
                    // Return special response to indicate Points-to-Cash voucher redemption
                    return Json(new { success = true, message = $"Voucher {voucher.VoucherCode} redeemed successfully!", requiresProductDetails = true, voucherId = voucher.Id });
                }
                else
                {
                    return Json(new { success = true, message = $"Voucher {voucher.VoucherCode} redeemed successfully!" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in RedeemVoucherByQR action: {ex.Message}");
                return Json(new { success = false, message = "Error redeeming voucher. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveProductDetails(int voucherId, string ProductName, string ProductDescription, decimal ProductPrice, int Quantity)
        {
            // Check if user is logged in and is a Shopkeeper
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Shopkeeper")
            {
                return RedirectToAction("Login");
            }

            try
            {
                // In a real implementation, you would save these product details to a database
                // For now, we'll just show a success message
                
                // Find the voucher to get its details
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == voucherId);
                
                if (voucher == null)
                {
                    TempData["ErrorMessage"] = "Voucher not found.";
                    return RedirectToAction("ShopkeeperHome");
                }
                
                // Here you would typically save the product details to a database table
                // For example:
                // var productDetails = new RedeemedProduct 
                // { 
                //     VoucherId = voucherId, 
                //     ProductName = ProductName, 
                //     Description = ProductDescription, 
                //     Price = ProductPrice, 
                //     Quantity = Quantity,
                //     RedemptionDate = DateTime.UtcNow
                // };
                // _context.RedeemedProducts.Add(productDetails);
                // await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully! Product details saved.";
                return RedirectToAction("ShopkeeperHome");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving product details: {ex.Message}");
                TempData["ErrorMessage"] = "Error saving product details. Please try again.";
                return RedirectToAction("ShopkeeperHome");
            }
        }

        // API endpoint to get notifications for the logged-in dealer
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Get the logged-in user's information
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Get unread notifications for the user
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id && !n.IsRead)
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        time = n.CreatedDate.ToString("yyyy-MM-dd"),
                        badge = GetBadgeText(n.Type)
                    })
                    .ToListAsync();

                return Json(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetNotifications action: {ex.Message}");
                return Json(new { success = false, message = "Error retrieving notifications" });
            }
        }

        // API endpoint to check if there are any notifications in the database
        [HttpGet]
        public async Task<IActionResult> CheckNotifications()
        {
            try
            {
                var count = await _context.Notifications.CountAsync();
                var sample = await _context.Notifications.Take(5).ToListAsync();
                
                return Json(new { 
                    success = true, 
                    totalNotifications = count,
                    sampleNotifications = sample.Select(n => new {
                        n.Id,
                        n.Title,
                        n.Message,
                        n.Type,
                        n.UserId,
                        CreatedDate = n.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        n.IsRead
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API endpoint to get notifications for testing
        [HttpGet]
        public async Task<IActionResult> TestNotifications()
        {
            try
            {
                // Get the logged-in user's information
                var userName = HttpContext.Session.GetString("UserName");
                var userRole = HttpContext.Session.GetString("role");

                if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Get all notifications for the user (both read and unread)
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    count = notifications.Count,
                    notifications = notifications.Select(n => new {
                        n.Id,
                        n.Title,
                        n.Message,
                        n.Type,
                        CreatedDate = n.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        n.IsRead
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API endpoint to create test notifications (for testing purposes)
        [HttpPost]
        public async Task<IActionResult> CreateTestNotifications()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Get the logged-in user's information
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Create test notifications
                var notifications = new[]
                {
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "New Campaign Started",
                        Message = "Points to Cash campaign \"Diwali Special\" has started!",
                        Type = "campaign",
                        CreatedDate = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "Order Placed",
                        Message = "Your order #12345 has been placed successfully!",
                        Type = "order",
                        CreatedDate = DateTime.UtcNow.AddHours(-2)
                    },
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "Voucher Generated",
                        Message = "Voucher V123456789 worth ₹500 has been generated!",
                        Type = "voucher",
                        CreatedDate = DateTime.UtcNow.AddHours(-5)
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Test notifications created successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateTestNotifications action: {ex.Message}");
                return Json(new { success = false, message = "Error creating test notifications" });
            }
        }

        // API endpoint to add test notifications
        [HttpPost]
        public async Task<IActionResult> AddTestNotifications()
        {
            try
            {
                // Get the logged-in user's information
                var userName = HttpContext.Session.GetString("UserName");
                var userRole = HttpContext.Session.GetString("role");

                if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Create test notifications
                var testNotifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "New Campaign Started",
                        Message = "Exciting new campaign 'Summer Sale' is now live!",
                        Type = "campaign",
                        CreatedDate = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "Order Confirmation",
                        Message = "Your order #ORD-7890 has been confirmed and is being processed.",
                        Type = "order",
                        CreatedDate = DateTime.UtcNow.AddHours(-2)
                    },
                    new Notification
                    {
                        UserId = user.Id,
                        Title = "Voucher Available",
                        Message = "You have a new voucher worth ₹500 available for use.",
                        Type = "voucher",
                        CreatedDate = DateTime.UtcNow.AddHours(-5)
                    }
                };

                _context.Notifications.AddRange(testNotifications);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Test notifications added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to get badge text based on notification type
        private string GetBadgeText(string type)
        {
            return type.ToLower() switch
            {
                "campaign" => "Campaign",
                "order" => "Order",
                "voucher" => "Voucher",
                _ => "Notification"
            };
        }
    }
}