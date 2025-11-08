using System.Diagnostics;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
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
            // Check if there are any materials in the database
            var materialCount = await _context.MaterialMaster.CountAsync();
            ViewBag.MaterialCount = materialCount;
            
            // Get first 5 materials for testing
            var materials = await _context.MaterialMaster.Take(5).ToListAsync();
            ViewBag.TestMaterials = materials;
            
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
    }
}