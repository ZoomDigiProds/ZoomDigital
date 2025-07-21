//namespace InternalProj.ViewModel
//{
//    public class ReceiptViewModel
//    {
//    }
//}


using InternalProj.Models;

namespace InternalProj.ViewModel
{
    public class ReceiptViewModel
    {
        public Receipt Receipt { get; set; }
        public List<ModeOfPayment> ModeOfPayments { get; set; } = new List<ModeOfPayment>();
        public List<CustomerReg> Customers { get; set; } = new List<CustomerReg>();
        public List<WorkOrderMaster> CustomerWorkOrders { get; set; } = new List<WorkOrderMaster>();
        public int ReceiptNo { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public List<WorkOrderSummaryDto> PagedWorkOrders { get; set; } = new();
        public double TotalBalance { get; set; }

    }

    public class WorkOrderSummaryDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNo { get; set; }
        public DateTime? Wdate { get; set; }
        public double SubTotal { get; set; }
        public double Advance { get; set; }
        public double TotalPaid { get; set; }
        public double Balance { get; set; }
    }
}