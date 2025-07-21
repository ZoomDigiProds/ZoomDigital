using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class ChildSubHead
    {
        [Key]
      
        public int ChildSubHeadId { get; set; }
        public string ChildSubHeadName { get; set; }
        public int SubHeadId { get; set; }

        [ForeignKey("SubHeadId")]
        public SubHeadDetails SubHead { get; set; }

        // Optional: You can store additional information related to the parent subhead

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

    }

}
