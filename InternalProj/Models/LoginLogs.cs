using System;
using System.ComponentModel.DataAnnotations;

namespace InternalProj.Models
{
    public class LoginLogs
    {
        [Key]
        public int Id { get; set; }
        public string? StaffName { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public string? Local_IPAddres { get; set; } = null!;
        public string? Public_IPAddress { get; set; }
        public string? Location { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? Reason { get; set; }
    }
}
