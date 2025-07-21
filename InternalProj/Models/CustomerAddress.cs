using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class CustomerAddress
    {
        [Key]
        public int AddressId { get; set; }
        
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        public int StateId { get; set; }
        public int RegionId { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }


        [Required]
        public int CustomerId { get; set; }

        // Navigation properties
        public CustomerReg Customer { get; set; }
        public StateMaster? State { get; set; }
        public RegionMaster? Region { get; set; }
    }
}
