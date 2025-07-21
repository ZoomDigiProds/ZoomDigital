using System.ComponentModel.DataAnnotations;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class StaffAddress
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N", ErrorMessage = "Active must be 'Y' or 'N'.")]
        public string? Active { get; set; }

        public StaffReg? Staff { get; set; }

    }
}
