using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class Machine
    {
        [Key]
        public int MachineId { get; set; }

        [Required]
        [StringLength(255)]
        public string MachineName { get; set; }
                
        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
