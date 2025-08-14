using InternalProj.Data;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using System.Linq;

namespace InternalProj.Workflow
{
    public class UpdateJobStageStep : StepBody
    {
        private readonly ApplicationDbContext _context;
        public int JobId { get; set; }

        public UpdateJobStageStep(ApplicationDbContext context)
        {
            _context = context;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var stages = _context.JobStages
                .Include(s => s.JobStageTemplate)
                .Where(s => s.JobId == JobId)
                .OrderBy(s => s.JobStageTemplate.Sequence)
                .ToList();

            var currentStage = stages.FirstOrDefault(s => s.InProgress);
            if (currentStage != null)
            {
                // Mark current as completed, remove in progress
                currentStage.InProgress = false;
                currentStage.IsCompleted = true;

                var nextIndex = stages.IndexOf(currentStage) + 1;
                if (nextIndex < stages.Count)
                {
                    stages[nextIndex].InProgress = true;
                    stages[nextIndex].IsCompleted = false;
                }
                else
                {
                    return ExecutionResult.Next(); // Last stage reached
                }

                _context.SaveChanges();
            }

            return ExecutionResult.Next();
        }
    }
}
