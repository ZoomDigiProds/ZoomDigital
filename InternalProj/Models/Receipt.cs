//namespace InternalProj.Models
//{
//    public class Receipt
//    {
//    }
//}


using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class Receipt
    {
        [Key]
        public int ReceiptId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        public double NetAmount { get; set; }

        public double CurrentAmount { get; set; }

        public int ModeId { get; set; }

        [ForeignKey("ModeId")]
        [ValidateNever]
        public ModeOfPayment? ModeOfPayment { get; set; }
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        [ValidateNever]
        public CustomerReg? Customer { get; set; }

        public int WorkOrderId { get; set; }

        [ForeignKey("WorkOrderId")]
        [ValidateNever]
        public WorkOrderMaster? WorkOrder { get; set; }
    }
}