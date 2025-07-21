using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class CustomerContact
    {
        [Key]
        public int ContactId { get; set; }

        // Foreign key reference to CustomerReg
        [Required]
        public int CustomerId { get; set; }

        public string Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Whatsapp { get; set; }
        public string Email { get; set; }

        // Foreign key reference to PhoneType
        [Required]
        public int PhoneTypeId { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public CustomerReg Customer { get; set; }

        [ForeignKey("PhoneTypeId")]
        public PhoneType PhoneType { get; set; }
    }
}
