using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace InternalProj.Workflow
{

    public class JobWorkflow : IWorkflow<JobWorkflowData>
    {
        public string Id => "JobWorkflow";
        public int Version => 1;

        public void Build(IWorkflowBuilder<JobWorkflowData> builder)
        {
            builder
                .StartWith<InitializeJobStagesStep>()
                .Then<UpdateJobStageStep>()
                .Then<CompleteJobStep>();
        }
    }

}
