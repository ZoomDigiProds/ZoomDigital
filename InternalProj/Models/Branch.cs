using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        public string BranchName { get; set; } = null!;

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        // Navigation properties
        public ICollection<CustomerReg>? CustomerRegs { get; set; }
        public ICollection<StaffReg>? StaffRegs { get; set; }
        public ICollection<WorkOrderMaster>? WorkOrders { get; set; }
    }
}
