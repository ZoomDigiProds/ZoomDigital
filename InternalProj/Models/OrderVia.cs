using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class OrderVia
    {
        [Key]
        public int OrderViaId { get; set; }

        [Required]
        [StringLength(255)]
        public string OrderViaCategory { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
