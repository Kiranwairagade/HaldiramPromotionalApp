using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace HaldiramPromotionalApp.Controllers
{
    public class UtilityController : Controller
    {
        private readonly ILogger<UtilityController> _logger;
        private readonly ApplicationDbContext _context;

        public UtilityController(ILogger<UtilityController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Helper method to generate QR codes (can be called from other controllers)
        public static string GenerateQRCodeBase64(string data)
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

        // Test action to seed DealerBasicOrders data
        [Route("Home/SeedDealerBasicOrders")] // Backward compatibility
        public async Task<IActionResult> SeedDealerBasicOrders()
        {
            // Check if user is logged in and is an Admin
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Get a test dealer
                var dealer = await _context.DealerMasters.FirstOrDefaultAsync();
                if (dealer == null)
                {
                    ViewBag.Message = "No dealer found in the database.";
                    return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error seeding DealerBasicOrders: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Test action to seed Materials data
        [Route("Home/SeedMaterials")] // Backward compatibility
        public async Task<IActionResult> SeedMaterials()
        {
            // Check if materials already exist
            var existingMaterials = await _context.MaterialMaster.CountAsync();
            if (existingMaterials > 0)
            {
                ViewBag.Message = $"Database already contains {existingMaterials} materials.";
                return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Index", "Home");
        }
    }
}
