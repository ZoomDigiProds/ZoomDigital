//namespace InternalProj.ViewModel
//{
//    public class ReceiptHistoryViewModel
//    {
//    }
//}


using InternalProj.Models;

namespace InternalProj.ViewModel
{
    public class ReceiptHistoryViewModel
    {
        public string? CustomerName { get; set; }
        public string? StudioName { get; set; }
        public List<Receipt>? Receipts { get; set; }
    }

}