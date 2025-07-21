using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class AlbumSizeDetails
    {
        [Key]
        public int SizeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Size { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
        public virtual ICollection<SubHeadDetails> SubHeads { get; set; }  // SubHeads associated with this size

    }

}
