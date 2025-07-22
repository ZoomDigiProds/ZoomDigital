namespace InternalProj.ViewModel
{
    public class DailyTransactionViewModel
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNo { get; set; }
        public string StudioName { get; set; }
        public decimal BillAmount { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastTransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
