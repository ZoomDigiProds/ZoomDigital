using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class SubHeadDetails
    {
        [Key]
        public int SubHeadId { get; set; }

        [Required]
        [StringLength(255)]
        public string SubHeadName { get; set; }

        // Foreign key to MainHeadReg
        public int MainHeadId { get; set; }

        // Foreign key to Machine
        public int MachineId { get; set; }

        public bool Status { get; set; }


        public string? Details { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        //public int SizeId { get; set; }
        //public virtual AlbumSizeDetails AlbumSize { get; set; }

        // Navigation Properties
        [ForeignKey("MainHeadId")]
        public virtual MainHeadReg MainHead { get; set; }

        [ForeignKey("MachineId")]
        public virtual Machine Machine { get; set; }

        // Collection of ChildSubHeads under this SubHead
        public virtual ICollection<ChildSubHead> ChildSubHeads { get; set; }
    }
}
