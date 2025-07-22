using InternalProj.Data;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InternalProj.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var summary = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .OrderByDescending(w => w.Wdate)
                .Take(5)
                .ToList()
                .Select(w =>
                {
                    var totalPaid = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .Sum(r => (double?)r.CurrentAmount) ?? 0;

                    return new WorkOrderSummaryViewModel
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        StudioName = w.Customer?.StudioName,
                        Advance = w.Advance ?? 0,
                        SubTotal = w.SubTotal,
                        TotalPaid = totalPaid,
                        //Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
                        Balance = w.Balance ?? 0,
                        WorkTypeName = w.WorkType?.TypeName,
                        Wdate = w.Wdate
                    };
                })
                .ToList();

            return View(summary);
        }

        public IActionResult GetBoxContent(int id, int page = 1)
        {
            if (id == 1)
            {
                int pageSize = 10;

                var workOrders = _context.WorkOrders
                    .AsNoTracking()
                    .Include(w => w.Customer)
                    .Include(w => w.WorkType)
                    .OrderByDescending(w => w.Wdate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModelList = workOrders.Select(w =>
                {
                    var totalPaid = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .Sum(r => (double?)r.CurrentAmount) ?? 0;

                    return new WorkOrderSummaryViewModel
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        StudioName = w.Customer?.StudioName,
                        Advance = w.Advance ?? 0,
                        SubTotal = w.SubTotal,
                        TotalPaid = totalPaid,
                        //Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
                        Balance = w.Balance ?? 0,
                        WorkTypeName = w.WorkType?.TypeName,
                        Wdate = w.Wdate
                    };
                }).ToList();

                var model = new WorkOrderViewModel
                {
                    ResultsSummary = viewModelList,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(_context.WorkOrders.Count() / (double)pageSize),
                    WorkTypes = _context.WorkTypes.ToList(),
                    StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList()
                };

                return PartialView("_WorkOrderDetailsPartialDashboard", model);
            }

            if (id == 2)
            {
                var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                var today = indiaTime.Date;


                var transactions = _context.Receipts
                    .Where(r => r.ReceiptDate.Date == today)
                    .Select(r => new DailyTransactionViewModel
                    {
                        Date = r.ReceiptDate,
                        Amount = Convert.ToDecimal(r.CurrentAmount),
                        Type = "Receipt",
                        Source = "Receipt",
                        StudioName = r.Customer.StudioName
                    })
                    .Union(_context.WorkOrders
                        .Where(w => w.Wdate == today)
                        .Select(w => new DailyTransactionViewModel
                        {
                            Date = w.Wdate.Value,
                            Amount = Convert.ToDecimal(w.Advance ?? 0),
                            Type = "WorkOrder",
                            Source = "WorkOrder",
                            StudioName = w.Customer.StudioName
                        }))
                    .OrderByDescending(t => t.Date)
                    .ToList();

                return PartialView("_DailyTransactionPaginatedPartial", transactions);
            }


            return PartialView("_PlaceholderPartial");
        }

        public IActionResult Details(string id)
        {
            var workOrder = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .FirstOrDefault(w => w.WorkOrderNo == id);

            if (workOrder == null)
                return NotFound();

            var workDetails = _context.WorkDetails
                .Include(w => w.SubHead)
                .Include(w => w.ChildSubHead)
                .Include(w => w.Size)
                .Where(w => w.WorkOrderId == workOrder.WorkOrderId)
                .ToList();

            var vm = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                WorkDetailsList = workDetails
            };

            return View(vm);
        }

        public IActionResult GetWorkOrderSummary()
        {
            var summary = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .OrderByDescending(w => w.Wdate)
                .Take(5)
                .ToList()
                .Select(w =>
                {
                    var totalPaid = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .Sum(r => (double?)r.CurrentAmount) ?? 0;

                    return new WorkOrderSummaryViewModel
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        StudioName = w.Customer?.StudioName,
                        Advance = w.Advance ?? 0,
                        SubTotal = w.SubTotal,
                        TotalPaid = totalPaid,
                        //Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
                        Balance = w.Balance ?? 0,
                        WorkTypeName = w.WorkType?.TypeName,
                        Wdate = w.Wdate
                    };
                })
                .ToList();

            return PartialView("_WorkOrderSummaryPartial", summary);
        }

        public IActionResult DailyTransactionSummaryPartial()
        {
            try
            {
                // Get IST Timezone safely
                TimeZoneInfo indiaZone;
                try
                {
                    indiaZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    indiaZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                }

                // Get today's date in IST
                var nowIST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);
                var todayIST = nowIST.Date;

                // Convert today's date range in IST to UTC for DB comparison
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayIST, indiaZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(todayIST.AddDays(1).AddTicks(-1), indiaZone);

                // Advance from WorkOrders
                var advanceData = _context.WorkOrders
                    .Include(w => w.Customer)
                    .Where(w => w.Wdate >= startUtc && w.Wdate <= endUtc && w.Advance > 0)
                    .Select(w => new DailyTransactionViewModel
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        StudioName = w.Customer.StudioName,
                        Amount = (decimal)(w.Advance ?? 0),
                        TransactionDate = (w.Wdate ?? DateTime.MinValue)
                    });

                // Receipts
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

                // Invoices
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

                // Combine all transactions
                var allTransactions = advanceData
                    .Concat(receiptData)
                    .Concat(invoiceData)
                    .ToList()
                    .GroupBy(t => t.WorkOrderId)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(x => x.TransactionDate).First();
                        return new DailyTransactionViewModel
                        {
                            WorkOrderId = g.Key,
                            WorkOrderNo = latest.WorkOrderNo,
                            StudioName = latest.StudioName,
                            BillAmount = g.Sum(x => x.Amount),
                            LastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(latest.TransactionDate.ToUniversalTime(), indiaZone)
                        };
                    })
                    .OrderByDescending(x => x.LastTransactionDate)
                    .Take(5)
                    .ToList();

                return PartialView("_DailyTransactionSummaryPartial", allTransactions);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message + "<br><br>StackTrace:<br>" + ex.StackTrace, "text/html");
            }
        }
    }
}

