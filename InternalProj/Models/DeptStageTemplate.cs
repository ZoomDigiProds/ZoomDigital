using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class DeptStageTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DeptId { get; set; }

        [ForeignKey("DeptId")]
        public DeptMaster Department { get; set; }

        [Required]
        public int JobStageTemplateId { get; set; }

        public int? StaffId { get; set; }
        public StaffReg Staff { get; set; }


        [ForeignKey("JobStageTemplateId")]
        public JobStageTemplate JobStageTemplate { get; set; }

        // Determines the order in which stages should be executed for the department
        public int Order { get; set; }
    }
}
