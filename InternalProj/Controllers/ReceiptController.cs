//using InternalProj.Data;
//using InternalProj.Filters;
//using InternalProj.Models;
//using InternalProj.ViewModel;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;


//[DepartmentAuthorize()]

//public class ReceiptController : Controller
//{
//    private readonly ApplicationDbContext _context;  

//    public ReceiptController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    [HttpGet]
//    public IActionResult Create(int? customerId, int currentPage = 1)
//    {
//        int pageSize = 10;

//        var allCustomers = _context.CustomerRegs.ToList();
//        var allModes = _context.ModeOfPayments.ToList();

//        List<WorkOrderSummaryDto> pagedOrders = new();
//        double totalBalance = 0;
//        int totalItems = 0;

//        if (customerId.HasValue)
//        {
//            var allFiltered = _context.WorkOrders
//                .Where(w => w.CustomerId == customerId && w.Active == "Y")
//                .Select(w => new WorkOrderSummaryDto
//                {
//                    WorkOrderId = w.WorkOrderId,
//                    WorkOrderNo = w.WorkOrderNo,
//                    Wdate = w.Wdate,
//                    SubTotal = w.SubTotal,
//                    Advance = w.Advance ?? 0,
//                    TotalPaid = _context.Receipts
//                        .Where(r => r.WorkOrderId == w.WorkOrderId)
//                        .Sum(r => (double?)r.CurrentAmount) ?? 0
//                })
//                .AsEnumerable()
//                .Select(w =>
//                {
//                    w.Balance = Math.Max(0, w.SubTotal - w.Advance - w.TotalPaid);
//                    return w;
//                })
//                .Where(w => w.Balance > 0)
//                .OrderByDescending(w => w.Wdate)
//                .ToList();

//            totalBalance = allFiltered.Sum(w => w.Balance);
//            totalItems = allFiltered.Count;
//            pagedOrders = allFiltered.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
//        }

//        var nextReceiptId = _context.Receipts.Any() ? _context.Receipts.Max(r => r.ReceiptId) + 1 : 1;

//        var vm = new ReceiptViewModel
//        {
//            Receipt = new Receipt
//            {
//                CustomerId = customerId ?? 0,
//                ReceiptDate = DateTime.UtcNow
//            },
//            Customers = allCustomers,
//            ModeOfPayments = allModes,
//            ReceiptNo = nextReceiptId,
//            PagedWorkOrders = pagedOrders,
//            CurrentPage = currentPage,
//            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
//            TotalBalance = totalBalance
//        };

//        return View(vm);
//    }

//    [HttpPost]
//    public IActionResult Create(ReceiptViewModel model)
//    {
//        if (!ModelState.IsValid)
//        {
//            TempData["Error"] = "Form data is invalid.";
//            model.ModeOfPayments = _context.ModeOfPayments.ToList();
//            model.Customers = _context.CustomerRegs.ToList();
//            return View(model);
//        }

//        if (!ModelState.IsValid || model.Receipt.CustomerId == 0 || model.Receipt.WorkOrderId == 0 || model.Receipt.CurrentAmount == 0)
//        {
//            TempData["Error"] = "Please select a valid customer and work order before submitting.";

//            model.ModeOfPayments = _context.ModeOfPayments.ToList();
//            model.Customers = _context.CustomerRegs.ToList();

//            // Populate additional view model data
//            var workOrders = _context.WorkOrders
//                .Where(w => w.CustomerId == model.Receipt.CustomerId && w.Active == "Y")
//                .Select(w => new WorkOrderSummaryDto
//                {
//                    WorkOrderId = w.WorkOrderId,
//                    WorkOrderNo = w.WorkOrderNo,
//                    Wdate = w.Wdate,
//                    SubTotal = w.SubTotal,
//                    Advance = w.Advance ?? 0,
//                    TotalPaid = _context.Receipts
//                        .Where(r => r.WorkOrderId == w.WorkOrderId)
//                        .Sum(r => (double?)r.CurrentAmount) ?? 0
//                })
//                .AsEnumerable()
//                .Select(w =>
//                {
//                    w.Balance = Math.Max(0, w.SubTotal - w.Advance - w.TotalPaid);
//                    return w;
//                })
//                .Where(w => w.Balance > 0)
//                .OrderByDescending(w => w.Wdate)
//                .ToList();

//            int pageSize = 10;
//            int currentPage = model.CurrentPage > 0 ? model.CurrentPage : 1;
//            var pagedOrders = workOrders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
//            double totalBalance = workOrders.Sum(w => w.Balance);
//            int totalItems = workOrders.Count;

//            model.ReceiptNo = _context.Receipts.Any() ? _context.Receipts.Max(r => r.ReceiptId) + 1 : 1;
//            model.PagedWorkOrders = pagedOrders;
//            model.CurrentPage = currentPage;
//            model.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
//            model.TotalBalance = totalBalance;

//            return View(model);
//        }

//        try
//        {
//            var workOrder = _context.WorkOrders
//                .FirstOrDefault(w => w.WorkOrderId == model.Receipt.WorkOrderId);

//            if (workOrder == null)
//            {
//                TempData["Error"] = "Invalid work order.";
//                model.ModeOfPayments = _context.ModeOfPayments.ToList();
//                model.Customers = _context.CustomerRegs.ToList();
//                return View(model);
//            }

//            var totalReceipts = _context.Receipts
//                .Where(r => r.WorkOrderId == model.Receipt.WorkOrderId)
//                .Sum(r => (double?)r.CurrentAmount) ?? 0;

//            var availableBalance = workOrder.SubTotal - (workOrder.Advance ?? 0) - totalReceipts;

//            if (model.Receipt.CurrentAmount > availableBalance)
//            {
//                TempData["Error"] = $"The amount exceeds the remaining balance of ₹{availableBalance:N2}.";
//                model.ModeOfPayments = _context.ModeOfPayments.ToList();
//                model.Customers = _context.CustomerRegs.ToList();
//                return View(model);
//            }

//            var receipt = new Receipt
//            {
//                ReceiptDate = DateTime.UtcNow,
//                ModeId = model.Receipt.ModeId,
//                CustomerId = model.Receipt.CustomerId,
//                WorkOrderId = model.Receipt.WorkOrderId,
//                NetAmount = model.Receipt.NetAmount,
//                CurrentAmount = model.Receipt.CurrentAmount
//            };

//            _context.Receipts.Add(receipt);
//            _context.SaveChanges();

//            workOrder.Balance = availableBalance - model.Receipt.CurrentAmount;
//            if (workOrder.Balance < 0)
//                workOrder.Balance = 0;

//            _context.Update(workOrder);
//            _context.SaveChanges();

//            TempData["Success"] = "Receipt saved successfully.";
//            return RedirectToAction("Create", new { customerId = model.Receipt.CustomerId });
//        }
//        catch (Exception ex)
//        {
//            TempData["Error"] = "Error: " + ex.Message;
//            model.ModeOfPayments = _context.ModeOfPayments.ToList();
//            model.Customers = _context.CustomerRegs.ToList();
//            return View(model);
//        }
//    }
//}



using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace InternalProj.Controllers
{
    [DepartmentAuthorize()]
    public class ReceiptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int? customerId, int currentPage = 1)
        {
            int pageSize = 10;

            var allCustomers = _context.CustomerRegs.ToList();
            var allModes = _context.ModeOfPayments.ToList();

            List<WorkOrderSummaryDto> pagedOrders = new();
            double totalBalance = 0;
            int totalItems = 0;

            if (customerId.HasValue)
            {
                var allFiltered = _context.WorkOrders
                    .Where(w => w.CustomerId == customerId && w.Active == "Y")
                    .Select(w => new WorkOrderSummaryDto
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        Wdate = w.Wdate,
                        SubTotal = w.SubTotal,
                        Advance = w.Advance ?? 0,
                        TotalPaid = _context.Receipts
                            .Where(r => r.WorkOrderId == w.WorkOrderId)
                            .Sum(r => (double?)r.CurrentAmount) ?? 0
                    })
                    .AsEnumerable()
                    .Select(w =>
                    {
                        w.Balance = Math.Max(0, w.SubTotal - w.Advance - w.TotalPaid);
                        return w;
                    })
                    .Where(w => w.Balance > 0)
                    .OrderByDescending(w => w.Wdate)
                    .ToList();

                totalBalance = allFiltered.Sum(w => w.Balance);
                totalItems = allFiltered.Count;
                pagedOrders = allFiltered.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }

            var nextReceiptId = _context.Receipts.Any() ? _context.Receipts.Max(r => r.ReceiptId) + 1 : 1;

            var vm = new ReceiptViewModel
            {
                Receipt = new Receipt
                {
                    CustomerId = customerId ?? 0,
                    ReceiptDate = DateTime.UtcNow
                },
                Customers = allCustomers,
                ModeOfPayments = allModes,
                ReceiptNo = nextReceiptId,
                PagedWorkOrders = pagedOrders,
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalBalance = totalBalance
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(ReceiptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form data is invalid.";
                //model.ModeOfPayments = _context.ModeOfPayments.ToList();
                //model.Customers = _context.CustomerRegs.ToList();
                PopulateViewModel(model);
                return View(model);
            }

            if (!ModelState.IsValid || model.Receipt.CustomerId == 0 || model.Receipt.WorkOrderId == 0 || model.Receipt.CurrentAmount == 0)
            {
                TempData["Error"] = "Please select a valid customer and work order before submitting.";

                model.ModeOfPayments = _context.ModeOfPayments.ToList();
                model.Customers = _context.CustomerRegs.ToList();

                // Populate additional view model data
                var workOrders = _context.WorkOrders
                    .Where(w => w.CustomerId == model.Receipt.CustomerId && w.Active == "Y")
                    .Select(w => new WorkOrderSummaryDto
                    {
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNo = w.WorkOrderNo,
                        Wdate = w.Wdate,
                        SubTotal = w.SubTotal,
                        Advance = w.Advance ?? 0,
                        TotalPaid = _context.Receipts
                            .Where(r => r.WorkOrderId == w.WorkOrderId)
                            .Sum(r => (double?)r.CurrentAmount) ?? 0
                    })
                    .AsEnumerable()
                    .Select(w =>
                    {
                        w.Balance = Math.Max(0, w.SubTotal - w.Advance - w.TotalPaid);
                        return w;
                    })
                    .Where(w => w.Balance > 0)
                    .OrderByDescending(w => w.Wdate)
                    .ToList();

                int pageSize = 10;
                int currentPage = model.CurrentPage > 0 ? model.CurrentPage : 1;
                var pagedOrders = workOrders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                double totalBalance = workOrders.Sum(w => w.Balance);
                int totalItems = workOrders.Count;

                model.ReceiptNo = _context.Receipts.Any() ? _context.Receipts.Max(r => r.ReceiptId) + 1 : 1;
                model.PagedWorkOrders = pagedOrders;
                model.CurrentPage = currentPage;
                model.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                model.TotalBalance = totalBalance;

                return View(model);
            }

            try
            {
                var workOrder = _context.WorkOrders
                    .FirstOrDefault(w => w.WorkOrderId == model.Receipt.WorkOrderId);

                if (workOrder == null)
                {
                    TempData["Error"] = "Invalid work order.";
                    model.ModeOfPayments = _context.ModeOfPayments.ToList();
                    model.Customers = _context.CustomerRegs.ToList();
                    return View(model);
                }

                var totalReceipts = _context.Receipts
                    .Where(r => r.WorkOrderId == model.Receipt.WorkOrderId)
                    .Sum(r => (double?)r.CurrentAmount) ?? 0;

                var availableBalance = workOrder.SubTotal - (workOrder.Advance ?? 0) - totalReceipts;

                if (model.Receipt.CurrentAmount > availableBalance)
                {
                    TempData["Error"] = $"The amount exceeds the remaining balance of ₹{availableBalance:N2}.";
                    //model.ModeOfPayments = _context.ModeOfPayments.ToList();
                    //model.Customers = _context.CustomerRegs.ToList();
                    PopulateViewModel(model);
                    return View(model);
                }


                var receipt = new Receipt
                {
                    ReceiptDate = DateTime.UtcNow,
                    ModeId = model.Receipt.ModeId,
                    CustomerId = model.Receipt.CustomerId,
                    WorkOrderId = model.Receipt.WorkOrderId,
                    NetAmount = model.Receipt.NetAmount,
                    CurrentAmount = model.Receipt.CurrentAmount
                };

                _context.Receipts.Add(receipt);
                _context.SaveChanges();

                workOrder.Balance = availableBalance - model.Receipt.CurrentAmount;
                if (workOrder.Balance < 0)
                    workOrder.Balance = 0;

                _context.Update(workOrder);
                _context.SaveChanges();

                TempData["Success"] = "Receipt saved successfully.";
                return RedirectToAction("Create", new { customerId = model.Receipt.CustomerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                model.ModeOfPayments = _context.ModeOfPayments.ToList();
                model.Customers = _context.CustomerRegs.ToList();
                return View(model);
            }
        }

        private void PopulateViewModel(ReceiptViewModel model)
        {
            model.ModeOfPayments = _context.ModeOfPayments.ToList();
            model.Customers = _context.CustomerRegs.ToList();

            var workOrders = _context.WorkOrders
                .Where(w => w.CustomerId == model.Receipt.CustomerId && w.Active == "Y")
                .Select(w => new WorkOrderSummaryDto
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNo = w.WorkOrderNo,
                    Wdate = w.Wdate,
                    SubTotal = w.SubTotal,
                    Advance = w.Advance ?? 0,
                    TotalPaid = _context.Receipts
                        .Where(r => r.WorkOrderId == w.WorkOrderId)
                        .Sum(r => (double?)r.CurrentAmount) ?? 0
                })
                .AsEnumerable()
                .Select(w =>
                {
                    w.Balance = Math.Max(0, w.SubTotal - w.Advance - w.TotalPaid);
                    return w;
                })
                .Where(w => w.Balance > 0)
                .OrderByDescending(w => w.Wdate)
                .ToList();

            int pageSize = 10;
            int currentPage = model.CurrentPage > 0 ? model.CurrentPage : 1;

            model.PagedWorkOrders = workOrders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            model.TotalBalance = workOrders.Sum(w => w.Balance);
            model.TotalPages = (int)Math.Ceiling((double)workOrders.Count / pageSize);
            model.ReceiptNo = _context.Receipts.Any() ? _context.Receipts.Max(r => r.ReceiptId) + 1 : 1;
        }

    }

}