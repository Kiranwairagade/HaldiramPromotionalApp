using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaldiramPromotionalApp.Models
{
    public class DealerBasicOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DealerId { get; set; }

        [Required]
        [Display(Name = "Material Name")]
        [StringLength(500)]
        public string MaterialName { get; set; }

        [Required]
        [Display(Name = "SAP Code")]
        [StringLength(500)]
        public string SapCode { get; set; }

        [Required]
        [Display(Name = "Short Code")]
        [StringLength(500)]
        public string ShortCode { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Rate")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }
    }
}