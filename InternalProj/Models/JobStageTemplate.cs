using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class JobStageTemplate
    {
        [Key]
        public int JobStageTemplateId { get; set; }
        public string StageName { get; set; }
        public int Sequence { get; set; }

        public ICollection<JobStage> JobStages { get; set; } = new List<JobStage>();
    }
}
