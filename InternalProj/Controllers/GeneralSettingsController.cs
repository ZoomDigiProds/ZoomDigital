using InternalProj.Data;
using InternalProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalProj.Controllers
{
    public class GeneralSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeneralSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index - Display settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.GeneralSettings.ToListAsync();
            return View(settings);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _context.GeneralSettings.FindAsync(id);
            if (setting == null)
            {
                return NotFound();
            }
            return View(setting);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GeneralSettings updatedSetting)
        {
            if (ModelState.IsValid)
            {
                if (id == 0) // Create new setting
                {
                    updatedSetting.CreateDate = DateTime.UtcNow;
                    _context.GeneralSettings.Add(updatedSetting);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Existing record edit logic
                var existingSetting = await _context.GeneralSettings.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
                if (existingSetting == null)
                    return NotFound();

                string oldValue = existingSetting.Value ?? "";
                string newValue = updatedSetting.Value ?? "";

                updatedSetting.UpdatedDate = DateTime.UtcNow;
                _context.Update(updatedSetting);
                await _context.SaveChangesAsync();

                if (oldValue != newValue)
                {
                    var staffIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                    int.TryParse(staffIdString, out int staffId);

                    var auditLog = new GeneralSettingsAuditLog
                    {
                        SettingType = existingSetting?.SettingType ?? "Unknown",
                        OldValue = oldValue ?? string.Empty,
                        NewValue = newValue ?? string.Empty,
                        StaffId = staffId,
                        StaffName = User.Identity?.Name ?? "System",
                        ModifiedDate = DateTime.UtcNow,
                        Path = HttpContext.Request.Path + HttpContext.Request.QueryString
                    };

                    _context.GeneralSettingsAuditLog.Add(auditLog);
                    await _context.SaveChangesAsync();
                }




                return RedirectToAction(nameof(Index));
            }

            return View(updatedSetting);
        }
    }
}
