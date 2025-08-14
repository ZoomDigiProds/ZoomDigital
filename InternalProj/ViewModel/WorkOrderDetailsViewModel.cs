using InternalProj.Models;
using System.Collections.Generic;

namespace InternalProj.ViewModel
{
    public class WorkOrderDetailsViewModel
    {
        public WorkOrderMaster WorkOrder { get; set; }

        public List<JobDetailsViewModel> Jobs { get; set; } = new();
    }

    public class JobDetailsViewModel
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }

        // Required for stepper buttons
        public int JobId { get; set; }

        // All stages for this job
        public List<JobStageStepperViewModel> Stages { get; set; } = new();

        // Name of the job
        public string JobName { get; set; }

        // The current active stage name
        public string CurrentStage { get; set; }

        // Convenience list of stage names for stepper rendering
        public List<string> AllStages { get; set; } = new();
    }
}
