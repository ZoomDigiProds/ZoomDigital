using InternalProj.Workflow;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace InternalProj.Workflows
{
    public class JobWorkflowData
    {
        public int JobId { get; set; }
    }

    public class JobWorkflow : IWorkflow<JobWorkflowData>
    {
        public string Id => "JobWorkflow";
        public int Version => 1;

        public void Build(IWorkflowBuilder<JobWorkflowData> builder)
        {
            builder
                .StartWith<InitializeJobStagesStep>()
                    .Input(step => step.JobId, data => data.JobId)
                .Then<UpdateJobStageStep>()
                    .Input(step => step.JobId, data => data.JobId)
                .Then<CompleteJobStep>()
                    .Input(step => step.JobId, data => data.JobId);
        }
    }
}
