using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class WorkType
    {
        public int WorkTypeId { get; set; }

        [Required]
        [StringLength(255)]
        public string TypeName { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
