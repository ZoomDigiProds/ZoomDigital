using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class DeliveryMaster
    {
        [Key]
        public int DeliveryId { get; set; }

        [Required]
        [StringLength(255)]
        public string DeliveryName { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }
    }

}
