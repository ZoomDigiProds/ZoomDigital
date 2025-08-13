namespace InternalProj.ViewModel
{
    public class WorkOrderSummaryViewModel
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNo { get; set; }
        public string StudioName { get; set; }
        public double Advance { get; set; }
        public string Size { get; set; }
        public double SubTotal { get; set; }
        public double TotalPaid { get; set; }
        public double Balance { get; set; }
        public string WorkTypeName { get; set; }
        public DateTime? Wdate { get; set; }
    }
}
