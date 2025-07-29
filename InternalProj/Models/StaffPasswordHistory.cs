using System;
using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class StaffPasswordHistory
    {
        [Key]
        public int Id { get; set; }

        public int StaffId { get; set; }

        [Required]
        public string HashedPassword { get; set; } = null!;

        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;
    }
}
