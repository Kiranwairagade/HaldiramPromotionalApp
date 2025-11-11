using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaldiramPromotionalApp.Models
{
    public class FreeProductCampaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public string? Materials { get; set; } // Comma-separated list of material IDs

        [Display(Name = "Material Quantities")]
        public string? MaterialQuantities { get; set; } // JSON string of material ID to quantity mapping

        [Display(Name = "Free Products")]
        public string? FreeProducts { get; set; } // JSON string of material ID to free product mapping

        [Display(Name = "Free Quantities")]
        public string? FreeQuantities { get; set; } // JSON string of material ID to free quantity mapping

        [Display(Name = "Campaign Image")]
        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;
    }
}