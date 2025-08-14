using InternalProj.Data;
using InternalProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InternalProj.Controllers
{
    public class GeneralSettingsAuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeneralSettingsAuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Audit Log List
        public async Task<IActionResult> Index()
        {
            var logs = await _context.GeneralSettingsAuditLog
                .OrderByDescending(x => x.ModifiedDate)
                .ToListAsync();
            return View(logs);
        }
    }
}
