// Models/Dictionary.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    [Table("Dictionary")]
    public class Dictionary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // This holds the table name, e.g., "State", "Region", etc.
    }
}
