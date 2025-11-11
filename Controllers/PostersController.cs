using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class PostersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Upload()
        {
            var viewModel = new PosterViewModel
            {
                ShowFrom = DateTime.Now.Date,
                ShowUntil = DateTime.Now.Date.AddDays(7)
            };
            
            // Get all posters to display in the view
            var posters = await _context.Posters.ToListAsync();
            ViewBag.Posters = posters;
            
            return View("~/Views/Manufacturer/UploadPoster.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(PosterViewModel viewModel)
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
                        
                        // Get all posters to display in the view
                        var posters = await _context.Posters.ToListAsync();
                        ViewBag.Posters = posters;
                        
                        return View("~/Views/Manufacturer/UploadPoster.cshtml", viewModel);
                    }
                    
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    // Generate unique filename
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(stream);
                    }
                    
                    // Save to database
                    var poster = new Poster
                    {
                        ImagePath = "/uploads/" + fileName,
                        Message = viewModel.Message,
                        ShowFrom = viewModel.ShowFrom,
                        ShowUntil = viewModel.ShowUntil
                    };
                    
                    _context.Posters.Add(poster);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Poster uploaded successfully!";
                    return RedirectToAction("Upload");
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            
            // Get all posters to display in the view
            var existingPosters = await _context.Posters.ToListAsync();
            ViewBag.Posters = existingPosters;
            
            return View("~/Views/Manufacturer/UploadPoster.cshtml", viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var poster = await _context.Posters.FindAsync(id);
            if (poster == null)
            {
                TempData["ErrorMessage"] = "Poster not found.";
                return RedirectToAction("Upload");
            }
            
            try
            {
                // Delete the file from the file system
                var fullPath = Path.Combine(_environment.WebRootPath, poster.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                
                // Remove from database
                _context.Posters.Remove(poster);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Poster deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the poster: " + ex.Message;
            }
            
            return RedirectToAction("Upload");
        }
    }
}