using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InternalProj.Models;

namespace InternalProj.Models
{
    public class StaffRegViewModel
    {
        public int StaffRegId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset? DOB { get; set; }
        public DateTimeOffset? DOJ { get; set; }
        public int BranchId { get; set; }
        //public int CategoryId { get; set; }
        //public string Remarks { get; set; }
        public string UserName { get; set; }

        public string Address1 { get; set; }
        public string? Address2 { get; set; }

        public string Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Whatsapp { get; set; }
        public string? Email { get; set; }
        public int PhoneTypeId { get; set; }
        public string? Active { get; set; }
        public IEnumerable<DeptMaster> Departments { get; set; } = new List<DeptMaster>();
        public IEnumerable<DesignationMaster> Designations { get; set; } = new List<DesignationMaster>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
        public IEnumerable<PhoneType> PhoneTypes { get; set; } = new List<PhoneType>();
        //public IEnumerable<CustomerCategory> CustomerCategories { get; set; } = new List<CustomerCategory>();

        [Display(Name = "Departments")]
        public List<int> SelectedDeptIds { get; set; } = new();

        [Display(Name = "Designations")]
        public List<int> SelectedDesignationIds { get; set; } = new();

        // You can keep these if you want but not required for edit
        public IEnumerable<StaffReg> StaffRegs { get; set; } = new List<StaffReg>();
        public IEnumerable<StaffAddress> StaffAddresses { get; set; } = new List<StaffAddress>();
        public IEnumerable<StaffContact> StaffContacts { get; set; } = new List<StaffContact>();
        public IEnumerable<StaffCredentials> StaffCredentials { get; set; } = new List<StaffCredentials>();
    }
}
