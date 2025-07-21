using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class DeliveryMode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
