using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class CustomerCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; } = null!;

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; } = "Y";

        public ICollection<CustomerReg>? Customers { get; set; }
    }
}
