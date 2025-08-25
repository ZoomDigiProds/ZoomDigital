using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class GeneralSettingsAuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string SettingType { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        [Required]
        //public string? Path { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        [Required]
        public int StaffId { get; set; }

        [Required]
        public string? StaffName { get; set; }
    }
}