using InternalProj.Data;
using InternalProj.Models;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using System.Linq;

namespace InternalProj.Workflow
{
    public class CompleteJobStep : StepBody
    {
        private readonly ApplicationDbContext _context;

        public int JobId { get; set; }

        public CompleteJobStep(ApplicationDbContext context)
        {
            _context = context;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            // Load job with stages
            var job = _context.Jobs
                              .Include(j => j.JobStages)
                              .ThenInclude(js => js.JobStageTemplate)
                              .FirstOrDefault(j => j.JobId == JobId);

            if (job != null && job.JobStages.Any())
            {
                // Mark all stages as completed, none in progress
                foreach (var stage in job.JobStages)
                {
                    stage.IsCompleted = true;
                    stage.InProgress = false;
                }

                // Find "Completed" stage and set it in progress if you want to highlight it
                var completedStage = job.JobStages
                    .FirstOrDefault(s => s.JobStageTemplate.StageName == "Completed");

                if (completedStage != null)
                    completedStage.InProgress = true;

                _context.SaveChanges();
            }

            return ExecutionResult.Next();
        }
    }
}
