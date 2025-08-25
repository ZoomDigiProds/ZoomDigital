using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.ViewModels;
using Microsoft.AspNetCore.Mvc;
using InternalProj.Models;
using System.Linq;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize()]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // ✅ Get Pagination value from GeneralSettings table
        public IActionResult Pagination()
        {
            var paginationSetting = _context.GeneralSettings
                .FirstOrDefault(s => s.SettingType == "Pagination");

            // fallback if no setting found
            var paginationValue = paginationSetting?.Value ?? "10";

            return PartialView("Pagination", paginationValue);
        }

        [HttpPost]
        public IActionResult UpdatePagination(int value)
        {
            try
            {
                var paginationSetting = _context.GeneralSettings
                    .FirstOrDefault(s => s.SettingType == "Pagination");

                string oldValue = paginationSetting?.Value;
                string newValue = value.ToString();

                if (paginationSetting != null)
                {
                    paginationSetting.Value = newValue;
                    paginationSetting.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.GeneralSettings.Add(new GeneralSettings
                    {
                        SettingType = "Pagination",
                        Value = newValue,
                        CreateDate = DateTime.UtcNow
                    });
                }

                // ✅ Add entry to audit log
                var staffId = HttpContext.Session.GetString("StaffId");
                int parsedStaffId = int.TryParse(staffId, out var sid) ? sid : 0;
                var staffName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                _context.GeneralSettingsAuditLog.Add(new GeneralSettingsAuditLog
                {
                    SettingType = "Pagination",
                    OldValue = oldValue,
                    NewValue = newValue,
                    ModifiedDate = DateTime.UtcNow,
                    StaffId = parsedStaffId,
                    StaffName = staffName
                });

                _context.SaveChanges();

                return Json(new { success = true, message = "Pagination updated successfully!" });
            }
            catch (Exception ex)
            {
                // ✅ Include inner exception if available
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Error updating pagination: " + errorMessage });
            }
        }

        //Scheduler view
        public IActionResult Scheduler() => PartialView("Scheduler");

        //Dictonaries views

        public IActionResult Dictionary()
        {
            var dbItems = _context.Dictionary.ToList();
            var model = new List<DictionaryTree>
            {
                new DictionaryTree
                {
                    Title = "Dictionary",
                    Children = dbItems.Select(d => new DictionaryItem
                    {
                        Name = d.Name,
                        Url = "#"
                    }).ToList()
                }
            };
            return PartialView("Dictionary", model);
        }

        // password policy
        public IActionResult PasswordPolicy() => PartialView("PasswordPolicy");

        // ✅ Email Content instead of SMS Content
        [HttpGet]
        public IActionResult EmailContent()
        {
            var emailSetting = _context.GeneralSettings
                .FirstOrDefault(s => s.SettingType == "EmailContent");

            string currentValue = emailSetting?.Value ?? string.Empty;

            return PartialView("EmailContent", currentValue);
        }

        [HttpPost]
        public IActionResult UpdateEmailContent(string content)
        {
            try
            {
                var emailSetting = _context.GeneralSettings
                    .FirstOrDefault(s => s.SettingType == "EmailContent");

                string oldValue = emailSetting?.Value;
                string newValue = content;

                if (emailSetting != null)
                {
                    emailSetting.Value = newValue;
                    emailSetting.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.GeneralSettings.Add(new GeneralSettings
                    {
                        SettingType = "EmailContent",
                        Value = newValue,
                        CreateDate = DateTime.UtcNow
                    });
                }

                // ✅ Add entry to audit log (consistent with Pagination logging)
                var staffIdString = HttpContext.Session.GetString("StaffId");
                int parsedStaffId = int.TryParse(staffIdString, out var sid) ? sid : 0;
                var staffName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                _context.GeneralSettingsAuditLog.Add(new GeneralSettingsAuditLog
                {
                    SettingType = "EmailContent",
                    OldValue = oldValue,
                    NewValue = newValue,
                    ModifiedDate = DateTime.UtcNow,
                    StaffId = parsedStaffId,
                    StaffName = staffName
                });


                _context.SaveChanges();

                return Json(new { success = true, message = "Email content updated successfully!" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Error updating Email content: " + errorMessage });
            }
        }

        //SMS Content

        [HttpGet]
        public IActionResult SMSContent()
        {
            var emailSetting = _context.GeneralSettings
                .FirstOrDefault(s => s.SettingType == "SMSContent");

            string currentValue = emailSetting?.Value ?? string.Empty;

            return PartialView("SMSContent", currentValue);
        }

        [HttpPost]
        public IActionResult UpdateSMSContent(string content)
        {
            try
            {
                var smsSetting = _context.GeneralSettings
                    .FirstOrDefault(s => s.SettingType == "SMSContent");

                string oldValue = smsSetting?.Value;
                string newValue = content;

                if (smsSetting != null)
                {
                    smsSetting.Value = newValue;
                    smsSetting.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.GeneralSettings.Add(new GeneralSettings
                    {
                        SettingType = "SMSContent",
                        Value = newValue,
                        CreateDate = DateTime.UtcNow
                    });
                }

                // ✅ Add entry to audit log
                var staffIdString = HttpContext.Session.GetString("StaffId");
                int parsedStaffId = int.TryParse(staffIdString, out var sid) ? sid : 0;
                var staffName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                _context.GeneralSettingsAuditLog.Add(new GeneralSettingsAuditLog
                {
                    SettingType = "SMSContent",
                    OldValue = oldValue,
                    NewValue = newValue,
                    ModifiedDate = DateTime.UtcNow,
                    StaffId = parsedStaffId,
                    StaffName = staffName
                });

                _context.SaveChanges();

                return Json(new { success = true, message = "SMS content updated successfully!" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Error updating SMS content: " + errorMessage });
            }
        }


        //States action
        public IActionResult GetStates()
        {
            var states = _context.StateMasters.Select(s => new
            {
                s.Id,
                s.Name,
                s.Active,
            }).ToList();

            return Json(states);
        }

        public IActionResult GetRegions()
        {
            var regions = _context.RegionMasters.Select(r => new
            {
                r.Id,
                r.Name,
                r.Active,
            }).ToList();

            return Json(regions);
        }

        public IActionResult GetSizes()
        {
            var sizes = _context.Albums.Select(s => new
            {
                s.SizeId,
                s.Size,
            }).ToList();

            return Json(sizes);
        }

        public IActionResult GeneralSettingsAuditLog()
        {
            var logs = _context.GeneralSettingsAuditLog
                .OrderByDescending(x => x.ModifiedDate)
                .ToList();

            return View(logs);
        }
        public IActionResult EmailContentAuditLog()
        {
            var logs = _context.GeneralSettingsAuditLog
                .Where(x => x.SettingType == "EmailContent")
                .OrderByDescending(x => x.ModifiedDate)
                .ToList();

            return View(logs);
        }
    }
}