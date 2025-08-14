using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        public int WorkOrderId { get; set; } // FK
        public WorkOrderMaster? WorkOrder { get; set; } // Navigation

        public ICollection<JobStage> JobStages { get; set; }
    }

}
