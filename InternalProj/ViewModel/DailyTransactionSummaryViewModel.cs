namespace InternalProj.ViewModel
{
    public class DailyTransactionSummaryViewModel
    {
        public string StudioName { get; set; }
        public double SubTotal { get; set; }
        public double Advance { get; set; }
        public double TotalPaid { get; set; }
        public double Balance => Math.Max(0, SubTotal - Advance - TotalPaid);
        public DateTime Date { get; set; }
    }
}