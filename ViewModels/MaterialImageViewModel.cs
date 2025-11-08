using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HaldiramPromotionalApp.ViewModels
{
    public class MaterialImageViewModel
    {
        [Required]
        [Display(Name = "Material")]
        public int MaterialMasterId { get; set; }
        
        [Required]
        [Display(Name = "Material Image")]
        public IFormFile? ImageFile { get; set; }
        
        public string? MaterialName { get; set; }
        public string? MaterialShortName { get; set; }
    }
}