namespace InternalProj.Models
{
    public class JobStage
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }

        public int JobStageTemplateId { get; set; }
        public JobStageTemplate JobStageTemplate { get; set; }

        // Only one stage per job will have Status = true (current stage)
        public bool Status { get; set; }
    }

}
