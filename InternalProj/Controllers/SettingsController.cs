using InternalProj.Data;
using InternalProj.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InternalProj.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        public IActionResult Pagination() => PartialView("Pagination");

        public IActionResult Scheduler() => PartialView("Scheduler");

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
                    Url = "#" // or build your dynamic URL here if needed
                }).ToList()
            }
        };
            return PartialView("Dictionary", model);
        }

        public IActionResult PasswordPolicy() => PartialView("PasswordPolicy");

        public IActionResult SmsContent()
        {
            return PartialView("SmsContent");
        }

        [HttpPost]
        public IActionResult UpdateSmsContent(string content)
        {
            try
            {
                return Json(new { success = true, message = "SMS content updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating SMS content: " + ex.Message });
            }
        }

        public IActionResult GetStates()
        {
            var states = _context.State.Select(s => new
            {
                s.Id,
                s.Name,
                s.StateValue
            }).ToList();

            return Json(states);
        }

        public IActionResult GetRegions()
        {
            var regions = _context.Region.Select(r => new
            {
                r.Id,
                r.RegionName
            }).ToList();

            return Json(regions);
        }

        public IActionResult GetSizes()
        {
            var sizes = _context.Size.Select(s => new
            {
                s.Id,
                s.SizeName
            }).ToList();

            return Json(sizes);
        }
    }
}
