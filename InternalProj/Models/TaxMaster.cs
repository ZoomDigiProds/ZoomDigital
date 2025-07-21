using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class TaxMaster
    {
        [Key]
        public int TaxId { get; set; }

        [Required]
        [StringLength(255)]
        public string TaxName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tax Percentage must be a positive value.")]
        public decimal TaxPer { get; set; }

        [Required]
        public DateTime WefDate { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
