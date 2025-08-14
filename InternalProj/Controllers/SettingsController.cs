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

        public IActionResult GetStateMasterss()
        {
            var states = _context.StateMasters.Select(s => new
            {
                s.Id,
                s.Name,
               
            }).ToList();

            return Json(states);
        }

        public IActionResult GetRegionMasterss()
        {
            var regions = _context.RegionMasters.Select(r => new
            {
                r.Id,
                r.StateId
            }).ToList();

            return Json(regions);
        }

        public IActionResult GetAlbumss()
        {
            var sizes = _context.Albums.Select(s => new
            {
                s.Size,
                s.SizeId
            }).ToList();

            return Json(sizes);
        }
    }
}
