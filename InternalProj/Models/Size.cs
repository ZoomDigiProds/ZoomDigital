using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    [Table("Size")]
    public class Size
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Changed from lowercase `id` to `Id`

        public string? SizeName { get; set; }
    }
}
