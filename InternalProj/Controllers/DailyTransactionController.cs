using InternalProj.Data;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace InternalProj.Controllers
{
    public class DailyTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.UtcNow.Date;

            var transactions = _context.WorkOrders
                .Include(w => w.Customer)
                .Where(w => w.Wdate.HasValue && w.Wdate.Value.Date == today)
                .Select(w => new DailyTransactionSummaryViewModel
                {
                    StudioName = w.Customer.StudioName,
                    SubTotal = w.SubTotal,
                    Advance = w.Advance ?? 0,
                    TotalPaid = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .Sum(r => (double?)r.CurrentAmount) ?? 0,
                    Date = w.Wdate.Value
                })
                .OrderByDescending(w => w.Date)
                .ToList();

            ViewBag.TotalAmount = transactions.Sum(t => t.SubTotal);

            return View(transactions);
        }

        public IActionResult RefreshTable()
        {
            var today = DateTime.UtcNow.Date;

            var transactions = _context.WorkOrders
             .Where(w => w.Wdate.HasValue && w.Wdate.Value.Date == today)
             .Include(w => w.Customer)
             .Select(w => new DailyTransactionSummaryViewModel
             {
                 StudioName = w.Customer.StudioName,
                 SubTotal = w.SubTotal,
                 Advance = w.Advance ?? 0,
                 Date = w.Wdate.Value,
                 TotalPaid = _context.Receipts
                     .Where(r => r.WorkOrderId == w.WorkOrderId)
                     .Sum(r => (double?)r.CurrentAmount) ?? 0
             })
             .OrderByDescending(w => w.Date)
             .ToList();

            return PartialView("_DailyTransactionTablePartial", transactions);
        }

    }
}