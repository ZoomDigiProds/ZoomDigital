using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        public decimal? Discount { get; set; } = 0.00m;
        public decimal? Tax { get; set; } = 0.00m;
        public decimal? Cess { get; set; } = 0.00m;
        public decimal? Commission { get; set; } = 0.00m;
        public decimal? NetAmount { get; set; } = 0.00m;

        public int CustomerId { get; set; }
        public int WorkOrderId { get; set; }
        public int ModeId { get; set; }


        [ValidateNever]
        public CustomerReg Customer { get; set; }
        
        [ValidateNever]
        public WorkOrderMaster WorkOrder { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ModeId))]
        public ModeOfPayment ModeOfPayment { get; set; }
    }

}
