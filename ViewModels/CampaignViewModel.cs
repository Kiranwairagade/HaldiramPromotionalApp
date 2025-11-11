using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HaldiramPromotionalApp.ViewModels
{
    public class CampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Campaign Type")]
        public string CampaignType { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Materials")]
        public List<int> SelectedMaterialIds { get; set; } = new List<int>();
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class MaterialViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
    
    public class PointsToCashCampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Display(Name = "Voucher Generation Threshold (Points)")]
        public int VoucherGenerationThreshold { get; set; }
        
        [Required]
        [Display(Name = "Voucher Value (₹)")]
        public decimal VoucherValue { get; set; }
        
        [Required]
        [Display(Name = "Voucher Validity (Days)")]
        public int VoucherValidity { get; set; }
        
        [Display(Name = "Materials")]
        public List<int> SelectedMaterialIds { get; set; } = new List<int>();
        
        // New property to store points for each material
        public Dictionary<int, int> MaterialPoints { get; set; } = new Dictionary<int, int>();
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class PointsRewardCampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Display(Name = "Default Voucher Generation Threshold (Points)")]
        public int VoucherGenerationThreshold { get; set; }
        
        [Required]
        [Display(Name = "Voucher Validity (Days)")]
        public int VoucherValidity { get; set; }
        
        [Display(Name = "Materials")]
        public List<int> SelectedMaterialIds { get; set; } = new List<int>();
        
        // New property to store points for each material
        public Dictionary<int, int> MaterialPoints { get; set; } = new Dictionary<int, int>();
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        // New properties for product selection
        [Display(Name = "Reward Product")]
        public int? RewardProductId { get; set; }
        
        [Display(Name = "All Products")]
        public List<ProductViewModel> AllProducts { get; set; } = new List<ProductViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
    }
    
    public class FreeProductCampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Materials")]
        public List<int> SelectedMaterialIds { get; set; } = new List<int>();
        
        // New properties to store quantities, free products, and free quantities for each material
        public Dictionary<int, int>? MaterialQuantities { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int>? FreeProducts { get; set; } = new Dictionary<int, int>(); // Changed to int for material ID
        public Dictionary<int, int>? FreeQuantities { get; set; } = new Dictionary<int, int>();
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        // Add the missing AllProducts property
        [Display(Name = "All Products")]
        public List<ProductViewModel> AllProducts { get; set; } = new List<ProductViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedMaterialIds != null && SelectedMaterialIds.Any())
            {
                foreach (var materialId in SelectedMaterialIds)
                {
                    // Validate MaterialQuantities
                    if (!MaterialQuantities.ContainsKey(materialId) || MaterialQuantities[materialId] <= 0)
                    {
                        yield return new ValidationResult($"Please enter a valid quantity for material {materialId}.", new[] { "MaterialQuantities" });
                    }
                    
                    // Allow FreeProducts to be null/empty, but if provided, validate it
                    // Only validate if the key exists and has a value <= 0
                    if (FreeProducts.ContainsKey(materialId) && FreeProducts[materialId] <= 0)
                    {
                        yield return new ValidationResult($"Please select a valid free product for material {materialId}.", new[] { "FreeProducts" });
                    }
                    
                    // Remove validation for FreeQuantities - allow 0 or null values
                    // Only validate if the value is explicitly provided and is negative
                    if (FreeQuantities.ContainsKey(materialId) && FreeQuantities[materialId] < 0)
                    {
                        yield return new ValidationResult($"Free quantity for material {materialId} cannot be negative.", new[] { "FreeQuantities" });
                    }
                }
            }
            else
            {
                yield return new ValidationResult("Please select at least one material.", new[] { "SelectedMaterialIds" });
            }
        }
    }
    
    public class AmountReachGoalCampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }
        
        [Required]
        [Display(Name = "Voucher Value (₹)")]
        public decimal VoucherValue { get; set; }
        
        [Required]
        [Display(Name = "Voucher Validity (Days)")]
        public int VoucherValidity { get; set; }
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class SessionDurationRewardCampaignViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Required]
        [Display(Name = "Session Duration")]
        public int SessionDuration { get; set; }
        
        [Required]
        [Display(Name = "Voucher Value (₹)")]
        public decimal VoucherValue { get; set; }
        
        [Required]
        [Display(Name = "Voucher Validity (Days)")]
        public int VoucherValidity { get; set; }
        
        [Display(Name = "All Materials")]
        public List<MaterialViewModel> AllMaterials { get; set; } = new List<MaterialViewModel>();
        
        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}