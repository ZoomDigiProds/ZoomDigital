using InternalProj.Data;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InternalProj.Controllers
{
    public class JobWorkflowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobWorkflowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List of Work Orders
        public async Task<IActionResult> Index()
        {
            var workOrders = await _context.WorkOrders
                .OrderByDescending(w => w.WorkOrderId)
                .ToListAsync();

            return View(workOrders);
        }

        // GET: Work Order Details (Jobs + Stages)
        public async Task<IActionResult> Details(int id)
        {
            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id);

            if (workOrder == null)
                return NotFound();

            var jobs = await _context.Jobs
                .Where(j => j.WorkOrderId == id)
                .Include(j => j.JobStages)
                    .ThenInclude(js => js.JobStageTemplate)
                .OrderBy(j => j.JobId)
                .ToListAsync();

            var jobViewModels = jobs.Select(j =>
            {
                var orderedStages = j.JobStages
                    .OrderBy(js => js.JobStageTemplate.Sequence)
                    .ToList();

                var currentStageEntity = orderedStages.FirstOrDefault(s => s.InProgress)
                                          ?? orderedStages.FirstOrDefault();

                return new JobDetailsViewModel
                {
                    JobId = j.JobId,
                    WorkOrderId = j.WorkOrderId,
                    //JobName = j.JobStages,
                    CurrentStage = currentStageEntity?.JobStageTemplate.StageName ?? "",
                    AllStages = orderedStages.Select(s => s.JobStageTemplate.StageName).ToList(),
                    Stages = orderedStages.Select(js => new JobStageStepperViewModel
                    {
                        Id = js.JobStageId,
                        JobStageTemplateId = js.JobStageTemplateId,
                        StageName = js.JobStageTemplate.StageName,
                        Sequence = js.JobStageTemplate.Sequence,
                        IsCompleted = js.IsCompleted,
                        InProgress = js.InProgress
                    }).ToList()
                };
            }).ToList();

            var vm = new WorkOrderDetailsViewModel
            {
                WorkOrder = workOrder,
                Jobs = jobViewModels
            };

            return View(vm);
        }
    }
}
