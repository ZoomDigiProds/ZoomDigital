using System.ComponentModel.DataAnnotations;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class StaffCredentials   
    {
        [Key]
        public int CredentialId { get; set; }

        public int StaffId { get; set; }

        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Status { get; set; }
        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
        public bool? IsFirstLogin { get; set; } = true;
        public StaffReg? Staff { get; set; }
    }
}
