using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class JobStage
    {
        [Key]
        public int JobStageId { get; set; }

        // FK to Job
        public int JobId { get; set; }
        public Job Job { get; set; }

        // FK to JobStageTemplate
        public int JobStageTemplateId { get; set; }
        public JobStageTemplate JobStageTemplate { get; set; }

        public bool InProgress { get; set; }
       
        public bool IsCompleted { get; set; }
    }
}
