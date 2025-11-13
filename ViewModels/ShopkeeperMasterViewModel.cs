using System.ComponentModel.DataAnnotations;

namespace HaldiramPromotionalApp.ViewModels
{
    public class ShopkeeperMasterViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Shopkeeper Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Store Location")]
        public string StoreLocation { get; set; }

        [Required]
        [Display(Name = "Store Type")]
        public string StoreType { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [MaxLength(10), MinLength(10)]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        
        public List<string> StoreTypes { get; set; } = new List<string> { "Haldiram", "Other" };
    }
}