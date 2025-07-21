using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class PhoneType
    {
        [Key]
        public int PhoneTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string PhoneTypeName { get; set; } = null!;

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; } = "Y";
    }
}
