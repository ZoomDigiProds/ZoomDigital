using Humanizer;
using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Helpers;
using InternalProj.Models;
using InternalProj.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ZoomColorLab.Controllers
{
    //[DepartmentAuthorize()]
    public class StaffRegController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffRegController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StaffReg
        public async Task<IActionResult> Index()
        {
            var staffList = await _context.StaffRegs
                .Include(s => s.StaffDepartments)
                    .ThenInclude(sd => sd.Department)
                .Include(s => s.StaffDesignations)
                .Include(s => s.Addresses)
                .Include(s => s.Contacts)
                .Include(s => s.Credentials)
                .ToListAsync();

            var branches = await _context.Branches.Where(b => b.Active == "Y").ToListAsync();
            //var categories = await _context.CustomerCategories.Where(c => c.Active == "Y").ToListAsync();

            var model = staffList.Select(s => new StaffRegViewModel
            {
                StaffRegId = s.StaffId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                BranchId = s.BranchId,
                //CategoryId = s.CategoryId,
                Branches = branches,
                //CustomerCategories = categories,
                Phone1 = EncryptionHelper.Decrypt(s.Contacts?.FirstOrDefault()?.Phone1),
                Phone2 = EncryptionHelper.Decrypt(s.Contacts?.FirstOrDefault()?.Phone2),
                Whatsapp = EncryptionHelper.Decrypt(s.Contacts?.FirstOrDefault()?.Whatsapp),
                Email = s.Contacts?.FirstOrDefault()?.Email,
                UserName = s.Credentials?.FirstOrDefault()?.UserName,
                Active = s.Active,
                Departments = s.StaffDepartments.Select(sd => sd.Department).ToList()

            }).ToList();

            return View(model);
        }

        // GET: StaffReg/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new StaffRegViewModel
            {
                Departments = await _context.DeptMasters.Where(d => d.Active == "Y").ToListAsync(),
                Designations = await _context.DesignationMasters.Where(d => d.Active == "Y").ToListAsync(),
                Branches = await _context.Branches.Where(b => b.Active == "Y").ToListAsync(),
                PhoneTypes = await _context.PhoneTypes.Where(p => p.Active == "Y").ToListAsync(),
                //CustomerCategories = await _context.CustomerCategories.Where(p => p.Active == "Y").ToListAsync()
            };

            return View(model);
        }

        // POST: StaffReg/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            StaffRegViewModel model,
            string FirstName, string LastName, string Address1, string? Address2,
            int BranchId,
            string Phone1, string? Phone2, string? Whatsapp, string Email,
            int PhoneTypeId,
            //int CategoryId,
            DateTimeOffset? DOB, DateTimeOffset? DOJ,
            //string Remarks,
            string UserName, string Password)
        {

            if (!ModelState.IsValid)
            {
                PrintModelErrors();
                await ReloadDropdowns(model);
                return View(model);
            }

            if (await _context.StaffCredentials.AnyAsync(u => u.UserName.ToLower() == UserName.ToLower()))
            {
                TempData["ErrorMessage"] = "Username already exists.";
                await ReloadDropdowns(model);
                return View(model);
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(UserName, @"^[a-zA-Z0-9_]{4,20}$"))
            {
                TempData["ErrorMessage"] = "Username must be 3-20 characters and contain only letters, numbers, and underscores.";
                await ReloadDropdowns(model);
                return View(model);
            }
            if (await _context.StaffContacts.AnyAsync(c => c.Email.ToLower() == Email.ToLower()))
            {
                TempData["ErrorMessage"] = "Email already exists.";
                await ReloadDropdowns(model);
                return View(model);
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                TempData["ErrorMessage"] = "Invalid email format.";
                await ReloadDropdowns(model);
                return View(model);
            }


            try
            {
                var newStaff = new StaffReg
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    DOB = DOB?.ToUniversalTime(),
                    DOJ = (DOJ ?? DateTimeOffset.UtcNow).ToUniversalTime(),
                    CreatedDate = DateTimeOffset.UtcNow,
                    Active = "Y",
                    BranchId = BranchId,
                    //CategoryId = CategoryId,
                    //Remarks = Remarks
                };

                await _context.StaffRegs.AddAsync(newStaff);
                await _context.SaveChangesAsync();

                var staffAddress = new StaffAddress
                {
                    StaffId = newStaff.StaffId,
                    Address1 = Address1,
                    Address2 = Address2,
                    Active = "Y"
                };
                await _context.StaffAddresses.AddAsync(staffAddress);

                var staffContact = new StaffContact
                {
                    StaffId = newStaff.StaffId,
                    Phone1 = EncryptionHelper.Encrypt(Phone1),
                    Phone2 = EncryptionHelper.Encrypt(Phone2),
                    Whatsapp = EncryptionHelper.Encrypt(Whatsapp),
                    Email = Email,
                    PhoneTypeId = PhoneTypeId,
                    Active = "Y"
                };
                await _context.StaffContacts.AddAsync(staffContact);

                var staffCredentials = new StaffCredentials
                {
                    StaffId = newStaff.StaffId,
                    UserName = UserName,
                    Status = 1,
                    Active = "Y",
                    IsFirstLogin = true
                };
                var passwordHasher = new PasswordHasher<StaffCredentials>();
                staffCredentials.Password = passwordHasher.HashPassword(staffCredentials, Password);
                await _context.StaffCredentials.AddAsync(staffCredentials);

                if (model.SelectedDeptIds?.Any() == true)
                {
                    foreach (var deptId in model.SelectedDeptIds)
                    {
                        await _context.StaffDepartments.AddAsync(new StaffDepartment
                        {
                            StaffId = newStaff.StaffId,
                            DeptId = deptId
                        });
                    }
                }

                if (model.SelectedDesignationIds?.Any() == true)
                {
                    foreach (var desigId in model.SelectedDesignationIds)
                    {
                        await _context.StaffDesignations.AddAsync(new StaffDesignation
                        {
                            StaffId = newStaff.StaffId,
                            DesignationId = desigId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Staff registration successful!";
                return RedirectToAction("Login", "Account");

            }
            catch (Exception ex)
            {
                var fullError = GetFullExceptionMessage(ex);

                //Console.WriteLine("===== STAFF REGISTRATION ERROR =====");
                //Console.WriteLine($"Timestamp: {DateTime.UtcNow}");
                //Console.WriteLine("Error Message:");
                //Console.WriteLine(fullError);
                //Console.WriteLine("Stack Trace:");
                //Console.WriteLine(ex.StackTrace);
                //Console.WriteLine("====================================");

                ModelState.AddModelError("", "An error occurred: " + fullError);
                TempData["ErrorMessage"] = "An error occureed while registering staff";
                await ReloadDropdowns(model);
                return View(model);
            }

        }

        // GET: StaffReg/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.StaffRegs
                .Include(s => s.Addresses)
                .Include(s => s.Contacts)
                .Include(s => s.StaffDepartments)
                .Include(s => s.StaffDesignations)
                .FirstOrDefaultAsync(s => s.StaffId == id);

            if (staff == null)
                return NotFound();

            var credentials = await _context.StaffCredentials.FirstOrDefaultAsync(c => c.StaffId == id);

            var model = new StaffRegViewModel
            {
                StaffRegId = staff.StaffId,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                DOB = staff.DOB,
                DOJ = staff.DOJ,
                BranchId = staff.BranchId,
                //CategoryId = staff.CategoryId,
                //Remarks = staff.Remarks,
                UserName = credentials?.UserName,
                Address1 = staff.Addresses?.FirstOrDefault()?.Address1,
                Address2 = staff.Addresses?.FirstOrDefault()?.Address2,
                Phone1 = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Phone1),
                Phone2 = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Phone2),
                Whatsapp = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Whatsapp),
                Email = staff.Contacts?.FirstOrDefault()?.Email,
                PhoneTypeId = staff.Contacts?.FirstOrDefault()?.PhoneTypeId ?? 0,
                SelectedDeptIds = staff.StaffDepartments.Select(sd => sd.DeptId).ToList(),
                SelectedDesignationIds = staff.StaffDesignations.Select(sd => sd.DesignationId).ToList(),
                Departments = await _context.DeptMasters.Where(d => d.Active == "Y").ToListAsync(),
                Designations = await _context.DesignationMasters.Where(d => d.Active == "Y").ToListAsync(),
                Branches = await _context.Branches.Where(b => b.Active == "Y").ToListAsync(),
                PhoneTypes = await _context.PhoneTypes.Where(p => p.Active == "Y").ToListAsync(),
                //CustomerCategories = await _context.CustomerCategories.Where(c => c.Active == "Y").ToListAsync()
            };

            return View(model);
        }

        // POST: StaffReg/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
                StaffRegViewModel model,
                string FirstName, string LastName, string Address1, string? Address2,
                int BranchId,
                string Phone1, string? Phone2, string? Whatsapp, string Email,
                int PhoneTypeId,
                //int CategoryId,
                //string Remarks,
                DateTimeOffset? DOB, DateTimeOffset? DOJ,
                string Active,
                 string UserName)
        {
            if (!ModelState.IsValid)
            {
                PrintModelErrors();
                await ReloadDropdowns(model);
                return View(model);
            }

            // ✅ Uniqueness checks
            var usernameToCheck = UserName?.Trim().ToLower();

            var existingUser = await _context.StaffCredentials
                .FirstOrDefaultAsync(c => c.UserName.ToLower() == usernameToCheck && c.StaffId != model.StaffRegId);

            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
            }

            var emailToCheck = Email?.Trim().ToLower();

            var existingEmail = await _context.StaffContacts
                .FirstOrDefaultAsync(c => c.Email.ToLower() == emailToCheck && c.StaffId != model.StaffRegId);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadDropdowns(model);
                return View(model);
            }

            var staff = await _context.StaffRegs
                .Include(s => s.Addresses)
                .Include(s => s.Contacts)
                .Include(s => s.StaffDepartments)
                .Include(s => s.StaffDesignations)
                .FirstOrDefaultAsync(s => s.StaffId == model.StaffRegId);

            if (staff == null)
            {
                ModelState.AddModelError("", "Staff not found");
                await ReloadDropdowns(model);
                return View(model);
            }


            try
            {
                staff.FirstName = FirstName;
                staff.LastName = LastName;
                staff.DOB = DOB?.ToUniversalTime();
                staff.DOJ = DOJ?.ToUniversalTime() ?? staff.DOJ;
                staff.BranchId = BranchId;
                //staff.CategoryId = CategoryId;
                //staff.Remarks = Remarks;

                var address = staff.Addresses.FirstOrDefault();
                if (address == null)
                {
                    address = new StaffAddress
                    {
                        StaffId = staff.StaffId,
                        Address1 = Address1,
                        Address2 = Address2,
                        Active = "Y"
                    };
                    await _context.StaffAddresses.AddAsync(address);
                }
                else
                {
                    address.Address1 = Address1;
                    address.Address2 = Address2;
                    _context.StaffAddresses.Update(address);
                }

                var contact = staff.Contacts.FirstOrDefault();
                if (contact == null)
                {
                    contact = new StaffContact
                    {
                        StaffId = staff.StaffId,
                        Phone1 = Phone1,
                        Phone2 = Phone2,
                        Whatsapp = Whatsapp,
                        Email = Email,
                        PhoneTypeId = PhoneTypeId,
                        Active = "Y"
                    };
                    await _context.StaffContacts.AddAsync(contact);
                }
                else
                {
                    contact.Phone1 = Phone1;
                    contact.Phone2 = Phone2;
                    contact.Whatsapp = Whatsapp;
                    contact.Email = Email;
                    contact.PhoneTypeId = PhoneTypeId;
                    _context.StaffContacts.Update(contact);
                }

                var credentials = await _context.StaffCredentials.FirstOrDefaultAsync(c => c.StaffId == staff.StaffId);
                if (credentials != null)
                {
                    credentials.UserName = UserName;
                    //if (!string.IsNullOrEmpty(Password))
                    //{
                    //    var hasher = new PasswordHasher<StaffCredentials>();
                    //    credentials.Password = hasher.HashPassword(credentials, Password);
                    //}
                    _context.StaffCredentials.Update(credentials);
                }

                _context.StaffDepartments.RemoveRange(staff.StaffDepartments);
                if (model.SelectedDeptIds?.Any() == true)
                {
                    foreach (var deptId in model.SelectedDeptIds)
                    {
                        await _context.StaffDepartments.AddAsync(new StaffDepartment
                        {
                            StaffId = staff.StaffId,
                            DeptId = deptId
                        });
                    }
                }

                _context.StaffDesignations.RemoveRange(staff.StaffDesignations);
                if (model.SelectedDesignationIds?.Any() == true)
                {
                    foreach (var desigId in model.SelectedDesignationIds)
                    {
                        await _context.StaffDesignations.AddAsync(new StaffDesignation
                        {
                            StaffId = staff.StaffId,
                            DesignationId = desigId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Staff details updated successfully!";

                if (HttpContext.Session.GetInt32("StaffId") == staff.StaffId)
                {
                    await SessionHelper.SetStaffSessionAsync(HttpContext, _context, staff.StaffId);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + GetFullExceptionMessage(ex));
                await ReloadDropdowns(model);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var staff = await _context.StaffRegs
                .Include(s => s.Addresses)
                .Include(s => s.Contacts)
                .Include(s => s.Credentials)
                .FirstOrDefaultAsync(s => s.StaffId == id);

            if (staff == null)
            {
                return NotFound();
            }

            if (staff.StaffDepartments.Any(sd => sd.Department.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = "Admin status cannot be changed.";
                return RedirectToAction("Index");
            }

            staff.Active = status;
            _context.StaffRegs.Update(staff);

            if (staff.Addresses != null && staff.Addresses.Any())
            {
                foreach (var address in staff.Addresses)
                {
                    address.Active = status;
                    _context.StaffAddresses.Update(address);
                }
            }

            if (staff.Contacts != null && staff.Contacts.Any())
            {
                foreach (var contact in staff.Contacts)
                {
                    contact.Active = status;
                    _context.StaffContacts.Update(contact);
                }
            }

            if (staff.Credentials != null && staff.Credentials.Any())
            {
                foreach (var cred in staff.Credentials)
                {
                    cred.Active = status;
                    _context.StaffCredentials.Update(cred);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Staff and related records status updated to {(status == "Y" ? "Active" : "Inactive")}";
            return RedirectToAction("Index");
        }


        // GET: StaffReg/Details/5
        //    [HttpGet]
        //    public async Task<IActionResult> Details(int id)
        //    {
        //        var staff = await _context.StaffRegs
        //            .Include(s => s.Addresses)
        //            .Include(s => s.Contacts)
        //            .Include(s => s.StaffDepartments)
        //            .Include(s => s.StaffDesignations)
        //            .FirstOrDefaultAsync(s => s.StaffId == id);

        //        if (staff == null)
        //            return NotFound();

        //        var credentials = await _context.StaffCredentials.FirstOrDefaultAsync(c => c.StaffId == id);

        //        var model = new StaffRegViewModel
        //        {
        //            StaffRegId = staff.StaffId,
        //            FirstName = staff.FirstName,
        //            LastName = staff.LastName,
        //            DOB = staff.DOB,
        //            DOJ = staff.DOJ,
        //            BranchId = staff.BranchId,
        //            //CategoryId = staff.CategoryId,
        //            //Remarks = staff.Remarks,
        //            UserName = credentials?.UserName,
        //            Address1 = staff.Addresses?.FirstOrDefault()?.Address1,
        //            Address2 = staff.Addresses?.FirstOrDefault()?.Address2,
        //            Phone1 = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Phone1),
        //            Phone2 = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Phone2),
        //            Whatsapp = EncryptionHelper.Decrypt(staff.Contacts?.FirstOrDefault()?.Whatsapp),
        //            Email = staff.Contacts?.FirstOrDefault()?.Email,
        //            PhoneTypeId = staff.Contacts?.FirstOrDefault()?.PhoneTypeId ?? 0,
        //            SelectedDeptIds = staff.StaffDepartments.Select(sd => sd.DeptId).ToList(),
        //            SelectedDesignationIds = staff.StaffDesignations.Select(sd => sd.DesignationId).ToList(),
        //            Departments = await _context.DeptMasters.Where(d => d.Active == "Y").ToListAsync(),
        //            Designations = await _context.DesignationMasters.Where(d => d.Active == "Y").ToListAsync(),
        //            Branches = await _context.Branches.Where(b => b.Active == "Y").ToListAsync(),
        //            PhoneTypes = await _context.PhoneTypes.Where(p => p.Active == "Y").ToListAsync(),
        //            //CustomerCategories = await _context.CustomerCategories.Where(c => c.Active == "Y").ToListAsync()
        //        };

        //        return View(model);
        //    }

        private async Task ReloadDropdowns(StaffRegViewModel model)
        {
            model.Departments = await _context.DeptMasters.Where(d => d.Active == "Y").ToListAsync();
            model.Designations = await _context.DesignationMasters.Where(d => d.Active == "Y").ToListAsync();
            model.Branches = await _context.Branches.Where(b => b.Active == "Y").ToListAsync();
            model.PhoneTypes = await _context.PhoneTypes.Where(p => p.Active == "Y").ToListAsync();
            //model.CustomerCategories = await _context.CustomerCategories.Where(p => p.Active == "Y").ToListAsync();
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            var messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            return string.Join(" --> ", messages);
        }

        private void PrintModelErrors()
        {
            foreach (var kvp in ModelState)
            {
                if (kvp.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"[ModelState Errors] Key: {kvp.Key}");
                    foreach (var err in kvp.Value.Errors)
                    {
                        Console.WriteLine($" - ErrorMessage: {err.ErrorMessage}");
                        if (err.Exception != null)
                        {
                            Console.WriteLine($" - Exception: {err.Exception}");
                        }
                    }
                }
            }
        }
    }
}