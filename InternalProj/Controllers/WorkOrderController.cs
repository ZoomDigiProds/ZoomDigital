using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace InternalProj.Controllers
{
    //[DepartmentAuthorize("ADMIN")]
    public class WorkOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderController(ApplicationDbContext context)
        {
            _context = context;
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
            //Console.WriteLine($"Received ID: {id}");
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

            //Console.WriteLine($"WorkDetailsList count: {model.WorkDetailsList?.Count ?? 0}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                model.WorkOrder.Active = "Y";
                model.WorkOrder.Wdate = DateTime.UtcNow;

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

                // Save WorkOrder
                _context.WorkOrders.Add(model.WorkOrder);
                await _context.SaveChangesAsync(); 

                if (model.WorkDetailsList == null || !model.WorkDetailsList.Any())
                {
                    ModelState.AddModelError("", "Please add at least one work detail entry.");
                    PopulateDropdowns(model);
                    return View(model);
                }

                double subTotal = 0;

                foreach (var detail in model.WorkDetailsList)
                {
                    if (detail.Qty <= 0)
                    {
                        ModelState.AddModelError("", "Quantity must be greater than 0.");
                        PopulateDropdowns(model);
                        return View(model);
                    }

                    var subHeadName = await _context.SubHeads
                        .Where(s => s.SubHeadId == detail.SubheadId)
                        .Select(s => s.SubHeadName)
                        .FirstOrDefaultAsync();

                    var sizeName = await _context.Albums
                        .Where(s => s.SizeId == detail.SizeId)
                        .Select(s => s.Size)
                        .FirstOrDefaultAsync();

                    var mainHead = await _context.MainHeads
                        .FirstOrDefaultAsync(m => m.MainHeadId == detail.MainHeadId);

                    if (mainHead == null)
                    {
                        ModelState.AddModelError("", $"Invalid MainHeadId: {detail.MainHeadId}. It does not exist.");
                        PopulateDropdowns(model);
                        return View(model);
                    }


                    if (detail.ChildSubheadId != null && detail.ChildSubheadId != 0)
                    {
                        var childExists = await _context.ChildSubHeads
                            .AnyAsync(c => c.ChildSubHeadId == detail.ChildSubheadId);

                        if (!childExists)
                        {
                            Console.WriteLine($"Warning: Invalid ChildSubheadId {detail.ChildSubheadId}, setting to null.");
                            detail.ChildSubheadId = null;
                        }
                    }
                    else
                    {
                        detail.ChildSubheadId = null;
                    }

                    if (detail.Rate > 0.00)
                    {
                        var existingRate = await _context.RateMasters
                            .FirstOrDefaultAsync(r => r.SubHeadId == detail.SubheadId && r.SizeId == detail.SizeId && r.Active == "Y");

                        if (existingRate == null)
                        {
                            existingRate = new RateMaster
                            {
                                SubHeadId = detail.SubheadId,
                                SizeId = detail.SizeId,
                                MainHeadId = detail.MainHeadId,
                                Rate = Math.Round((decimal)detail.Rate, 2),
                                Active = "Y",
                                Details = $"{subHeadName} - {sizeName}"
                            };

                            _context.RateMasters.Add(existingRate);

                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"RateMaster save error: {ex.Message}");
                                throw;
                            }
                        }
                        else if (existingRate.Rate != (decimal)detail.Rate)
                        {
                            existingRate.Rate = Math.Round((decimal)detail.Rate, 2);
                            existingRate.Details = $"{subHeadName} - {sizeName}";
                            _context.RateMasters.Update(existingRate);
                            await _context.SaveChangesAsync();
                        }

                        detail.Rate = (double)existingRate.Rate;
                    }

                    Console.WriteLine($"subHeadName: {subHeadName}");
                    Console.WriteLine($"sizeName: {sizeName}");

                    detail.WorkOrderId = model.WorkOrder.WorkOrderId;
                    detail.GTotal = detail.Qty * detail.Rate;
                    detail.Tax ??= 0;
                    detail.Cess ??= 0;
                    detail.Active = "Y";

                    subTotal += detail.GTotal;

                    _context.WorkDetails.Add(detail);
                }

                model.WorkOrder.SubTotal = Math.Round(subTotal, 2);

                model.WorkOrder.Advance = model.Advance;
                model.WorkOrder.Balance = model.Advance.HasValue
                    ? model.SubTotal - model.Advance.Value
                    : model.SubTotal;

                _context.WorkOrders.Update(model.WorkOrder);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var phoneNumber = await _context.CustomerContacts
                .Where(c => c.CustomerId == model.WorkOrder.CustomerId)
                .Select(c => c.Phone1)
                .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    try
                    {
                        const string accountSid = "AC7052ba65c1bdf3346a3b4c3b10b8814b";
                        const string authToken = "c64da20074379fe9a886304a882bceec";
                        const string fromPhoneNumber = "+14849608592";

                        TwilioClient.Init(accountSid, authToken);

                        var message = $"Dear Customer, your Work Order #{model.WorkOrder.WorkOrderNo} has been successfully created.";

                        var sms = MessageResource.Create(
                            body: message,
                            from: new PhoneNumber(fromPhoneNumber),
                            to: new PhoneNumber(phoneNumber)
                        );

                        Console.WriteLine($"SMS sent to {phoneNumber}: SID = {sms.Sid}, Status = {sms.Status}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SMS error: {ex.Message}");
                    }
                }



                TempData["SuccessMessage"] = "Work Order created successfully.";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                //ModelState.AddModelError("", "An error occurred while saving the work order.");
                TempData["ErrorMessage"] = "An error occurred while saving the work order.";
                PopulateDropdowns(model);
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

            var details = _context.WorkDetails
                .Include(d => d.SubHead)
                .Include(d => d.Size)
                .Include(d => d.ChildSubHead)
                .Where(d => d.WorkOrderId == id)
                .ToList();

            var viewModel = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                WorkDetailsList = details
            };

            return PartialView("_WorkOrderDetailsPartial", viewModel);
        }

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


    }
}
