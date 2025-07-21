using InternalProj.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System;

namespace InternalProj.ViewModel
{
    public class WorkOrderViewModel
    {
        // WorkOrderMaster specific fields
        public WorkOrderMaster WorkOrder { get; set; } = new WorkOrderMaster();
        public List<WorkDetails> WorkDetailsList { get; set; } = new List<WorkDetails>();

        // Collection properties for dropdowns and other relational data
        public IEnumerable<CustomerReg> Customers { get; set; } = new List<CustomerReg>();
        public IEnumerable<Machine> Machines { get; set; } = new List<Machine>();
        public IEnumerable<DeliveryMaster> DeliveryMasters { get; set; } = new List<DeliveryMaster>();
        public IEnumerable<DeliveryMode> DeliveryModes { get; set; } = new List<DeliveryMode>();
        public IEnumerable<OrderVia> OrderVias { get; set; } = new List<OrderVia>();
        public IEnumerable<WorkType> WorkTypes { get; set; } = new List<WorkType>();        
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
        public IEnumerable<StaffReg> StaffRegs { get; set; } = new List<StaffReg>();
        public IEnumerable<MainHeadReg> MainHeads { get; set; } = new List<MainHeadReg>();
        public IEnumerable<SubHeadDetails> SubHeads { get; set; } = new List<SubHeadDetails>();
        public IEnumerable<AlbumSizeDetails> Albums { get; set; } = new List<AlbumSizeDetails>();
        public IEnumerable<ChildSubHead> ChildSubHeads { get; set; } = new List<ChildSubHead>();
        public List<CombinedSubHead> CombinedSubHeadList { get; set; } = new List<CombinedSubHead>();
        [BindNever]
        public IEnumerable<RateMaster> RateMasterList { get; set; } = new List<RateMaster>();
        public List<WorkOrderMaster> Results { get; set; } = new List<WorkOrderMaster>();
        public List<string> StudioList { get; set; } = new();
        public string? StudioFilter { get; set; }
        public DateTime? FromDateFilter { get; set; }
        public DateTime? ToDateFilter { get; set; }
        public int? WorkTypeFilter { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public double SubTotal { get; set; }
        public double? Advance { get; set; }
        public double? Balance { get; set; }

        public DateTime? Wdate { get; set; }
        public DateTime? Ddate { get; set; }
    }
    public class CombinedSubHead
    {
        public string DisplayName { get; set; } 
        public double Rate { get; set; }
    }

}
