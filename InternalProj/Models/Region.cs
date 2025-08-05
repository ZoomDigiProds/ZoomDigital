using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    [Table("Region")]
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // ✅ Primary Key

        public string? RegionName { get; set; }
    }
}
