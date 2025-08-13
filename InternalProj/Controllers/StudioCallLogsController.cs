using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternalProj.Data;
using InternalProj.Models;

namespace InternalProj.Controllers
{
    public class StudioCallLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public StudioCallLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = PageSize;

            var today = DateTime.Today; // Local date
            var now = DateTime.Now;

            // Fetch all records
            var query = _context.StudioCallLogs.AsQueryable();

            // Ordering logic:
            var sortedQuery = query
                .OrderByDescending(c =>
                    c.UpdatedCallTime.HasValue &&
                    c.UpdatedCallTime.Value.ToLocalTime().Date == today)
                .ThenByDescending(c => c.UpdatedCallTime)
                .ThenByDescending(c => c.CallTime);

            int totalItems = await sortedQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedData = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedData);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var studioCallLog = await _context.StudioCallLogs
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.CallId == id);

            if (studioCallLog == null) return NotFound();

            return View(studioCallLog);
        }

        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CallTime,Description,UpdatedCallTime,Active,StudioName,Phone,Address1,Address2,State,Region")] StudioCallLog studioCallLog)
        {
            if (ModelState.IsValid)
            {
                var customer = await _context.CustomerRegs
                    .Include(c => c.Contacts)
                    .Include(c => c.Address).ThenInclude(a => a.State)
                    .Include(c => c.Address).ThenInclude(a => a.Region)
                    .FirstOrDefaultAsync(c => c.Id == studioCallLog.CustomerId);

                if (customer == null)
                {
                    ModelState.AddModelError("", "Invalid Customer.");
                }
                else
                {
                    studioCallLog.StudioName = customer.StudioName;
                    studioCallLog.Phone = customer.Contacts.FirstOrDefault()?.Phone1 ?? "";
                    studioCallLog.Region = customer.Address?.Region?.Name ?? "";

                    // Convert to UTC before saving
                    studioCallLog.CallTime = DateTime.SpecifyKind(studioCallLog.CallTime, DateTimeKind.Local).ToUniversalTime();
                    studioCallLog.UpdatedCallTime = studioCallLog.UpdatedCallTime.HasValue
                        ? DateTime.SpecifyKind(studioCallLog.UpdatedCallTime.Value, DateTimeKind.Local).ToUniversalTime()
                        : (DateTime?)null;

                    try
                    {
                        _context.StudioCallLogs.Add(studioCallLog);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error saving: " + ex.Message);
                        ModelState.AddModelError("", "Error saving: " + ex.Message);
                    }
                }
            }

            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName", studioCallLog.CustomerId);
            return View(studioCallLog);
        }

        // GET: StudioCallLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var studioCallLog = await _context.StudioCallLogs.FindAsync(id);
            if (studioCallLog == null) return NotFound();

            // Convert UTC to local time for display
            studioCallLog.CallTime = DateTime.SpecifyKind(studioCallLog.CallTime, DateTimeKind.Utc).ToLocalTime();

            if (studioCallLog.UpdatedCallTime.HasValue)
            {
                studioCallLog.UpdatedCallTime = DateTime.SpecifyKind(
                    studioCallLog.UpdatedCallTime.Value,
                    DateTimeKind.Utc
                ).ToLocalTime();
            }

            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName", studioCallLog.CustomerId);
            return View(studioCallLog);
        }

        // POST: StudioCallLogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var existingLog = await _context.StudioCallLogs.FindAsync(id);
            if (existingLog == null)
                return NotFound();

            if (await TryUpdateModelAsync(
                existingLog,
                "",
                s => s.CallTime,
                s => s.Description,
                s => s.UpdatedCallTime
            ))
            {
                // Convert LocalTime (from form) to UTC before saving
                existingLog.CallTime = DateTime.SpecifyKind(existingLog.CallTime, DateTimeKind.Local).ToUniversalTime();

                if (existingLog.UpdatedCallTime.HasValue)
                {
                    existingLog.UpdatedCallTime = DateTime.SpecifyKind(
                        existingLog.UpdatedCallTime.Value,
                        DateTimeKind.Local
                    ).ToUniversalTime();
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudioCallLogExists(id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName", existingLog.CustomerId);
            return View(existingLog);
        }

        // GET: StudioCallLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var studioCallLog = await _context.StudioCallLogs
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.CallId == id);

            if (studioCallLog == null) return NotFound();

            return View(studioCallLog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studioCallLog = await _context.StudioCallLogs.FindAsync(id);
            if (studioCallLog != null)
            {
                _context.StudioCallLogs.Remove(studioCallLog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDetails(int id)
        {
            var customer = await _context.CustomerRegs
                .Include(c => c.Contacts)
                .Include(c => c.Address).ThenInclude(a => a.State)
                .Include(c => c.Address).ThenInclude(a => a.Region)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return NotFound();

            var result = new
            {
                studioName = customer.StudioName,
                phone = customer.Contacts.FirstOrDefault()?.Phone1 ?? "",
                region = customer.Address?.Region?.Name ?? ""
            };

            return Json(result);
        }
        private bool StudioCallLogExists(int id)
        {
            return _context.StudioCallLogs.Any(e => e.CallId == id);
        }
    }
}