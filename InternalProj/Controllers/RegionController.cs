using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize()]
    public class RegionController : Controller
    {

        private readonly ApplicationDbContext _context;

        public RegionController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RegionMaster model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.StateId == 0)
                return Json(new { success = false, message = "Region name and state are required." });

            var existing = await _context.RegionMasters
                .FirstOrDefaultAsync(r => r.Name.ToLower() == model.Name.ToLower() && r.StateId == model.StateId);

            if (existing != null)
                return Json(new { success = false, message = "Region already exists in the selected state." });

            _context.RegionMasters.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = model.Id, name = model.Name });
        }

    }
}
