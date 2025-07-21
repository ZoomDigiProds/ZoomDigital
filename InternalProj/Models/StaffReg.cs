using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class StaffReg
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset? DOB { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset? DOJ { get; set; }

        //public string? Remarks { get; set; }

        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string Active { get; set; }

        public int BranchId { get; set; }

        //public int CategoryId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        public ICollection<StaffDepartment> StaffDepartments { get; set; } = new List<StaffDepartment>();
        public ICollection<StaffDesignation> StaffDesignations { get; set; } = new List<StaffDesignation>();

        public ICollection<StaffAddress>? Addresses { get; set; }
        public ICollection<StaffContact>? Contacts { get; set; }
        public ICollection<StaffCredentials>? Credentials { get; set; }
        public ICollection<AuditLog>? AuditLogs { get; set; }
    }

    public class StaffDepartment
    {
        public int StaffId { get; set; }
        public StaffReg Staff { get; set; }

        public int DeptId { get; set; }
        public DeptMaster Department { get; set; }
    }

    public class StaffDesignation
    {
        public int StaffId { get; set; }
        public StaffReg Staff { get; set; }

        public int DesignationId { get; set; }
        public DesignationMaster Designation { get; set; }
    }
}
