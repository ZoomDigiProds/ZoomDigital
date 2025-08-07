using InternalProj.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace InternalProj.Workflow
{
    public class CompleteJobStep : StepBody
    {
        private readonly ApplicationDbContext _context;

        public CompleteJobStep(ApplicationDbContext context)
        {
            _context = context;
        }

        public int JobId { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            // Mark job as complete
            return ExecutionResult.Next();
        }
    }

}
