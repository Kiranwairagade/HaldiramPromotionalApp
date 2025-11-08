using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class MaterialImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MaterialImagesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Upload()
        {
            var viewModel = new MaterialImageViewModel();
            
            // Get all active materials for the dropdown
            var materials = await _context.MaterialMaster
                .Where(m => m.isactive)
                .Select(m => new
                {
                    m.Id,
                    DisplayName = $"{m.Materialname} ({m.ShortName})"
                })
                .ToListAsync();
            
            ViewBag.Materials = new SelectList(materials, "Id", "DisplayName");
            
            // Get all existing material images to display
            var materialImages = await _context.MaterialImages
                .Include(mi => mi.MaterialMaster)
                .ToListAsync();
            
            ViewBag.MaterialImages = materialImages;
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(MaterialImageViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(viewModel.ImageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                    }
                    else
                    {
                        // Create uploads directory if it doesn't exist
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        // Generate unique filename
                        var fileName = $"material_{viewModel.MaterialMasterId}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        
                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ImageFile.CopyToAsync(stream);
                        }
                        
                        // Save to database
                        var materialImage = new MaterialImage
                        {
                            MaterialMasterId = viewModel.MaterialMasterId,
                            ImagePath = $"/uploads/materials/{fileName}"
                        };
                        
                        _context.MaterialImages.Add(materialImage);
                        await _context.SaveChangesAsync();
                        
                        TempData["SuccessMessage"] = "Material image uploaded successfully!";
                        return RedirectToAction("Upload");
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            
            // Repopulate dropdown if model state is invalid
            var materials = await _context.MaterialMaster
                .Where(m => m.isactive)
                .Select(m => new
                {
                    m.Id,
                    DisplayName = $"{m.Materialname} ({m.ShortName})"
                })
                .ToListAsync();
            
            ViewBag.Materials = new SelectList(materials, "Id", "DisplayName");
            
            // Get all existing material images to display
            var materialImages = await _context.MaterialImages
                .Include(mi => mi.MaterialMaster)
                .ToListAsync();
            
            ViewBag.MaterialImages = materialImages;
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var materialImage = await _context.MaterialImages.FindAsync(id);
            if (materialImage == null)
            {
                TempData["ErrorMessage"] = "Material image not found.";
                return RedirectToAction("Upload");
            }
            
            try
            {
                // Delete the file from the file system
                var fullPath = Path.Combine(_environment.WebRootPath, materialImage.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                
                // Remove from database
                _context.MaterialImages.Remove(materialImage);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Material image deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the material image: " + ex.Message;
            }
            
            return RedirectToAction("Upload");
        }
    }
}