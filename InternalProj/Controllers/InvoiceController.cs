//using InternalProj.Data;
//using InternalProj.Filters;
//using InternalProj.Models;
//using InternalProj.ViewModel;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace InternalProj.Controllers
//{
//    [DepartmentAuthorize("ADMIN")]
//    public class InvoiceController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        public InvoiceController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult CreateInvoice()
//        {
//            int nextInvoiceId = (_context.Invoices.OrderByDescending(i => i.InvoiceId).FirstOrDefault()?.InvoiceId ?? 0) + 1;

//            var model = new InvoiceViewModel
//            {
//                Invoice = new Invoice
//                {
//                    InvoiceId = nextInvoiceId,
//                    BillDate = DateTime.Now
//                },
//                Customers = _context.CustomerRegs.ToList(),
//                PaymentModes = _context.ModeOfPayments.ToList(),
//                WorkOrders = _context.WorkOrders.ToList(),
//            };

//            return View(model);
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult CreateInvoice(InvoiceViewModel model)
//        {

//            foreach (var entry in ModelState)
//            {
//                //Console.WriteLine($"{entry.Key}: {(entry.Value.Errors.Count == 0 ? "Valid" : "Invalid")}");
//                foreach (var error in entry.Value.Errors)
//                {
//                    Console.WriteLine($"  Error: {error.ErrorMessage}");
//                }
//            }

//            //Console.WriteLine("Submitted ModeId: " + model.Invoice.ModeId);
//            var mode = _context.ModeOfPayments.FirstOrDefault(m => m.ModeId == model.Invoice.ModeId);
//            if (mode == null)
//            {
//                TempData["ErrorMessage"] = "Invalid Payment Mode selected.";
//                model.Customers = _context.CustomerRegs.ToList();
//                model.PaymentModes = _context.ModeOfPayments.ToList();
//                return View(model);
//            }

//            if (!ModelState.IsValid)
//            {                
//                model.Customers = _context.CustomerRegs.ToList();
//                model.PaymentModes = _context.ModeOfPayments.ToList();
//                TempData["ErrorMessage"] = "Please correct the errors and try again.";
//                return View(model);
//            }

//            var workOrder = _context.WorkOrders.FirstOrDefault(w => w.WorkOrderId == model.Invoice.WorkOrderId);
//            if (workOrder == null)
//            {
//                TempData["ErrorMessage"] = "Invalid Work Order selected.";
//                model.Customers = _context.CustomerRegs.ToList();
//                model.PaymentModes = _context.ModeOfPayments.ToList();
//                return View(model);
//            }

//            try
//            {
//                decimal balance = (decimal)(workOrder.Balance ?? 0);

//                decimal discountPct = model.Invoice.Discount ?? 0;
//                decimal taxPct = model.Invoice.Tax ?? 0;
//                decimal cessPct = model.Invoice.Cess ?? 0;
//                decimal commissionPct = model.Invoice.Commission ?? 0;

//                decimal discountAmt = (discountPct / 100) * balance;
//                decimal taxAmt = (taxPct / 100) * balance;
//                decimal cessAmt = (cessPct / 100) * balance;
//                decimal commissionAmt = (commissionPct / 100) * balance;

//                decimal netAmount = (balance - discountAmt) + taxAmt + cessAmt + commissionAmt;

//                var invoice = new Invoice
//                {
//                    BillDate = DateTime.UtcNow,
//                    CustomerId = model.Invoice.CustomerId,
//                    WorkOrderId = model.Invoice.WorkOrderId,
//                    ModeId = model.Invoice.ModeId,
//                    Discount = discountPct,
//                    Tax = taxPct,
//                    Cess = cessPct,
//                    Commission = commissionPct,
//                    NetAmount = netAmount
//                };

//                _context.Invoices.Add(invoice);

//                workOrder.Balance = 0;
//                _context.WorkOrders.Update(workOrder);

//                _context.SaveChanges();

//                TempData["SuccessMessage"] = "Invoice created successfully!";
//                return RedirectToAction("CreateInvoice");
//            }
//            catch (Exception ex)
//            {
//                TempData["ErrorMessage"] = "Error while saving invoice: " + ex.Message;
//                model.Customers = _context.CustomerRegs.ToList();
//                model.PaymentModes = _context.ModeOfPayments.ToList();
//                return View(model);
//            }
//        }


//        [HttpGet]
//        public IActionResult GetCustomerDetails(int customerId)
//        {
//            var customer = _context.CustomerRegs
//                .Include(c => c.Address)
//                .Include(c => c.Contacts)
//                .FirstOrDefault(c => c.Id == customerId);

//            var allWorkOrders = _context.WorkOrders
//                .Where(w => w.CustomerId == customerId)
//                .OrderByDescending(w => w.WorkOrderId)
//                .Select(w => new {w.WorkOrderId, w.WorkOrderNo})
//                .ToList();

//            var latestWorkOrders = allWorkOrders.Take(3).ToList();

//            var total = _context.WorkOrders
//                .Where(w => w.CustomerId == customerId)
//                .Sum(w => w.SubTotal);

//            var advance = _context.WorkOrders
//                .Where(w => w.CustomerId == customerId)
//                .Sum(w => w.Advance ?? 0);

//            return Json(new
//            {
//                studio = customer.StudioName,
//                customerName = $"{customer.FirstName} {customer.LastName}",
//                address = customer.Address?.Address1,
//                mobile = customer.Contacts.FirstOrDefault()?.Phone1,
//                latestWorkOrders,
//                allWorkOrders,
//                outstanding = total - advance
//            });
//        }

//        [HttpGet]
//        public IActionResult GetWorkOrderDetails(int workOrderId)
//        {
//            var details = _context.WorkDetails
//                .Include(w => w.SubHead)
//                .Include(w => w.Size)
//                .Include(w => w.ChildSubHead)
//                .Where(w => w.WorkOrderId == workOrderId)
//                .Select(w => new WorkDetailDTO
//                {
//                    Particulars = $"{w.SubHead.SubHeadName} - {w.Size.Size} - {w.ChildSubHead.ChildSubHeadName ?? ""}",
//                    Qty = w.Qty,
//                    Rate = w.Rate,
//                    GTotal = w.GTotal
//                }).ToList();

//            return Json(details);
//        }
//        [HttpGet]
//        public IActionResult GetFinancials(int workOrderId)
//        {
//            var workOrder = _context.WorkOrders
//                .Where(w => w.WorkOrderId == workOrderId)
//                .Select(w => new
//                {
//                    w.Advance,
//                    w.Balance
//                })
//                .FirstOrDefault();

//            if (workOrder == null)
//                return NotFound();

//            return Json(new
//            {
//                advance = workOrder.Advance ?? 0,
//                balance = workOrder.Balance ?? 0
//            });
//        }
//    }
//}



using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize("ADMIN")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateInvoice()
        {
            int nextInvoiceId = (_context.Invoices.OrderByDescending(i => i.InvoiceId).FirstOrDefault()?.InvoiceId ?? 0) + 1;

            var model = new InvoiceViewModel
            {
                Invoice = new Invoice
                {
                    InvoiceId = nextInvoiceId,
                    BillDate = DateTime.Now
                },
                Customers = _context.CustomerRegs.ToList(),
                PaymentModes = _context.ModeOfPayments.ToList(),
                WorkOrders = _context.WorkOrders.ToList(),
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateInvoice(InvoiceViewModel model)
        {

            foreach (var entry in ModelState)
            {
                //Console.WriteLine($"{entry.Key}: {(entry.Value.Errors.Count == 0 ? "Valid" : "Invalid")}");
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"  Error: {error.ErrorMessage}");
                }
            }

            //Console.WriteLine("Submitted ModeId: " + model.Invoice.ModeId);
            var mode = _context.ModeOfPayments.FirstOrDefault(m => m.ModeId == model.Invoice.ModeId);
            if (mode == null)
            {
                TempData["ErrorMessage"] = "Invalid Payment Mode selected.";
                model.Customers = _context.CustomerRegs.ToList();
                model.PaymentModes = _context.ModeOfPayments.ToList();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.Customers = _context.CustomerRegs.ToList();
                model.PaymentModes = _context.ModeOfPayments.ToList();
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(model);
            }

            var workOrder = _context.WorkOrders.FirstOrDefault(w => w.WorkOrderId == model.Invoice.WorkOrderId);
            if (workOrder == null)
            {
                TempData["ErrorMessage"] = "Invalid Work Order selected.";
                model.Customers = _context.CustomerRegs.ToList();
                model.PaymentModes = _context.ModeOfPayments.ToList();
                return View(model);
            }

            try
            {
                decimal balance = (decimal)(workOrder.Balance ?? 0);

                if (balance != 0)
                {
                    TempData["ErrorMessage"] = "Invoice can only be generated once the balance is fully cleared.";
                    model.Customers = _context.CustomerRegs.ToList();
                    model.PaymentModes = _context.ModeOfPayments.ToList();
                    return View(model);
                }

                decimal discountPct = model.Invoice.Discount ?? 0;
                decimal taxPct = model.Invoice.Tax ?? 0;
                decimal cessPct = model.Invoice.Cess ?? 0;
                decimal commissionPct = model.Invoice.Commission ?? 0;

                decimal discountAmt = (discountPct / 100) * balance;
                decimal taxAmt = (taxPct / 100) * balance;
                decimal cessAmt = (cessPct / 100) * balance;
                decimal commissionAmt = (commissionPct / 100) * balance;

                decimal netAmount = (balance - discountAmt) + taxAmt + cessAmt + commissionAmt;

                var invoice = new Invoice
                {
                    BillDate = DateTime.UtcNow,
                    CustomerId = model.Invoice.CustomerId,
                    WorkOrderId = model.Invoice.WorkOrderId,
                    ModeId = model.Invoice.ModeId,
                    Discount = discountPct,
                    Tax = taxPct,
                    Cess = cessPct,
                    Commission = commissionPct,
                    NetAmount = netAmount
                };

                _context.Invoices.Add(invoice);

                //workOrder.Balance = 0;
                //_context.WorkOrders.Update(workOrder);

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Invoice created successfully!";
                return RedirectToAction("CreateInvoice");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error while saving invoice: " + ex.Message;
                model.Customers = _context.CustomerRegs.ToList();
                model.PaymentModes = _context.ModeOfPayments.ToList();
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult GetCustomerDetails(int customerId)
        {
            var customer = _context.CustomerRegs
                .Include(c => c.Address)
                .Include(c => c.Contacts)
                .FirstOrDefault(c => c.Id == customerId);

            var allWorkOrders = _context.WorkOrders
                //.Where(w => w.CustomerId == customerId)
                .Where(w => w.CustomerId == customerId
                    && !_context.Invoices.Any(i => i.WorkOrderId == w.WorkOrderId))
                .OrderByDescending(w => w.WorkOrderId)
                .Select(w => new { w.WorkOrderId, w.WorkOrderNo })
                .ToList();

            var latestWorkOrders = allWorkOrders.Take(3).ToList();

            var total = _context.WorkOrders
                .Where(w => w.CustomerId == customerId)
                .Sum(w => w.SubTotal);

            var advance = _context.WorkOrders
                .Where(w => w.CustomerId == customerId)
                .Sum(w => w.Advance ?? 0);

            var outstanding = _context.WorkOrders
                .Where(w => w.CustomerId == customerId)
                .Sum(w => w.Balance ?? 0);

            return Json(new
            {
                studio = customer.StudioName,
                customerName = $"{customer.FirstName} {customer.LastName}",
                address = customer.Address?.Address1,
                mobile = customer.Contacts.FirstOrDefault()?.Phone1,
                latestWorkOrders,
                allWorkOrders,
                outstanding
            });
        }

        [HttpGet]
        public IActionResult GetWorkOrderDetails(int workOrderId)
        {
            var details = _context.WorkDetails
                .Include(w => w.SubHead)
                .Include(w => w.Size)
                .Include(w => w.ChildSubHead)
                .Where(w => w.WorkOrderId == workOrderId)
                .Select(w => new WorkDetailDTO
                {
                    Particulars = $"{w.SubHead.SubHeadName} - {w.Size.Size} - {w.ChildSubHead.ChildSubHeadName ?? ""}",
                    Qty = w.Qty,
                    Rate = w.Rate,
                    GTotal = w.GTotal
                }).ToList();

            return Json(details);
        }
        [HttpGet]
        public IActionResult GetFinancials(int workOrderId)
        {
            var workOrder = _context.WorkOrders
                .Where(w => w.WorkOrderId == workOrderId)
                .Select(w => new
                {
                    w.Advance,
                    w.Balance
                })
                .FirstOrDefault();

            if (workOrder == null)
                return NotFound();

            return Json(new
            {
                advance = workOrder.Advance ?? 0,
                balance = workOrder.Balance ?? 0
            });
        }
    }
}