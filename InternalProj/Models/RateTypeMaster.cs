using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{

    public class RateTypeMaster
    {
        [Key]
        public int RateTypeId { get; set; }
        public string RateTypeName { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }


        public ICollection<CustomerReg> CustomerRegs { get; set; }

    }
}
