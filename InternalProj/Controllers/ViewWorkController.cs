using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize("ADMIN")]
    public class ViewWorkController : Controller
    {
       private readonly ApplicationDbContext _context;

        public ViewWorkController(ApplicationDbContext context)
        {
            _context = context;
        }
    
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new WorkOrderViewModel();

            viewModel.StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList();
            viewModel.WorkTypes = _context.WorkTypes.ToList();

            viewModel.Results = new List<WorkOrderMaster>();
            viewModel.CurrentPage = 1;
            viewModel.TotalPages = 0;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(string studio, DateTime? fromDate, DateTime? toDate, int? workTypeId, int currentPage = 1)
        {
            const int pageSize = 10;

            var viewModel = new WorkOrderViewModel();

            var query = _context.WorkOrders
                .Where(w => w.Active == "Y")
                .AsQueryable();

            if (!string.IsNullOrEmpty(studio))
            {
                query = query.Where(w =>
                    w.Customer != null &&
                    w.Customer.StudioName.Trim().ToLower() == studio.Trim().ToLower());
            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(w => w.Wdate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(w => w.Wdate <= toUtc);
            }


            if (workTypeId.HasValue && workTypeId.Value > 0)
            {
                query = query.Where(w => w.WorkTypeId == workTypeId);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var results = query
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .OrderBy(w => w.Wdate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            viewModel.StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList();
            viewModel.WorkTypes = _context.WorkTypes.ToList();
            viewModel.Results = results;
            viewModel.CurrentPage = currentPage;
            viewModel.TotalPages = totalPages;
            viewModel.StudioFilter = studio;
            viewModel.FromDateFilter = fromDate;
            viewModel.ToDateFilter = toDate;
            viewModel.WorkTypeFilter = workTypeId;

            return View(viewModel);
        }
    }
}
