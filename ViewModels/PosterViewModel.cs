using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HaldiramPromotionalApp.ViewModels
{
    public class PosterViewModel
    {
        [Required]
        [Display(Name = "Poster Image")]
        public IFormFile? ImageFile { get; set; }
        
        [Required]
        public string? Message { get; set; }
        
        [Required]
        [Display(Name = "Show From")]
        public DateTime ShowFrom { get; set; }
        
        [Required]
        [Display(Name = "Show Until")]
        public DateTime ShowUntil { get; set; }
    }
}