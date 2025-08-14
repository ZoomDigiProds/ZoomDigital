using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class WorkOrderMaster
    {
        [Key]
        public int WorkOrderId { get; set; }
        public string? WorkOrderNo { get; set; }
        //public string Studio { get; set; }
        public string? Name { get; set; }
        public int? NoOfSheets { get; set; }
        public int? NoOfCopies { get; set; }
        public DateTime? Wdate { get; set; }
        public DateTime? Ddate { get; set; }
        public string? Mobile { get; set; }
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public double SubTotal { get; set; }
        public double? Advance { get; set; }
        public double? Balance { get; set; }

        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }

        public int? DeliveryTypeId { get; set; }
        public DeliveryMaster? DeliveryType { get; set; }

        public int? DeliveryModeId { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }

        public int? AlbumSizeId { get; set; }
        public AlbumSizeDetails? AlbumSize { get; set; }

        public int? CustomerId { get; set; }
        public CustomerReg? Customer { get; set; }

        public int? WorkTypeId { get; set; }
        public WorkType? WorkType { get; set; }

        public int? StaffId { get; set; }     
        public StaffReg? Staff { get; set; }

        public int? OrderViaId { get; set; }
        public OrderVia? OrderVia { get; set; }

        public int? BranchId { get; set; }
        [ValidateNever]
        public Branch Branch { get; set; }


        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
        public ICollection<WorkDetails> WorkDetails { get; set; } = new List<WorkDetails>();

        public ICollection<Job>? Jobs { get; set; }
    }
}
