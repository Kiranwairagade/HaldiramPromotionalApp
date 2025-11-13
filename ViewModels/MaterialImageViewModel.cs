using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HaldiramPromotionalApp.ViewModels
{
    public class MaterialImageViewModel
    {
        [Display(Name = "Materials")]
        [MinLength(1, ErrorMessage = "Please select at least one material.")]
        public List<int> MaterialMasterIds { get; set; } = new List<int>();
        
        [Required]
        [Display(Name = "Material Image")]
        public IFormFile? ImageFile { get; set; }
        
        public string? MaterialName { get; set; }
        public string? MaterialShortName { get; set; }
    }
}