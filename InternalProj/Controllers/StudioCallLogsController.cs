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

        // GET: StudioCallLogs with Pagination
        public async Task<IActionResult> Index(int page = 1)
        {
            var totalItems = await _context.StudioCallLogs.CountAsync();
            var logs = await _context.StudioCallLogs
                .Include(s => s.Customer)
                .OrderByDescending(s => s.CallTime)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(logs);
        }

        // GET: StudioCallLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var studioCallLog = await _context.StudioCallLogs
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.CallId == id);

            if (studioCallLog == null) return NotFound();

            return View(studioCallLog);
        }

        // GET: StudioCallLogs/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName");
            return View();
        }

        // POST: StudioCallLogs/Create
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
                    studioCallLog.CallTime = DateTime.UtcNow;
                    studioCallLog.UpdatedCallTime = studioCallLog.UpdatedCallTime?.ToUniversalTime();

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

            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName", studioCallLog.CustomerId);
            return View(studioCallLog);
        }

        // POST: StudioCallLogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CallId,CustomerId,StudioName,Phone,Address1,Address2,State,Region,CallTime,Description,UpdatedCallTime,Active")] StudioCallLog studioCallLog)
        {
            if (id != studioCallLog.CallId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    studioCallLog.CallTime = DateTime.SpecifyKind(studioCallLog.CallTime, DateTimeKind.Utc);

                    if (studioCallLog.UpdatedCallTime.HasValue)
                    {
                        studioCallLog.UpdatedCallTime = DateTime.SpecifyKind(
                            studioCallLog.UpdatedCallTime.Value,
                            DateTimeKind.Utc
                        );
                    }

                    _context.Update(studioCallLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudioCallLogExists(studioCallLog.CallId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerId"] = new SelectList(_context.CustomerRegs, "Id", "StudioName", studioCallLog.CustomerId);
            return View(studioCallLog);
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

        // POST: StudioCallLogs/Delete/5
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

        // GET: StudioCallLogs/GetCustomerDetails/5 (AJAX)
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