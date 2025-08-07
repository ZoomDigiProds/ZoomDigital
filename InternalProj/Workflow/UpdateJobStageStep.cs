using InternalProj.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace InternalProj.Workflow
{

    public class UpdateJobStageStep : StepBody
    {
        private readonly ApplicationDbContext _context;

        public UpdateJobStageStep(ApplicationDbContext context)
        {
            _context = context;
        }

        public int JobId { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            // Advance to next stage logic here
            return ExecutionResult.Next();
        }
    }


}
