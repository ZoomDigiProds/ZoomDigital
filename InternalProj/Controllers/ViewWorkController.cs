using InternalProj.Data;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    public class ViewWorkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ViewWorkController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1, bool isSearch = false)
        //{
        //    const int pageSize = 10;

        //    bool hasAnyFilter = !string.IsNullOrEmpty(studio) || fromDate.HasValue || toDate.HasValue || (workTypeId.HasValue && workTypeId.Value > 0);

        //    // Only load results if user clicked search
        //    bool shouldShowResults = isSearch || hasAnyFilter;

        //    var query = _context.WorkOrders
        //        .Include(w => w.Customer)
        //        .Include(w => w.WorkType)
        //        .Where(w => w.Active == "Y");

        //    if (shouldShowResults)
        //    {
        //        if (!string.IsNullOrWhiteSpace(studio))
        //            query = query.Where(w => w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());

        //        if (fromDate.HasValue)
        //            query = query.Where(w => w.Wdate >= DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc));

        //        if (toDate.HasValue)
        //            query = query.Where(w => w.Wdate <= DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));

        //        if (workTypeId.HasValue && workTypeId.Value > 0)
        //            query = query.Where(w => w.WorkTypeId == workTypeId.Value);
        //    }
        //    else
        //    {
        //        // Don't return anything
        //        query = query.Where(w => false);
        //    }

        //    var totalItems = query.Count();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        //    var viewModel = new WorkOrderViewModel
        //    {
        //        StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList(),
        //        WorkTypes = _context.WorkTypes.ToList(),
        //        ResultsSummary = query
        //            .OrderBy(w => w.Wdate)
        //            .Skip((currentPage - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToList()
        //            .Select(w =>
        //            {
        //                var totalPaid = _context.Receipts
        //                    .Where(r => r.WorkOrderId == w.WorkOrderId)
        //                    .Sum(r => (double?)r.CurrentAmount) ?? 0;

        //                return new WorkOrderSummaryViewModel
        //                {
        //                    WorkOrderId = w.WorkOrderId,
        //                    WorkOrderNo = w.WorkOrderNo,
        //                    StudioName = w.Customer?.StudioName,
        //                    Advance = w.Advance ?? 0,
        //                    SubTotal = w.SubTotal,
        //                    TotalPaid = totalPaid,
        //                    Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
        //                    WorkTypeName = w.WorkType?.TypeName,
        //                    Wdate = w.Wdate
        //                };
        //            }).ToList(),

        //        CurrentPage = currentPage,
        //        TotalPages = totalPages,
        //        StudioFilter = studio,
        //        FromDateFilter = fromDate,
        //        ToDateFilter = toDate,
        //        WorkTypeFilter = workTypeId
        //    };

        //    return View(viewModel);
        //}


        [HttpGet]
        public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1, bool isSearch = false)
        {
            const int pageSize = 10;

            bool hasAnyFilter = !string.IsNullOrEmpty(studio) || fromDate.HasValue || toDate.HasValue || (workTypeId.HasValue && workTypeId.Value > 0);

            bool shouldShowResults = isSearch || hasAnyFilter;

            var query = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .Where(w => w.Active == "Y");

            if (shouldShowResults)
            {
                if (!string.IsNullOrWhiteSpace(studio))
                    query = query.Where(w => w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());

                if (fromDate.HasValue)
                    query = query.Where(w => w.Wdate >= DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc));

                if (toDate.HasValue)
                    query = query.Where(w => w.Wdate <= DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));

                if (workTypeId.HasValue && workTypeId.Value > 0)
                    query = query.Where(w => w.WorkTypeId == workTypeId.Value);
            }
            else
            {
                query = query.Where(w => false);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var viewModel = new WorkOrderViewModel
            {
                StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList(),
                WorkTypes = _context.WorkTypes.ToList(),
                ResultsSummary = query
                    .OrderBy(w => w.Wdate)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
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
                            Balance = Math.Max(0, w.SubTotal - (w.Advance ?? 0) - totalPaid),
                            WorkTypeName = w.WorkType?.TypeName,
                            Wdate = w.Wdate
                        };
                    }).ToList(),

                CurrentPage = currentPage,
                TotalPages = totalPages,
                StudioFilter = studio,
                FromDateFilter = fromDate,
                ToDateFilter = toDate,
                WorkTypeFilter = workTypeId
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WorkOrderResultsPartial", viewModel);
            }

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult Search(string studio, DateTime? fromDate, DateTime? toDate, int? WorkTypeFilter, int currentPage = 1)
        {
            return RedirectToAction("Index", new
            {
                studio,
                fromDate = fromDate?.ToString("yyyy-MM-dd"),
                toDate = toDate?.ToString("yyyy-MM-dd"),
                workTypeId = WorkTypeFilter,
                currentPage,
                isSearch = true,
            });
        }
    }
}
