using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InternalProj.Models
{
    public class WorkDetails
    {
        [Key]
        public int WorkDetailsId { get; set; }

        [ForeignKey("WorkOrder")]
        public int WorkOrderId { get; set; }

        [ForeignKey("MainHead")]
        public int MainHeadId { get; set; }

        [ForeignKey("SubHead")]
        public int SubheadId { get; set; }

        [ForeignKey("ChildSubhead")]
        public int? ChildSubheadId { get; set; }

        public int Qty { get; set; }

        public double Rate { get; set; }

        public double? Tax { get; set; }

        public double GTotal { get; set; }

        public double? Cess { get; set; }

        public string Details { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        [ForeignKey("Size")]
        public int SizeId { get; set; }

        [ValidateNever]
        public AlbumSizeDetails Size { get; set; }

        // Navigation properties
        [ValidateNever]
        public WorkOrderMaster WorkOrder { get; set; }
        [ValidateNever]
        public MainHeadReg MainHead { get; set; }
        [ValidateNever]
        public SubHeadDetails SubHead { get; set; }
        [ValidateNever]
        public ChildSubHead ChildSubHead { get; set; }
    }
}
