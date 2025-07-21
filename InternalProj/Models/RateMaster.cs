using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class RateMaster
    {
        [Key]
        public int RateId { get; set; }
        public decimal Rate { get; set; }   
        
        // Foreign key to Size
        public int SizeId { get; set; }

        // Foreign key to Subhead
        public int SubHeadId { get; set; }

        // Foreign key to MainHeadReg
        public int MainHeadId { get; set; }
        public string? Details { get; set; }



        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
            
        [ForeignKey("SizeId")]
        public virtual AlbumSizeDetails AlbumSize { get; set; }

        [ForeignKey("SubHeadId")]
        public virtual SubHeadDetails SubHead { get; set; }

        // Navigation Properties
        [ForeignKey("MainHeadId")]
        public virtual MainHeadReg MainHead { get; set; }
    }
}
