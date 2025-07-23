using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using InternalProj.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize()]
    public class CustomerRegController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerRegController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var customers = _context.CustomerRegs
                .Include(c => c.CustomerCategory)
                .Include(c => c.Branch)
                .Include(c => c.RateTypeMaster)
                .Include(c => c.StaffReg)
                .Include(c => c.Address)
                .Include(c => c.Contacts)
                .Where(c => c.Active == "Y")
                .ToList();

            var addresses = _context.CustomerAddresses
                .Include(a => a.State)
                .Include(a => a.Region)
                .Where(a => a.Active == "Y")
                .ToList();

            var contacts = _context.CustomerContacts
                .Include(c => c.PhoneType)
                .Where(c => c.Active == "Y")
                .ToList();

            var viewModel = new CustomerRegViewModel
            {
                CustomerRegs = customers,
                CustomerAddresses = addresses,
                CustomerContacts = contacts
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CustomerRegViewModel
            {
                StateMasterRegs = _context.StateMasters.Where(s => s.Active == "Y").ToList(),
                RegionMasterRegs = _context.RegionMasters.Where(r => r.Active == "Y").ToList(),
                PhoneTypes = _context.PhoneTypes.Where(p => p.Active == "Y").ToList(),
                CustomerCategories = _context.CustomerCategories.Where(p => p.Active == "Y").ToList(),
                Branches = _context.Branches.Where(c => c.Active == "Y").ToList(),
                RateTypes = _context.RateTypeMasters.Where(d => d.Active == "Y").ToList(),
                StaffRegs = _context.StaffRegs.Where(e => e.Active == "Y").ToList(),
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerRegViewModel model)
        {
            if (ModelState.IsValid)
            {
                var staffId = HttpContext.Session.GetInt32("StaffId");
                var staffName = HttpContext.Session.GetString("UserName");

                if (staffId == null || string.IsNullOrEmpty(staffName))
                {
                    TempData["ErrorMessage"] = "Staff session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var newCustomer = new CustomerReg
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StudioName = model.Studio,
                    Discount = model.Discount ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    Active = "Y",
                    CategoryId = model.CategoryId,
                    BranchId = model.BranchId,
                    RateTypeId = model.RateTypeId,
                    StaffId = staffId.Value
                };

                _context.CustomerRegs.Add(newCustomer);
                _context.SaveChanges();

                _context.CustomerAddresses.Add(new CustomerAddress
                {
                    CustomerId = newCustomer.Id,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    StateId = model.StateId,
                    RegionId = model.RegionId,
                    Active = "Y"
                });

                _context.CustomerContacts.Add(new CustomerContact
                {
                    CustomerId = newCustomer.Id,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Whatsapp = model.Whatsapp,
                    Email = model.Email,
                    PhoneTypeId = model.PhoneTypeId,
                    Active = "Y"
                });

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Customer registration successful!";
                return RedirectToAction("Index", "StaffReg");
            }
            else
            {
                Console.WriteLine("Model is NOT valid");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"Key: {error.Key} - Error: {err.ErrorMessage}");
                    }
                    TempData["ErrorMessage"] = "An error occured while registering customer. Please check all fields";
                    return View(model);
                }
            }

            // If validation fails, reload dropdowns
            model.StateMasterRegs = _context.StateMasters.Where(s => s.Active == "Y").ToList();
            model.RegionMasterRegs = _context.RegionMasters.Where(r => r.Active == "Y").ToList();
            model.PhoneTypes = _context.PhoneTypes.Where(p => p.Active == "Y").ToList();
            model.CustomerCategories = _context.CustomerCategories.Where(p => p.Active == "Y").ToList();
            model.Branches = _context.Branches.Where(q => q.Active == "Y").ToList();
            model.RateTypes = _context.RateTypeMasters.Where(e => e.Active == "Y").ToList();
            model.StaffRegs = _context.StaffRegs.Where(r => r.Active == "Y").ToList();

            TempData["ErrorMessage"] = "An error occurred while registering customer. Please check all fields.";
            return View(model);
        }

        [HttpGet]
        public IActionResult GetRegionsByState(int stateId)
        {
            var regions = _context.RegionMasters
                .Where(r => r.StateId == stateId && r.Active == "Y")
                .Select(r => new { id = r.Id, name = r.Name })
                .ToList();

            return Json(regions);
        }
    }
}