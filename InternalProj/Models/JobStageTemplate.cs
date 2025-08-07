namespace InternalProj.Models
{
    public class JobStageTemplate
    {
        public int Id { get; set; }
        public string StageName { get; set; }
        public int Sequence { get; set; }

        public ICollection<JobStage> JobStages { get; set; }
    }
}
