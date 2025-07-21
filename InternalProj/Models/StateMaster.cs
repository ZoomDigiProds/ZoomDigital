using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace InternalProj.Models
{
    public class StateMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        // Navigation property: One State has many Regions
        public ICollection<RegionMaster> Regions { get; set; }
    }
}
