using InternalProj.Data;
using InternalProj.Models;
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
            var templates = _context.JobStageTemplates.OrderBy(j => j.Id).ToList();
            if (!templates.Any())
                return ExecutionResult.Next(); // nothing to create

            foreach (var template in templates)
            {
                var jobStage = new JobStage
                {
                    JobId = JobId,
                    JobStageTemplateId = template.Id,
                    Status = false
                };
                _context.JobStages.Add(jobStage);
            }

            _context.SaveChanges();

            return ExecutionResult.Next();
        }
    }


}
