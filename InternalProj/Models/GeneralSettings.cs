using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class GeneralSettings
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string SettingType { get; set; } // 1 = Pagination, 2 = EmailContent, 3 = SMSContent
        [Required]
        public string? Value { get; set; }
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

    }
}