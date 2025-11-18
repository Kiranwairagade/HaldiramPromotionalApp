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
            
            // Get all active materials
            var allActiveMaterials = await _context.MaterialMaster
                .Where(m => m.isactive)
                .OrderBy(m => m.Materialname)
                .Select(m => new
                {
                    m.Id,
                    DisplayName = $"{m.Materialname} ({m.ShortName})"
                })
                .ToListAsync();
            
            // Log the count of all active materials for debugging
            System.Diagnostics.Debug.WriteLine($"Total active materials: {allActiveMaterials.Count}");
            
            // Get IDs of materials that currently have images
            var materialsWithImages = await _context.MaterialImages
                .Select(mi => mi.MaterialMasterId)
                .Distinct()
                .ToListAsync();
            
            // Log the count of materials with images for debugging
            System.Diagnostics.Debug.WriteLine($"Materials with images: {materialsWithImages.Count}");
            
            // Filter to only show materials that don't have images
            var materials = allActiveMaterials
                .Where(m => !materialsWithImages.Contains(m.Id))
                .ToList();
            
            // Log the count of materials without images for debugging
            System.Diagnostics.Debug.WriteLine($"Materials without images: {materials.Count}");
            
            // Create SelectList with proper value and text fields
            var selectList = new List<SelectListItem>();
            foreach (var material in materials)
            {
                selectList.Add(new SelectListItem
                {
                    Value = material.Id.ToString(),
                    Text = material.DisplayName
                });
            }
            
            ViewBag.Materials = new SelectList(selectList, "Value", "Text");
            
            // Get all existing material images to display
            var materialImages = await _context.MaterialImages
                .Include(mi => mi.MaterialMaster)
                .OrderByDescending(mi => mi.CreatedAt) // Show newest images first
                .ToListAsync();
            
            ViewBag.MaterialImages = materialImages;
            
            return View("~/Views/Manufacturer/UploadMaterialImages.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(MaterialImageViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if at least one material is selected
                if (viewModel.MaterialMasterIds == null || !viewModel.MaterialMasterIds.Any())
                {
                    ModelState.AddModelError("MaterialMasterIds", "Please select at least one material.");
                }
                else if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
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
                        try
                        {
                            // Create uploads directory if it doesn't exist
                            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "", "uploads", "materials");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            // Generate unique filename
                            var fileName = $"material_{string.Join("_", viewModel.MaterialMasterIds)}_{Guid.NewGuid()}{fileExtension}";
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            // Save file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await viewModel.ImageFile.CopyToAsync(stream);
                            }

                            // Save to database for each selected material
                            foreach (var materialId in viewModel.MaterialMasterIds)
                            {
                                var materialImage = new MaterialImage
                                {
                                    MaterialMasterId = materialId,
                                    ImagePath = $"/uploads/materials/{fileName}"
                                };

                                _context.MaterialImages.Add(materialImage);
                            }

                            await _context.SaveChangesAsync();

                            // Get the names of the materials for the success message
                            var materialNames = await _context.MaterialMaster
                                .Where(m => viewModel.MaterialMasterIds.Contains(m.Id))
                                .Select(m => m.Materialname)
                                .ToListAsync();

                            TempData["SuccessMessage"] = $"{viewModel.MaterialMasterIds.Count} material image(s) uploaded successfully for: {string.Join(", ", materialNames)}";
                            return RedirectToAction("Upload");
                        }
                        catch (Exception ex)
                        {
                            // Capture exception details for debugging and show friendly message
                            System.Diagnostics.Debug.WriteLine($"Error saving material image: {ex}");
                            TempData["ErrorMessage"] = "An error occurred while saving the image. " + ex.Message;
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                }
            }
            
            // If we reach here ModelState is invalid or an error occurred. Log model state errors for debugging.
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Any())
                {
                    var msg = string.Join("; ", errors);
                    System.Diagnostics.Debug.WriteLine($"ModelState errors: {msg}");
                    // Only set a TempData error if one isn't already set from an exception
                    if (string.IsNullOrEmpty(TempData["ErrorMessage"] as string))
                    {
                        TempData["ErrorMessage"] = msg;
                    }
                }
            }
            
            // Repopulate dropdown if model state is invalid
            // Get all active materials
            var allActiveMaterials = await _context.MaterialMaster
                .Where(m => m.isactive)
                .OrderBy(m => m.Materialname)
                .Select(m => new
                {
                    m.Id,
                    DisplayName = $"{m.Materialname} ({m.ShortName})"
                })
                .ToListAsync();
            
            // Log the count of all active materials for debugging
            System.Diagnostics.Debug.WriteLine($"Total active materials: {allActiveMaterials.Count}");
            
            // Get IDs of materials that currently have images
            var materialsWithImages = await _context.MaterialImages
                .Select(mi => mi.MaterialMasterId)
                .Distinct()
                .ToListAsync();
            
            // Log the count of materials with images for debugging
            System.Diagnostics.Debug.WriteLine($"Materials with images: {materialsWithImages.Count}");
            
            // Filter to only show materials that don't have images
            var materials = allActiveMaterials
                .Where(m => !materialsWithImages.Contains(m.Id))
                .ToList();
            
            // Log the count of materials without images for debugging
            System.Diagnostics.Debug.WriteLine($"Materials without images: {materials.Count}");
            
            // Create SelectList with proper value and text fields
            var selectList = new List<SelectListItem>();
            foreach (var material in materials)
            {
                selectList.Add(new SelectListItem
                {
                    Value = material.Id.ToString(),
                    Text = material.DisplayName
                });
            }
            
            ViewBag.Materials = new SelectList(selectList, "Value", "Text");
            
            // Get all existing material images to display
            var materialImages = await _context.MaterialImages
                .Include(mi => mi.MaterialMaster)
                .OrderByDescending(mi => mi.CreatedAt) // Show newest images first
                .ToListAsync();
            
            ViewBag.MaterialImages = materialImages;
            
            return View("~/Views/Manufacturer/UploadMaterialImages.cshtml", viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var materialImage = await _context.MaterialImages
                .Include(mi => mi.MaterialMaster)
                .FirstOrDefaultAsync(mi => mi.Id == id);
                
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
                
                TempData["SuccessMessage"] = $"Material image for '{materialImage.MaterialMaster?.Materialname}' deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the material image: " + ex.Message;
            }
            
            return RedirectToAction("Upload");
        }
    }
}