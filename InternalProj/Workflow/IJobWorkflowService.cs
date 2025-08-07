using InternalProj.Models;

namespace InternalProj.Workflow
{

        public interface IJobWorkflowService
        {
            Task<Job> CreateJobWithStagesAsync(int workOrderId);
        }
    


}
