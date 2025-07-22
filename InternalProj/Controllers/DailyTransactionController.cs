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

        // Full list with pagination
        public IActionResult Index(int page = 1)
        {
            var today = DateTime.UtcNow.Date;

            var todayReceipts = _context.Receipts
                .Where(r => r.ReceiptDate >= today && r.ReceiptDate < today.AddDays(1))
                .Select(r => new
                {
                    r.WorkOrderId,
                    Date = r.ReceiptDate,
                    Source = "Receipt"
                });

            var todayInvoices = _context.Invoices
                .Where(i => i.BillDate.Date == today)
                .Select(i => new
                {
                    i.WorkOrderId,
                    Date = i.BillDate,
                    Source = "Invoice"
                });

            var todayWorkOrders = _context.WorkOrders
                .Where(w => w.Wdate.HasValue && w.Wdate.Value.Date == today)
                .Select(w => new
                {
                    w.WorkOrderId,
                    Date = w.Wdate.Value,
                    Source = "Advance"
                });

            var allTodayTransactions = todayReceipts
                .Union(todayInvoices)
                .Union(todayWorkOrders)
                .ToList();

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
                var rTotal = receipts.Where(r => r.WorkOrderId == w.WorkOrderId).Sum(r => (decimal?)r.CurrentAmount) ?? 0;
                var iTotal = invoices.Where(i => i.WorkOrderId == w.WorkOrderId).Sum(i => (decimal?)i.NetAmount) ?? 0;

                decimal amount = tran.Source switch
                {
                    "Advance" => (decimal)w.Advance,
                    "Receipt" => rTotal,
                    "Invoice" => iTotal - (decimal)w.Advance - rTotal,
                    _ => 0
                };

                return new DailyTransactionViewModel
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNo = w.WorkOrderNo,
                    StudioName = w.Customer?.StudioName ?? "",
                    Date = tran.Date,
                    BillAmount = amount,
                    LastTransactionDate = tran.Date.ToLocalTime()
                };
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
        }

        private List<DailyTransactionViewModel> GetPaginatedTransactions(int page, out int totalPages)
        {
            var today = DateTime.UtcNow.Date;

            var receipts = _context.Receipts
                .Where(r => r.ReceiptDate.Date == today)
                .Select(r => new { r.WorkOrderId, Date = r.ReceiptDate, Source = "Receipt" });

            var invoices = _context.Invoices
                .Where(i => i.BillDate.Date == today)
                .Select(i => new { i.WorkOrderId, Date = i.BillDate, Source = "Invoice" });

            var advances = _context.WorkOrders
                .Where(w => w.Wdate.HasValue && w.Wdate.Value.Date == today)
                .Select(w => new { w.WorkOrderId, Date = w.Wdate.Value, Source = "Advance" });

            var all = receipts
                .Union(invoices)
                .Union(advances)
                .ToList();

            var latestTransactions = all
                .GroupBy(x => x.WorkOrderId)
                .Select(g => g.OrderByDescending(x => x.Date).First())
                .OrderByDescending(x => x.Date)
                .ToList();

            totalPages = (int)Math.Ceiling(latestTransactions.Count / (double)PageSize);
            var paged = latestTransactions
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var workOrderIds = paged.Select(p => p.WorkOrderId).ToList();

            var workOrders = _context.WorkOrders
                .Include(w => w.Customer)
                .Where(w => workOrderIds.Contains(w.WorkOrderId))
                .ToList();

            var receiptsMap = _context.Receipts
                .Where(r => workOrderIds.Contains(r.WorkOrderId))
                .ToList();

            var invoicesMap = _context.Invoices
                .Where(i => workOrderIds.Contains(i.WorkOrderId))
                .ToList();

            var vm = paged.Select(tran =>
            {
                var w = workOrders.FirstOrDefault(x => x.WorkOrderId == tran.WorkOrderId);
                var rTotal = receiptsMap.Where(r => r.WorkOrderId == w.WorkOrderId).Sum(r => (decimal?)r.CurrentAmount) ?? 0;
                var iTotal = invoicesMap.Where(i => i.WorkOrderId == w.WorkOrderId).Sum(i => (decimal?)i.NetAmount) ?? 0;

                decimal amount = tran.Source switch
                {
                    "Advance" => (decimal)w.Advance,
                    "Receipt" => rTotal,
                    "Invoice" => iTotal - (decimal)w.Advance - rTotal,
                    _ => 0
                };

                return new DailyTransactionViewModel
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNo = w.WorkOrderNo,
                    StudioName = w.Customer?.StudioName ?? "",
                    Date = tran.Date,
                    BillAmount = amount,
                    LastTransactionDate = tran.Date.ToLocalTime()
                };
            }).ToList();

            return vm;
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