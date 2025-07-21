using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class StaffContact
    {
        [Key]
        public int ContactId { get; set; }
        [Required]
        public int StaffId { get; set; }

        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Whatsapp { get; set; }
        public string? Email { get; set; }

        [Required]
        public int PhoneTypeId { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }


        [ForeignKey("StaffId")]
        public StaffReg? Staff { get; set; }

        [ForeignKey("PhoneTypeId")]
        public PhoneType? PhoneType { get; set; }
    }
}
