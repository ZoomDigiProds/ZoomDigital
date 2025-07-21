using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class MainHeadReg
    {
        [Key]
        public int MainHeadId { get; set; }

        [Required]
        [StringLength(255)]
        public string MainHeadName { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        // Navigation Property: MainHead can have many SubHeads (One-to-Many relationship)
        public virtual ICollection<SubHeadDetails> SubHeads { get; set; }
    }

}
