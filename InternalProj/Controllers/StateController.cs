using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize("ADMIN")]
    public class StateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StateController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StateMaster model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "State name is required." });

            var existing = await _context.StateMasters
                .FirstOrDefaultAsync(s => s.Name.ToLower() == model.Name.ToLower());

            if (existing != null)
                return Json(new { success = false, message = "State already exists." });

            _context.StateMasters.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = model.Id, name = model.Name });
        }

    }
}
