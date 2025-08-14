using InternalProj.Data;
using InternalProj.Models;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace InternalProj.Workflow
{
    public class InitializeJobStagesStep : StepBody
    {
        private readonly ApplicationDbContext _context;
        public int JobId { get; set; }

        public InitializeJobStagesStep(ApplicationDbContext context)
        {
            _context = context;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var exists = _context.JobStages.Any(s => s.JobId == JobId);
            if (exists)
                return ExecutionResult.Next();

            var templates = _context.JobStageTemplates
                                     .OrderBy(t => t.Sequence)
                                     .ToList();
            if (!templates.Any())
                return ExecutionResult.Next();

            foreach (var template in templates)
            {
                var jobStage = new JobStage
                {
                    JobId = JobId,
                    JobStageTemplateId = template.JobStageTemplateId,
                    InProgress = template.Sequence == 1, // first stage active
                    IsCompleted = false                  // none completed initially
                };
                _context.JobStages.Add(jobStage);
            }

            _context.SaveChanges();
            return ExecutionResult.Next();
        }
    }
}
