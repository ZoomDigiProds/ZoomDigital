using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    //[DepartmentAuthorize("ADMIN")]
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
                .OrderByDescending(w => w.Wdate)
                .Take(5)
                .Select(w => new WorkOrderSummaryViewModel
                {
                    WorkOrderNo = w.WorkOrderNo,
                    StudioName = w.Customer.StudioName,
                    Advance = w.Advance ?? 0.0,
                    SubTotal = w.SubTotal
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
                    .Include(w => w.Customer)
                    .Include(w => w.WorkType)
                    .OrderByDescending(w => w.Wdate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new WorkOrderViewModel
                {
                    Results = workOrders,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(_context.WorkOrders.Count() / (double)pageSize),
                    WorkTypes = _context.WorkTypes.ToList(),
                    StudioList = _context.CustomerRegs.Select(c => c.StudioName).Distinct().ToList()
                };


                return PartialView("_WorkOrderDetailsPartialDashboard", model);
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
                .OrderByDescending(w => w.Wdate)
                .Take(5)
                .Select(w => new WorkOrderSummaryViewModel
                {
                    WorkOrderNo = w.WorkOrderNo,
                    StudioName = w.Customer.StudioName,
                    Advance = (double)w.Advance,
                    SubTotal = w.SubTotal
                })
                .ToList();

            return PartialView("_WorkOrderSummaryPartial", summary);
        }

    }
}
