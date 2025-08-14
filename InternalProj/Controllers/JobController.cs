using InternalProj.Data;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InternalProj.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Fetch job stages for a given job
        private async Task<JobDetailsViewModel> GetJobWorkflow(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobStages)
                    .ThenInclude(js => js.JobStageTemplate)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null)
                return null;

            var orderedStages = job.JobStages
                .OrderBy(js => js.JobStageTemplate.Sequence)
                .ToList();

            // Only in-progress stage counts as current
            var currentStageEntity = orderedStages.FirstOrDefault(s => s.InProgress);

            return new JobDetailsViewModel
            {
                JobId = job.JobId,
                WorkOrderId = job.WorkOrderId,
                CurrentStage = currentStageEntity?.JobStageTemplate.StageName ?? "", // empty if nothing in progress
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
        }


        //private string GetCurrentStageName(IEnumerable<JobStage> stages)
        //{
        //    var currentStage = stages.FirstOrDefault(s => s.InProgress);
        //    return currentStage?.JobStageTemplate.StageName ?? string.Empty;
        //}

        [HttpPost]
        public async Task<IActionResult> AdvanceStage(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobStages)
                    .ThenInclude(js => js.JobStageTemplate)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return NotFound();

            var currentStage = job.JobStages.FirstOrDefault(s => s.InProgress);
            if (currentStage != null)
            {
                currentStage.IsCompleted = true;
                currentStage.InProgress = false;

                var nextStage = job.JobStages
                    .Where(s => s.JobStageTemplate.Sequence > currentStage.JobStageTemplate.Sequence)
                    .OrderBy(s => s.JobStageTemplate.Sequence)
                    .FirstOrDefault();

                if (nextStage != null)
                {
                    nextStage.InProgress = true;
                    nextStage.IsCompleted = false;
                }
            }

            await _context.SaveChangesAsync();

            var viewModel = await GetJobWorkflow(jobId);
            return PartialView("~/Views/JobWorkflow/_WorkFlowDetailsPartial.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RollbackStage(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobStages)
                    .ThenInclude(js => js.JobStageTemplate)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return NotFound();

            // Get the current stage: either in-progress OR the last completed stage
            var currentStage = job.JobStages
                .FirstOrDefault(s => s.InProgress)
                ?? job.JobStages
                    .Where(s => s.IsCompleted)
                    .OrderByDescending(s => s.JobStageTemplate.Sequence)
                    .FirstOrDefault();

            if (currentStage != null)
            {
                // Mark current as not in-progress and not completed (optional)
                currentStage.InProgress = false;
                currentStage.IsCompleted = false;

                // Find previous stage
                var prevStage = job.JobStages
                    .Where(s => s.JobStageTemplate.Sequence < currentStage.JobStageTemplate.Sequence)
                    .OrderByDescending(s => s.JobStageTemplate.Sequence)
                    .FirstOrDefault();

                if (prevStage != null)
                {
                    prevStage.InProgress = true;
                    prevStage.IsCompleted = false;
                }
            }

            await _context.SaveChangesAsync();

            var viewModel = await GetJobWorkflow(jobId);
            return PartialView("~/Views/JobWorkflow/_WorkFlowDetailsPartial.cshtml", viewModel);
        }


    }
}
