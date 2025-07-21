using InternalProj.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InternalProj.ViewModel
{
    public class CustomerRegViewModel
    {
        public IEnumerable<CustomerReg> CustomerRegs { get; set; } = new List<CustomerReg>();
        public IEnumerable<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
        public IEnumerable<StateMaster> StateMasterRegs { get; set; } = new List<StateMaster>();
        public IEnumerable<RegionMaster> RegionMasterRegs { get; set; } = new List<RegionMaster>();
        public IEnumerable<CustomerContact> CustomerContacts { get; set; } = new List<CustomerContact>();
        public IEnumerable<PhoneType> PhoneTypes { get; set; } = new List<PhoneType>();
        public IEnumerable<CustomerCategory> CustomerCategories { get; set; } = new List<CustomerCategory>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
        public IEnumerable<RateTypeMaster> RateTypes { get; set; } = new List<RateTypeMaster>();
        public IEnumerable<StaffReg> StaffRegs { get; set; } = new List<StaffReg>();

        // Form-bound properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Studio { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public decimal? Discount { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string Email { get; set; }
        public string? Whatsapp { get; set; }
        public int StateId { get; set; }
        public int RegionId { get; set; }
        public int PhoneTypeId { get; set; }
        public int CustomerCategoryId { get; set; }
        public int BranchId { get; set; }
        public int RateTypeId { get; set; }
        public int StaffId { get; set; }
        public int CategoryId { get; set; }

        [BindNever]
        public string? Message { get; set; }
    }
}
