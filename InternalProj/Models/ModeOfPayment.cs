using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class ModeOfPayment
    {
        [Key]
        public int ModeId { get; set; }

        [Required]
        public string ModeType { get; set; } 
    }

}
