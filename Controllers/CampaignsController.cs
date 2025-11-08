using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HaldiramPromotionalApp.Controllers
{
    public class CampaignsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CampaignsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CreateCampaign()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult SelectCampaign(string campaignType)
        {
            // Redirect to specific campaign creation page based on type
            switch (campaignType)
            {
                case "POINTS_TO_CASH":
                    return RedirectToAction("CreatePointsToCashCampaign");
                case "POINTS_REWARD":
                    return RedirectToAction("CreatePointsRewardCampaign");
                case "FREE_PRODUCT":
                    return RedirectToAction("CreateFreeProductCampaign");
                case "AMOUNT_REACH_GOAL":
                    return RedirectToAction("CreateAmountReachGoalCampaign");
                case "SESSION_DURATION_REWARD":
                    return RedirectToAction("CreateSessionDurationRewardCampaign");
                default:
                    return RedirectToAction("CreateCampaign");
            }
        }
        
        public async Task<IActionResult> CreatePointsToCashCampaign()
        {
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var viewModel = new PointsToCashCampaignViewModel
            {
                AllMaterials = materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Materialname ?? "",
                    ShortName = m.ShortName ?? "",
                    Category = m.Category ?? "",
                    Price = m.price
                }).ToList()
            };
            
            ViewBag.CampaignType = "Points to Cash";
            return View("CreatePointsToCashCampaign", viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreatePointsToCashCampaign(PointsToCashCampaignViewModel viewModel)
        {
            // Add debugging information
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"SelectedMaterialIds Count: {viewModel.SelectedMaterialIds?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"MaterialPoints Count: {viewModel.MaterialPoints?.Count ?? 0}");
            
            if (viewModel.MaterialPoints != null)
            {
                foreach (var kvp in viewModel.MaterialPoints)
                {
                    System.Diagnostics.Debug.WriteLine($"MaterialPoints[{kvp.Key}]: {kvp.Value}");
                }
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var campaign = new PointsToCashCampaign
                    {
                        CampaignName = viewModel.CampaignName,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Description = viewModel.Description,
                        VoucherGenerationThreshold = viewModel.VoucherGenerationThreshold,
                        VoucherValue = viewModel.VoucherValue,
                        VoucherValidity = viewModel.VoucherValidity,
                        Materials = viewModel.SelectedMaterialIds != null ? string.Join(",", viewModel.SelectedMaterialIds) : "",
                        MaterialPoints = viewModel.MaterialPoints != null ? JsonSerializer.Serialize(viewModel.MaterialPoints) : "{}",
                        IsActive = viewModel.IsActive
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Campaign to be saved: {campaign.CampaignName}");
                    System.Diagnostics.Debug.WriteLine($"Materials: {campaign.Materials}");
                    System.Diagnostics.Debug.WriteLine($"MaterialPoints JSON: {campaign.MaterialPoints}");
                    
                    _context.PointsToCashCampaigns.Add(campaign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Points to Cash Campaign created successfully!";
                    return RedirectToAction("CreateCampaign");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the campaign: " + ex.Message);
                }
            }
            else
            {
                // Add model state errors to TempData for display
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                
                string errorMessages = "";
                foreach (var error in errors)
                {
                    foreach (var subError in error.Errors)
                    {
                        errorMessages += $"{error.Key}: {subError.ErrorMessage}\n";
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors:\n" + errorMessages;
            }
            
            // Repopulate materials if model state is invalid
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            viewModel.AllMaterials = materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Materialname ?? "",
                ShortName = m.ShortName ?? "",
                Category = m.Category ?? "",
                Price = m.price
            }).ToList();
            
            ViewBag.CampaignType = "Points to Cash";
            return View("CreatePointsToCashCampaign", viewModel);
        }
        
        public async Task<IActionResult> CreatePointsRewardCampaign()
        {
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
            var viewModel = new PointsRewardCampaignViewModel
            {
                AllMaterials = materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Materialname ?? "",
                    ShortName = m.ShortName ?? "",
                    Category = m.Category ?? "",
                    Price = m.price
                }).ToList(),
                AllProducts = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    ProductName = p.ProductName ?? "",
                    Price = p.Price,
                    Category = p.Category ?? ""
                }).ToList()
            };
            
            ViewBag.CampaignType = "Points Reward";
            return View("CreatePointsRewardCampaign", viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreatePointsRewardCampaign(PointsRewardCampaignViewModel viewModel)
        {
            // Add debugging information
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"SelectedMaterialIds Count: {viewModel.SelectedMaterialIds?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"MaterialPoints Count: {viewModel.MaterialPoints?.Count ?? 0}");
            
            if (viewModel.MaterialPoints != null)
            {
                foreach (var kvp in viewModel.MaterialPoints)
                {
                    System.Diagnostics.Debug.WriteLine($"MaterialPoints[{kvp.Key}]: {kvp.Value}");
                }
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var campaign = new PointsRewardCampaign
                    {
                        CampaignName = viewModel.CampaignName,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Description = viewModel.Description,
                        VoucherGenerationThreshold = viewModel.VoucherGenerationThreshold,
                        VoucherValidity = viewModel.VoucherValidity,
                        Materials = viewModel.SelectedMaterialIds != null ? string.Join(",", viewModel.SelectedMaterialIds) : "",
                        MaterialPoints = viewModel.MaterialPoints != null ? JsonSerializer.Serialize(viewModel.MaterialPoints) : "{}",
                        RewardProductId = viewModel.RewardProductId,
                        IsActive = viewModel.IsActive
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Campaign to be saved: {campaign.CampaignName}");
                    System.Diagnostics.Debug.WriteLine($"Materials: {campaign.Materials}");
                    System.Diagnostics.Debug.WriteLine($"MaterialPoints JSON: {campaign.MaterialPoints}");
                    
                    _context.PointsRewardCampaigns.Add(campaign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Points Reward Campaign created successfully!";
                    return RedirectToAction("CreateCampaign");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the campaign: " + ex.Message);
                }
            }
            else
            {
                // Add model state errors to TempData for display
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                
                string errorMessages = "";
                foreach (var error in errors)
                {
                    foreach (var subError in error.Errors)
                    {
                        errorMessages += $"{error.Key}: {subError.ErrorMessage}\n";
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors:\n" + errorMessages;
            }
            
            // Repopulate materials and products if model state is invalid
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
            viewModel.AllMaterials = materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Materialname ?? "",
                ShortName = m.ShortName ?? "",
                Category = m.Category ?? "",
                Price = m.price
            }).ToList();
            viewModel.AllProducts = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductName = p.ProductName ?? "",
                Price = p.Price,
                Category = p.Category ?? ""
            }).ToList();
            
            ViewBag.CampaignType = "Points Reward";
            return View("CreatePointsRewardCampaign", viewModel);
        }
        
        public async Task<IActionResult> CreateFreeProductCampaign()
        {
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var viewModel = new FreeProductCampaignViewModel
            {
                AllMaterials = materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Materialname ?? "",
                    ShortName = m.ShortName ?? "",
                    Category = m.Category ?? "",
                    Price = m.price
                }).ToList()
            };
            
            ViewBag.CampaignType = "Free Product";
            return View("CreateFreeProductCampaign", viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateFreeProductCampaign(FreeProductCampaignViewModel viewModel)
        {
            // Add debugging information
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"SelectedMaterialIds Count: {viewModel.SelectedMaterialIds?.Count ?? 0}");
            
            // Process only checked materials
            if (viewModel.SelectedMaterialIds != null && viewModel.SelectedMaterialIds.Any())
            {
                var validMaterialIds = new List<int>();
                
                foreach (var materialId in viewModel.SelectedMaterialIds)
                {
                    // Check if all required data is present for this material
                    bool hasValidQuantity = viewModel.MaterialQuantities?.ContainsKey(materialId) == true && 
                                           viewModel.MaterialQuantities[materialId] > 0;
                    
                    // Allow FreeProducts to be null/empty, but if provided, validate it
                    bool hasValidFreeProduct = true; // Allow null/empty by default
                    if (viewModel.FreeProducts?.ContainsKey(materialId) == true && viewModel.FreeProducts[materialId] > 0)
                    {
                        // If a value is provided, it must be valid
                        hasValidFreeProduct = true;
                    }
                    // If key exists but value is 0 or negative, it's invalid
                    else if (viewModel.FreeProducts?.ContainsKey(materialId) == true && viewModel.FreeProducts[materialId] <= 0)
                    {
                        hasValidFreeProduct = false;
                    }
                    // If key doesn't exist, that's fine (null/empty is allowed)
                    
                    // Allow FreeQuantities to be 0 or null, but if provided, it must be non-negative
                    bool hasValidFreeQuantity = true; // Allow 0 or null by default
                    if (viewModel.FreeQuantities?.ContainsKey(materialId) == true && viewModel.FreeQuantities[materialId] >= 0)
                    {
                        // If a value is provided, it must be valid (non-negative)
                        hasValidFreeQuantity = true;
                    }
                    // If key exists but value is negative, it's invalid
                    else if (viewModel.FreeQuantities?.ContainsKey(materialId) == true && viewModel.FreeQuantities[materialId] < 0)
                    {
                        hasValidFreeQuantity = false;
                    }
                    // If key doesn't exist, that's fine (null/empty is allowed)
                    
                    if (hasValidQuantity && hasValidFreeProduct && hasValidFreeQuantity)
                    {
                        validMaterialIds.Add(materialId);
                    }
                    else
                    {
                        // Add specific error messages for missing data
                        if (!hasValidQuantity)
                        {
                            ModelState.AddModelError("", $"Please enter a valid quantity for material {materialId}.");
                        }
                        if (!hasValidFreeProduct)
                        {
                            ModelState.AddModelError("", $"Please select a valid free product for material {materialId}.");
                        }
                        if (!hasValidFreeQuantity)
                        {
                            ModelState.AddModelError("", $"Free quantity for material {materialId} cannot be negative.");
                        }
                    }
                }
                
                // Update the selected material IDs to only include valid ones
                viewModel.SelectedMaterialIds = validMaterialIds;
            }
            else
            {
                ModelState.AddModelError("", "Please select at least one material.");
            }
            
            if (viewModel.SelectedMaterialIds.Any())
            {
                try
                {
                    // Create filtered dictionaries containing only data for selected materials
                    var filteredMaterialQuantities = new Dictionary<int, int>();
                    var filteredFreeProducts = new Dictionary<int, int>();
                    var filteredFreeQuantities = new Dictionary<int, int>();
                    
                    foreach (var materialId in viewModel.SelectedMaterialIds)
                    {
                        if (viewModel.MaterialQuantities?.ContainsKey(materialId) == true)
                        {
                            filteredMaterialQuantities[materialId] = viewModel.MaterialQuantities[materialId];
                        }
                        
                        if (viewModel.FreeProducts?.ContainsKey(materialId) == true && viewModel.FreeProducts[materialId] > 0)
                        {
                            filteredFreeProducts[materialId] = viewModel.FreeProducts[materialId];
                        }
                        
                        // Store FreeQuantities even if 0, but only if the key exists
                        if (viewModel.FreeQuantities?.ContainsKey(materialId) == true)
                        {
                            filteredFreeQuantities[materialId] = viewModel.FreeQuantities[materialId];
                        }
                    }
                    
                    var campaign = new FreeProductCampaign
                    {
                        CampaignName = viewModel.CampaignName,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Description = viewModel.Description,
                        Materials = viewModel.SelectedMaterialIds != null ? string.Join(",", viewModel.SelectedMaterialIds) : "",
                        MaterialQuantities = filteredMaterialQuantities.Any() ? JsonSerializer.Serialize(filteredMaterialQuantities) : "{}",
                        FreeProducts = filteredFreeProducts.Any() ? JsonSerializer.Serialize(filteredFreeProducts) : "{}",
                        FreeQuantities = filteredFreeQuantities.Any() ? JsonSerializer.Serialize(filteredFreeQuantities) : "{}",
                        IsActive = viewModel.IsActive
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Campaign to be saved: {campaign.CampaignName}");
                    System.Diagnostics.Debug.WriteLine($"Materials: {campaign.Materials}");
                    
                    _context.FreeProductCampaigns.Add(campaign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Free Product Campaign created successfully!";
                    return RedirectToAction("CreateCampaign");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the campaign: " + ex.Message);
                }
            }
            else if (ModelState.IsValid && !viewModel.SelectedMaterialIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one material with valid data.");
            }
            
            // Add model state errors to TempData for display
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                
                string errorMessages = "";
                foreach (var error in errors)
                {
                    foreach (var subError in error.Errors)
                    {
                        errorMessages += $"{error.Key}: {subError.ErrorMessage}\n";
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors:\n" + errorMessages;
            }
            
            // Repopulate materials if model state is invalid
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            viewModel.AllMaterials = materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Materialname ?? "",
                ShortName = m.ShortName ?? "",
                Category = m.Category ?? "",
                Price = m.price
            }).ToList();
            
            ViewBag.CampaignType = "Free Product";
            return View("CreateFreeProductCampaign", viewModel);
        }
        
        public async Task<IActionResult> CreateAmountReachGoalCampaign()
        {
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var viewModel = new AmountReachGoalCampaignViewModel
            {
                AllMaterials = materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Materialname ?? "",
                    ShortName = m.ShortName ?? "",
                    Category = m.Category ?? "",
                    Price = m.price
                }).ToList()
            };
            
            ViewBag.CampaignType = "Amount Reach Goal";
            return View("CreateAmountReachGoalCampaign", viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateAmountReachGoalCampaign(AmountReachGoalCampaignViewModel viewModel)
        {
            // Add debugging information
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var campaign = new AmountReachGoalCampaign
                    {
                        CampaignName = viewModel.CampaignName,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Description = viewModel.Description,
                        TargetAmount = viewModel.TargetAmount,
                        VoucherValue = viewModel.VoucherValue,
                        VoucherValidity = viewModel.VoucherValidity,
                        IsActive = viewModel.IsActive
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Campaign to be saved: {campaign.CampaignName}");
                    
                    _context.AmountReachGoalCampaigns.Add(campaign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Amount Reach Goal Campaign created successfully!";
                    return RedirectToAction("CreateCampaign");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the campaign: " + ex.Message);
                }
            }
            else
            {
                // Add model state errors to TempData for display
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                
                string errorMessages = "";
                foreach (var error in errors)
                {
                    foreach (var subError in error.Errors)
                    {
                        errorMessages += $"{error.Key}: {subError.ErrorMessage}\n";
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors:\n" + errorMessages;
            }
            
            // Repopulate materials if model state is invalid
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            viewModel.AllMaterials = materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Materialname ?? "",
                ShortName = m.ShortName ?? "",
                Category = m.Category ?? "",
                Price = m.price
            }).ToList();
            
            ViewBag.CampaignType = "Amount Reach Goal";
            return View("CreateAmountReachGoalCampaign", viewModel);
        }
        
        public async Task<IActionResult> CreateSessionDurationRewardCampaign()
        {
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            var viewModel = new SessionDurationRewardCampaignViewModel
            {
                AllMaterials = materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Materialname ?? "",
                    ShortName = m.ShortName ?? "",
                    Category = m.Category ?? "",
                    Price = m.price
                }).ToList()
            };
            
            ViewBag.CampaignType = "Session Duration Reward";
            return View("CreateSessionDurationRewardCampaign", viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSessionDurationRewardCampaign(SessionDurationRewardCampaignViewModel viewModel)
        {
            // Add debugging information
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var campaign = new SessionDurationRewardCampaign
                    {
                        CampaignName = viewModel.CampaignName,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Description = viewModel.Description,
                        SessionDuration = viewModel.SessionDuration,
                        VoucherValue = viewModel.VoucherValue,
                        VoucherValidity = viewModel.VoucherValidity,
                        IsActive = viewModel.IsActive
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"Campaign to be saved: {campaign.CampaignName}");
                    
                    _context.SessionDurationRewardCampaigns.Add(campaign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Session Duration Reward Campaign created successfully!";
                    return RedirectToAction("CreateCampaign");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while saving the campaign: " + ex.Message);
                }
            }
            else
            {
                // Add model state errors to TempData for display
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                
                string errorMessages = "";
                foreach (var error in errors)
                {
                    foreach (var subError in error.Errors)
                    {
                        errorMessages += $"{error.Key}: {subError.ErrorMessage}\n";
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors:\n" + errorMessages;
            }
            
            // Repopulate materials if model state is invalid
            var materials = await _context.MaterialMaster.Where(m => m.isactive).ToListAsync();
            viewModel.AllMaterials = materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Materialname ?? "",
                ShortName = m.ShortName ?? "",
                Category = m.Category ?? "",
                Price = m.price
            }).ToList();
            
            ViewBag.CampaignType = "Session Duration Reward";
            return View("CreateSessionDurationRewardCampaign", viewModel);
        }
        
        // GET: Campaigns/ViewCampaigns
        public async Task<IActionResult> ViewCampaigns()
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
                    Description = campaign.Description,
                    Materials = campaign.Materials,
                    MaterialPoints = campaign.MaterialPoints,
                    VoucherGenerationThreshold = campaign.VoucherGenerationThreshold,
                    VoucherValue = campaign.VoucherValue,
                    VoucherValidity = campaign.VoucherValidity,
                    IsActive = campaign.IsActive
                };

                // Parse material details
                if (!string.IsNullOrEmpty(campaign.Materials))
                {
                    var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                    var materialPoints = new Dictionary<int, int>();

                    if (!string.IsNullOrEmpty(campaign.MaterialPoints))
                    {
                        try
                        {
                            materialPoints = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialPoints) ?? new Dictionary<int, int>();
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

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
                    Description = campaign.Description,
                    Materials = campaign.Materials,
                    MaterialPoints = campaign.MaterialPoints,
                    VoucherGenerationThreshold = campaign.VoucherGenerationThreshold,
                    VoucherValidity = campaign.VoucherValidity,
                    RewardProductId = campaign.RewardProductId,
                    RewardProduct = campaign.RewardProduct,
                    IsActive = campaign.IsActive
                };

                // Parse material details
                if (!string.IsNullOrEmpty(campaign.Materials))
                {
                    var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                    var materialPoints = new Dictionary<int, int>();

                    if (!string.IsNullOrEmpty(campaign.MaterialPoints))
                    {
                        try
                        {
                            materialPoints = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialPoints) ?? new Dictionary<int, int>();
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

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

                detailedPointsRewardCampaigns.Add(detailedCampaign);
            }

            // Process FreeProductCampaigns to include material details
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
                    Materials = campaign.Materials,
                    MaterialQuantities = campaign.MaterialQuantities,
                    FreeProducts = campaign.FreeProducts,
                    FreeQuantities = campaign.FreeQuantities,
                    IsActive = campaign.IsActive
                };

                // Parse material details
                if (!string.IsNullOrEmpty(campaign.Materials))
                {
                    var materialIds = campaign.Materials.Split(',').Select(int.Parse).ToList();
                    var materialQuantities = new Dictionary<int, int>();
                    
                    if (!string.IsNullOrEmpty(campaign.MaterialQuantities))
                    {
                        try
                        {
                            materialQuantities = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.MaterialQuantities) ?? new Dictionary<int, int>();
                        }
                        catch
                        {
                            // Handle deserialization error
                        }
                    }

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
                }

                // Parse free product details
                if (!string.IsNullOrEmpty(campaign.FreeProducts))
                {
                    try
                    {
                        var freeProducts = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.FreeProducts) ?? new Dictionary<int, int>();
                        var freeQuantities = new Dictionary<int, int>();
                        
                        if (!string.IsNullOrEmpty(campaign.FreeQuantities))
                        {
                            freeQuantities = JsonSerializer.Deserialize<Dictionary<int, int>>(campaign.FreeQuantities) ?? new Dictionary<int, int>();
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
                AmountReachGoalCampaigns = amountReachGoalCampaigns,
                SessionDurationRewardCampaigns = sessionDurationRewardCampaigns
            };

            return View(viewModel);
        }
    }
}