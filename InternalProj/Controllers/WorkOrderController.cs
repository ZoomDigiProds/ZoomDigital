using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.Service;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace InternalProj.Controllers

{
    [DepartmentAuthorize()]
    public class WorkOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly WorkOrderService _workOrderService;

        public WorkOrderController(ApplicationDbContext context, WorkOrderService workOrderService)
        {
            _context = context;
            _workOrderService = workOrderService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new WorkOrderViewModel
            {
                WorkOrder = new WorkOrderMaster(),
                Machines = _context.Machines.Where(s => s.Active == "Y").ToList(),
                DeliveryMasters = _context.DeliveryMasters.Where(s => s.Active == "Y").ToList(),
                DeliveryModes = _context.DeliveryModes.Where(s => s.Active == "Y").ToList(),
                OrderVias = _context.OrderVias.Where(s => s.Active == "Y").ToList(),
                WorkTypes = _context.WorkTypes.Where(s => s.Active == "Y").ToList(),
                Customers = _context.CustomerRegs.Where(a => a.Active == "Y").ToList(),
                Branches = _context.Branches.Where(s => s.Active == "Y").ToList(),
                StaffRegs = _context.StaffRegs.Where(s => s.Active == "Y").ToList(),
                MainHeads = _context.MainHeads.Include(m => m.SubHeads)
                                              .Where(s => s.Active == "Y")
                                              .ToList(),
                SubHeads = _context.SubHeads.Where(s => s.Active == "Y").ToList(),
                Albums = _context.Albums.Where(s => s.Active == "Y").ToList(),
                ChildSubHeads = _context.ChildSubHeads.Where(s => s.Active == "Y").ToList(),
                RateMasterList = _context.RateMasters.Where(e => e.Active == "Y").ToList(),
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetCustomerDetailsById(int id)
        {
            Console.WriteLine($"Received ID: {id}");
            var customer = _context.CustomerRegs
                .Where(c => c.Id == id && c.Active == "Y")
                .Join(_context.CustomerContacts,
                      reg => reg.Id,
                      contact => contact.CustomerId,
                      (reg, contact) => new
                      {
                          FullName = reg.FirstName + " " + reg.LastName,
                          Mobile = contact.Phone1
                      })
                .FirstOrDefault();

            return Json(customer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"Key: {modelError.Key}, Error: {error.ErrorMessage}");
                    }
                }

                PopulateDropdowns(model);

                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var isEdit = model.WorkOrder.WorkOrderId > 0;
                double subTotal = 0;
                var totalPaid = _context.Receipts
                       .Where(r => r.WorkOrderId == model.WorkOrder.WorkOrderId)
                       .Sum(r => (double?)r.CurrentAmount) ?? 0;

                if (model.WorkOrder.Ddate.HasValue)
                {
                    if (model.WorkOrder.Ddate.Value < DateTime.UtcNow)
                    {
                        ModelState.AddModelError("WorkOrder.Ddate", "Delivery date cannot be in the past.");
                        PopulateDropdowns(model);
                        return View(model);
                    }

                    model.WorkOrder.Ddate = model.WorkOrder.Ddate.Value.ToUniversalTime();
                }

                List<WorkDetails> existingDetails = [];

                if (isEdit)
                {
                    var existingOrder = await _context.WorkOrders
                        .Include(w => w.WorkDetails)
                            .ThenInclude(d => d.SubHead)
                        .Include(w => w.WorkDetails)
                            .ThenInclude(d => d.Size)
                        .Include(w => w.WorkDetails)
                            .ThenInclude(d => d.ChildSubHead)
                        .Include(w => w.WorkDetails)
                            .ThenInclude(d => d.MainHead)
                        .FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrder.WorkOrderId);


                    if (existingOrder == null)
                    {
                        TempData["ErrorMessage"] = "Work order not found.";
                        return RedirectToAction("Create");
                    }

                    model.WorkOrder.WorkOrderNo = existingOrder.WorkOrderNo;

                    existingOrder.Ddate = model.WorkOrder.Ddate;
                    existingOrder.SubTotal = model.WorkOrder.SubTotal;
                    existingOrder.Balance = model.WorkOrder.Balance;
                    existingOrder.Advance = model.WorkOrder.Advance;



                    existingDetails = existingOrder.WorkDetails.ToList();

                    var deletedIds = Request.Form["DeletedDetailIds"]
                        .ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse).ToList();

                    var toDelete = existingOrder.WorkDetails.Where(d => deletedIds.Contains(d.WorkDetailsId)).ToList();
                    _context.WorkDetails.RemoveRange(toDelete);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // New work order
                    model.WorkOrder.Wdate = DateTime.UtcNow;
                    model.WorkOrder.Active = "Y";

                    //model.WorkOrder.SubTotal = Math.Round(subTotal, 2);

                    //if ((model.WorkOrder.Advance ?? 0) > model.WorkOrder.SubTotal)
                    //{
                    //    //ModelState.AddModelError("WorkOrder.Advance", "Advance cannot be greater than the SubTotal.");
                    //    TempData["ErrorMessage"] = $"Advance amount cannot exceed SubTotal.";
                    //    PopulateDropdowns(model);
                    //    return View(model);
                    //}

                    subTotal = 0;

                    if (model.WorkDetailsList != null)
                    {
                        foreach (var item in model.WorkDetailsList)
                        {
                            subTotal += item.Rate * item.Qty;
                        }
                    }

                    model.WorkOrder.SubTotal = Math.Round(subTotal, 2);

                    // Now validation works as expected
                    if ((model.WorkOrder.Advance ?? 0) > model.WorkOrder.SubTotal)
                    {
                        TempData["ErrorMessage"] = $"Advance amount cannot exceed SubTotal.";
                        PopulateDropdowns(model);
                        return View(model);
                    }


                    model.WorkOrder.Balance = model.WorkOrder.SubTotal
                        - (model.WorkOrder.Advance ?? 0)
                        - totalPaid;

                    if (model.WorkOrder.Balance < 0)
                        model.WorkOrder.Balance = 0;



                    _context.WorkOrders.Add(model.WorkOrder);
                    await _context.SaveChangesAsync();
                }

                if (model.WorkDetailsList == null || !model.WorkDetailsList.Any())
                {
                    ModelState.AddModelError("", "Please add at least one work detail.");
                    PopulateDropdowns(model);
                    return View(model);
                }

                foreach (var detail in model.WorkDetailsList)
                {
                    if (detail.Qty <= 0)
                    {
                        ModelState.AddModelError("", "Quantity must be greater than 0.");
                        PopulateDropdowns(model);
                        return View(model);
                    }

                    // Normalize missing/null values
                    detail.ChildSubheadId = detail.ChildSubheadId == 0 ? null : detail.ChildSubheadId;
                    detail.Tax ??= 0;
                    detail.Cess ??= 0;
                    detail.Active = "Y";
                    detail.WorkOrderId = model.WorkOrder.WorkOrderId;
                    detail.GTotal = detail.Qty * detail.Rate;

                    //subTotal += detail.GTotal;

                    var subHeadName = await _context.SubHeads
                        .Where(s => s.SubHeadId == detail.SubheadId)
                        .Select(s => s.SubHeadName)
                        .FirstOrDefaultAsync();

                    var sizeName = await _context.Albums
                        .Where(s => s.SizeId == detail.SizeId)
                        .Select(s => s.Size)
                        .FirstOrDefaultAsync();

                    var childName = detail.ChildSubheadId.HasValue
                        ? await _context.ChildSubHeads
                            .Where(c => c.ChildSubHeadId == detail.ChildSubheadId)
                            .Select(c => c.ChildSubHeadName)
                            .FirstOrDefaultAsync()
                        : "";

                    detail.Details = $"{subHeadName} - {sizeName}" + (string.IsNullOrWhiteSpace(childName) ? "" : $" - {childName}");


                    if (detail.WorkDetailsId > 0)
                    {
                        var existing = existingDetails.FirstOrDefault(d => d.WorkDetailsId == detail.WorkDetailsId);
                        if (existing != null)
                        {
                            _context.Entry(existing).CurrentValues.SetValues(detail);
                        }
                    }
                    else
                    {
                        _context.WorkDetails.Add(detail);
                    }

                    // Optional: Auto-sync rate to RateMaster
                    if (detail.Rate > 0)
                    {
                        var existingRate = await _context.RateMasters
                            .FirstOrDefaultAsync(r => r.SubHeadId == detail.SubheadId && r.SizeId == detail.SizeId && r.Active == "Y");

                        if (existingRate == null)
                        {
                            _context.RateMasters.Add(new RateMaster
                            {
                                SubHeadId = detail.SubheadId,
                                SizeId = detail.SizeId,
                                MainHeadId = detail.MainHeadId,
                                Rate = (decimal)detail.Rate,
                                Active = "Y",
                                Details = $"{subHeadName} - {sizeName}"
                            });
                        }
                        else if (existingRate.Rate != (decimal)detail.Rate)
                        {
                            existingRate.Rate = (decimal)detail.Rate;
                            _context.RateMasters.Update(existingRate);
                        }
                    }
                }

                await _context.SaveChangesAsync();



                model.WorkOrder.SubTotal = Math.Round(subTotal, 2);

                if (isEdit)
                {
                    // Editing: consider receipts also
                    model.WorkOrder.Balance = model.WorkOrder.SubTotal
                        - (model.WorkOrder.Advance ?? 0)
                        - totalPaid;
                }
                else
                {
                    // Creating: consider only advance
                    model.WorkOrder.Balance = model.WorkOrder.SubTotal
                        - (model.WorkOrder.Advance ?? 0);
                }

                if (model.WorkOrder.Balance < 0)
                    model.WorkOrder.Balance = 0;


                await _context.SaveChangesAsync();

                await _workOrderService.UpdateWorkOrderBalance(model.WorkOrder.WorkOrderId);

                await transaction.CommitAsync();

                // SMS logic (if applicable)
                var phone = await _context.CustomerContacts
                    .Where(c => c.CustomerId == model.WorkOrder.CustomerId)
                    .Select(c => c.Phone1)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(phone))
                {
                    try
                    {
                        TwilioClient.Init("AC7052ba65c1bdf3346a3b4c3b10b8814b", "c64da20074379fe9a886304a882bceec");
                        var message = MessageResource.Create(
                            body: $"Dear Customer, your Work Order #{model.WorkOrder.WorkOrderNo} has been successfully {(isEdit ? "updated" : "created")}.",
                            from: new PhoneNumber("+14849608592"),
                            to: new PhoneNumber(phone)
                        );

                        Console.WriteLine($"SMS sent: {message.Sid}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SMS send failed: {ex.Message}");
                    }
                }

                TempData["SuccessMessage"] = $"Work Order {(isEdit ? "updated" : "created")} successfully.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { redirectUrl = Url.Action("Create") });
                }

                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Save error: {ex}");
                TempData["ErrorMessage"] = "An error occurred while saving the work order.";
                PopulateDropdowns(model);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Create", model);
                return View(model);
            }
        }

        private void PopulateDropdowns(WorkOrderViewModel model)
        {
            model.Machines = _context.Machines.Where(s => s.Active == "Y").ToList();
            model.DeliveryMasters = _context.DeliveryMasters.Where(s => s.Active == "Y").ToList();
            model.DeliveryModes = _context.DeliveryModes.Where(s => s.Active == "Y").ToList();
            model.OrderVias = _context.OrderVias.Where(s => s.Active == "Y").ToList();
            model.WorkTypes = _context.WorkTypes.Where(s => s.Active == "Y").ToList();
            model.Albums = _context.Albums.Where(s => s.Active == "Y").ToList();
            model.Customers = _context.CustomerRegs.Where(s => s.Active == "Y").ToList();
            model.Branches = _context.Branches.Where(s => s.Active == "Y").ToList();
            model.StaffRegs = _context.StaffRegs.Where(s => s.Active == "Y").ToList();
            model.MainHeads = _context.MainHeads.Where(s => s.Active == "Y").ToList();
            model.SubHeads = _context.SubHeads.Where(s => s.Active == "Y").ToList();
            model.RateMasterList = _context.RateMasters.Where(e => e.Active == "Y").ToList();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubHeadRate(int subHeadId, int sizeId, double newRate)
        {
            var rateMaster = await _context.RateMasters
                .FirstOrDefaultAsync(r => r.SubHeadId == subHeadId && r.SizeId == sizeId);

            if (rateMaster == null)
            {
                return NotFound(new { success = false, message = "Rate not found for SubHead and Size!" });
            }

            rateMaster.Rate = (decimal)newRate;

            try
            {
                _context.RateMasters.Update(rateMaster);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Rate updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetSubHeads(int mainHeadId)
        {
            var subHeads = _context.SubHeads
                                   .Where(s => s.MainHeadId == mainHeadId)
                                   .Select(s => new
                                   {
                                       subHeadId = s.SubHeadId,
                                       subHeadName = s.SubHeadName,
                                   })
                                   .ToList();

            return Json(subHeads);
        }

        [HttpGet]
        public IActionResult GetChildSubHeads(int subHeadId, int sizeId)
        {
            Console.WriteLine($"sizeId before query: {sizeId}");

            // Fetch the rate once based on subHeadId and sizeId
            var rate = _context.RateMasters
                .Where(r => r.SubHeadId == subHeadId && r.SizeId == sizeId && r.Active == "Y")
                .Select(r => r.Rate)
                .FirstOrDefault();

            Console.WriteLine($"Fetched Rate: {rate}");

            // Fetch child subheads based on subHeadId
            var childSubHeads = _context.ChildSubHeads
                .Where(c => c.SubHeadId == subHeadId)
                .Select(c => new
                {
                    childSubHeadId = c.ChildSubHeadId,
                    childSubHeadName = c.ChildSubHeadName,
                    // Fetch the rate from RateMasters table based on SubHeadId and SizeId
                    rate = _context.RateMasters
                        .Where(r => r.SubHeadId == subHeadId && r.SizeId == sizeId && r.Active == "Y")
                        .Select(r => r.Rate)
                        .FirstOrDefault(),
                    size = _context.Albums
                        .Where(s => s.SizeId == sizeId)
                        .Select(s => s.Size)
                        .FirstOrDefault()
                })
                .ToList();

            return Json(childSubHeads);
        }

        public IActionResult GetWorkOrderDetails(int id)
        {
            var workOrder = _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .FirstOrDefault(w => w.WorkOrderId == id);

            if (workOrder == null)
                return NotFound();

            var details = _context.WorkDetails
                .Include(d => d.SubHead)
                .Include(d => d.Size)
                .Include(d => d.ChildSubHead)
                .Where(d => d.WorkOrderId == id)
                .ToList();

            // Fetch partial payments with dates
            var partialPayments = _context.Receipts
                .Where(r => r.WorkOrderId == id)
                .OrderBy(r => r.ReceiptDate)
                .Select(r => new PartialPaymentDto
                {
                    ReceiptDate = r.ReceiptDate,
                    Amount = r.CurrentAmount
                })
                .ToList();

            // Calculate totals
            double totalPaid = partialPayments.Sum(p => p.Amount);
            double advance = workOrder.Advance ?? 0;
            double balance = Math.Max(0, workOrder.SubTotal - advance - totalPaid);

            var viewModel = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                WorkDetailsList = details,
                Advance = advance,
                Balance = balance,
                SubTotal = workOrder.SubTotal,
                PartialPayments = partialPayments 
            };

            return PartialView("_WorkOrderDetailsPartial", viewModel);
        }


        //public IActionResult GetWorkOrderDetails(int id)
        //{
        //    var workOrder = _context.WorkOrders
        //        .Include(w => w.Customer)
        //        .Include(w => w.WorkType)
        //        .FirstOrDefault(w => w.WorkOrderId == id);

        //    if (workOrder == null)
        //        return NotFound();

        //    var details = _context.WorkDetails
        //        .Include(d => d.SubHead)
        //        .Include(d => d.Size)
        //        .Include(d => d.ChildSubHead)
        //        .Where(d => d.WorkOrderId == id)
        //        .ToList();

        //    // Fetch total paid from Receipts
        //    double totalPaid = _context.Receipts
        //        .Where(r => r.WorkOrderId == id)
        //        .Sum(r => (double?)r.CurrentAmount) ?? 0;

        //    // Calculate balance
        //    double advance = workOrder.Advance ?? 0;
        //    double balance = Math.Max(0, workOrder.SubTotal - advance - totalPaid);

        //    var viewModel = new WorkOrderViewModel
        //    {
        //        WorkOrder = workOrder,
        //        WorkDetailsList = details,
        //        Advance = advance,
        //        Balance = balance,
        //        SubTotal = workOrder.SubTotal
        //    };

        //    return PartialView("_WorkOrderDetailsPartial", viewModel);
        //}

        [HttpGet]
        public IActionResult GetNextWorkOrderNo(int workTypeId)
        {
            var workType = _context.WorkTypes.FirstOrDefault(w => w.WorkTypeId == workTypeId);
            if (workType == null)
                return BadRequest("Invalid Work Type");

            var prefix = workType.TypeName.Substring(0, 1).ToUpper();

            var latestOrder = _context.WorkOrders
                .Where(w => w.WorkOrderNo.StartsWith(prefix + "-"))
                .OrderByDescending(w => w.WorkOrderId)
                .Select(w => w.WorkOrderNo)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(latestOrder))
            {
                var parts = latestOrder.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            string newOrderNo = $"{prefix}-{nextNumber:D3}";
            return Json(new { workOrderNo = newOrderNo });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var workOrder = _context.WorkOrders
                .AsNoTracking()
                .Include(w => w.Customer)
                .Include(w => w.WorkType)
                .FirstOrDefault(w => w.WorkOrderId == id);

            if (workOrder == null)
                return NotFound();

            var totalPaid = _context.Receipts
                .Where(r => r.WorkOrderId == id)
                .Sum(r => (double?)r.CurrentAmount) ?? 0;

            var details = _context.WorkDetails
                .Where(d => d.WorkOrderId == id)
                .ToList();

            var viewModel = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                WorkDetailsList = details,
                TotalPaid = totalPaid,
                Customers = _context.CustomerRegs.Where(s => s.Active == "Y").ToList(),
                WorkTypes = _context.WorkTypes.Where(s => s.Active == "Y").ToList(),
                Machines = _context.Machines.Where(s => s.Active == "Y").ToList(),
                DeliveryMasters = _context.DeliveryMasters.Where(s => s.Active == "Y").ToList(),
                DeliveryModes = _context.DeliveryModes.Where(s => s.Active == "Y").ToList(),
                OrderVias = _context.OrderVias.Where(s => s.Active == "Y").ToList(),
                Branches = _context.Branches.Where(s => s.Active == "Y").ToList(),
                StaffRegs = _context.StaffRegs.Where(s => s.Active == "Y").ToList(),
                MainHeads = _context.MainHeads.Where(s => s.Active == "Y").ToList(),
                SubHeads = _context.SubHeads.Where(s => s.Active == "Y").ToList(),
                Albums = _context.Albums.Where(s => s.Active == "Y").ToList(),
                ChildSubHeads = _context.ChildSubHeads.Where(s => s.Active == "Y").ToList(),
                RateMasterList = _context.RateMasters.Where(e => e.Active == "Y").ToList(),
                Advance = workOrder.Advance,
                SubTotal = workOrder.SubTotal,
                //Balance = Math.Max(0, workOrder.SubTotal - (workOrder.Advance ?? 0) - totalPaid)
                Balance = workOrder.Balance ?? 0

            };

            return View("Create", viewModel);
        }
    }
}
