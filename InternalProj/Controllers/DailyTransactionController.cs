using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternalProj.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InternalProj.Controllers
{
    public class DailyTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public DailyTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Get today's receipts
            var todayReceipts = _context.Receipts
                .Where(r => r.ReceiptDate >= today && r.ReceiptDate < tomorrow)
                .Select(r => new
                {
                    r.WorkOrderId,
                    Date = r.ReceiptDate,
                    Source = "Receipt"
                });

            // Get today's invoices
            var todayInvoices = _context.Invoices
                .Where(i => i.BillDate >= today && i.BillDate < tomorrow)
                .Select(i => new
                {
                    i.WorkOrderId,
                    Date = i.BillDate,
                    Source = "Invoice"
                });

            // Get today's work orders (advance payments)
            var todayWorkOrders = _context.WorkOrders
                .Where(w => w.Wdate.HasValue && w.Wdate.Value >= today && w.Wdate.Value < tomorrow)
                .Select(w => new
                {
                    w.WorkOrderId,
                    Date = w.Wdate.Value,
                    Source = "Advance"
                });

            // Combine all today's transactions
            var allTodayTransactions = todayReceipts
                .Union(todayInvoices)
                .Union(todayWorkOrders)
                .ToList();

            // Get latest transaction per WorkOrderId
            var latestTransactions = allTodayTransactions
                .GroupBy(t => t.WorkOrderId)
                .Select(g => g.OrderByDescending(x => x.Date).First())
                .OrderByDescending(t => t.Date)
                .ToList();

            int totalCount = latestTransactions.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var pagedTransactions = latestTransactions
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var workOrderIds = pagedTransactions.Select(x => x.WorkOrderId).ToList();

            var workOrders = _context.WorkOrders
                .Include(w => w.Customer)
                .Where(w => workOrderIds.Contains(w.WorkOrderId))
                .ToList();

            var receipts = _context.Receipts
                .Where(r => workOrderIds.Contains(r.WorkOrderId))
                .ToList();

            var invoices = _context.Invoices
                .Where(i => workOrderIds.Contains(i.WorkOrderId))
                .ToList();

            var result = pagedTransactions.Select(tran =>
            {
                var w = workOrders.FirstOrDefault(x => x.WorkOrderId == tran.WorkOrderId);

                decimal advanceAmount = (decimal)(w?.Advance ?? 0);

                decimal rTotal = receipts
                    .Where(r => r.WorkOrderId == tran.WorkOrderId &&
                                r.ReceiptDate >= today && r.ReceiptDate < tomorrow)
                    .Sum(r => (decimal?)r.CurrentAmount) ?? 0;

                decimal iTotal = invoices
                    .Where(i => i.WorkOrderId == tran.WorkOrderId &&
                                i.BillDate >= today && i.BillDate < tomorrow)
                    .Sum(i => (decimal?)i.NetAmount) ?? 0;

                decimal amount = tran.Source switch
                {
                    "Advance" => advanceAmount,
                    "Receipt" => rTotal,
                    "Invoice" => iTotal - advanceAmount - rTotal,
                    _ => 0
                };

                return new DailyTransactionViewModel
                {
                    WorkOrderId = w?.WorkOrderId ?? 0,
                    WorkOrderNo = w?.WorkOrderNo ?? "N/A",
                    StudioName = w?.Customer?.StudioName ?? "N/A",
                    Date = tran.Date,
                    BillAmount = amount,
                    LastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(tran.Date, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                };
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
        }

        private List<DailyTransactionViewModel> GetPaginatedTransactions(int page, out int totalPages)
        {
            var indiaZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var nowIST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);
            var todayIST = nowIST.Date;

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayIST, indiaZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(todayIST.AddDays(1).AddTicks(-1), indiaZone);

            // Get today's Advance, Receipts, and Invoices
            var advanceData = _context.WorkOrders
                .Include(w => w.Customer)
                .Where(w => w.Wdate >= startUtc && w.Wdate <= endUtc && w.Advance > 0)
                .Select(w => new DailyTransactionViewModel
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNo = w.WorkOrderNo,
                    StudioName = w.Customer.StudioName,
                    Amount = (decimal)(w.Advance ?? 0),
                    TransactionDate = w.Wdate ?? DateTime.MinValue
                });

            var receiptData = _context.Receipts
                .Include(r => r.WorkOrder)
                .ThenInclude(w => w.Customer)
                .Where(r => r.ReceiptDate >= startUtc && r.ReceiptDate <= endUtc)
                .Select(r => new DailyTransactionViewModel
                {
                    WorkOrderId = r.WorkOrderId,
                    WorkOrderNo = r.WorkOrder.WorkOrderNo,
                    StudioName = r.WorkOrder.Customer.StudioName,
                    Amount = (decimal)r.CurrentAmount,
                    TransactionDate = r.ReceiptDate
                });

            var invoiceData = _context.Invoices
                .Include(i => i.WorkOrder)
                .ThenInclude(w => w.Customer)
                .Where(i => i.BillDate >= startUtc && i.BillDate <= endUtc)
                .Select(i => new DailyTransactionViewModel
                {
                    WorkOrderId = i.WorkOrderId,
                    WorkOrderNo = i.WorkOrder.WorkOrderNo,
                    StudioName = i.WorkOrder.Customer.StudioName,
                    Amount = (decimal)(i.NetAmount ?? 0),
                    TransactionDate = i.BillDate
                });

            // Merge all and group by WorkOrderId
            var allTransactions = advanceData
                .Concat(receiptData)
                .Concat(invoiceData)
                .ToList();

            var grouped = allTransactions
                .GroupBy(x => x.WorkOrderId)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.TransactionDate).First();
                    return new DailyTransactionViewModel
                    {
                        WorkOrderId = latest.WorkOrderId,
                        WorkOrderNo = latest.WorkOrderNo,
                        StudioName = latest.StudioName,
                        BillAmount = g.Sum(x => x.Amount),
                        LastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(latest.TransactionDate.ToUniversalTime(), indiaZone)
                    };
                })
                .OrderByDescending(x => x.LastTransactionDate)
                .ToList();

            totalPages = (int)Math.Ceiling(grouped.Count / (double)PageSize);

            return grouped
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult PaginatedTransactionsPartial(int page = 1)
        {
            var transactions = GetPaginatedTransactions(page, out int totalPages);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return PartialView("_DailyTransactionPaginatedPartial", transactions);
        }


    }
}