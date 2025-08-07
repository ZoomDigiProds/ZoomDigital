namespace InternalProj.Models
{
    public class Job
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }

        public ICollection<JobStage> JobStages { get; set; }
    }
}
