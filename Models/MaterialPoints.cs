using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaldiramPromotionalApp.Models
{
    public class MaterialPoints
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Material ID")]
        public int MaterialId { get; set; }

        [Required]
        [Display(Name = "Points")]
        public int Points { get; set; } = 0;

        // Navigation property
        [ForeignKey("MaterialId")]
        public virtual MaterialMaster Material { get; set; }
    }
}