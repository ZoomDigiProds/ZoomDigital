using InternalProj.Models;

namespace InternalProj.ViewModel
{
    public class InvoiceViewModel
    {
        public Invoice Invoice { get; set; } = new Invoice();

        public List<WorkDetailDTO> WorkDetails { get; set; } = new();
        public List<WorkDetails> AllWorkDetails { get; set; } = new();
        public IEnumerable<CustomerReg> Customers { get; set; } = new List<CustomerReg>();
        public IEnumerable<WorkOrderMaster> WorkOrders { get; set; } = new List<WorkOrderMaster>();
        public IEnumerable<ModeOfPayment> PaymentModes { get; set; } = new List<ModeOfPayment>(); 

        public double OutstandingAmount { get; set; }
        public double SubAmount { get; set; }
        public double Tax { get; set; }
        public double Cess { get; set; }
        public double Commission { get; set; }
        public double NetAmount { get; set; }

    }


    public class WorkDetailDTO
    {
        public string Particulars { get; set; }
        public int Qty { get; set; }
        public double Rate { get; set; }
        public double GTotal { get; set; }
    }
}
