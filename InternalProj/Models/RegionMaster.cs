using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class RegionMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        // Foreign key
        [Required]
        public int StateId { get; set; }

        // Navigation property
        [ForeignKey("StateId")]
        public StateMaster State { get; set; }

    }
}
