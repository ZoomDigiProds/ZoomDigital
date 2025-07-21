
using System.ComponentModel.DataAnnotations;
using InternalProj.Models;


namespace InternalProj.Models
{
    public class DeptMaster
    {
        [Key]
        public int DeptId { get; set; }
        public string? Name { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }


        public ICollection<StaffReg>? Staffs { get; set; }
    }

}
