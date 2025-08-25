    using InternalProj.Data;
    using InternalProj.Models;
    using InternalProj.ViewModel;
    using InternalProj.Filters;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    namespace InternalProj.Controllers
    {
        [DepartmentAuthorize()]
        public class HeadMasterController : Controller
        {
            private readonly ApplicationDbContext _context;

            public HeadMasterController(ApplicationDbContext context)
            {
                _context = context;
            }

            public IActionResult Index()
            {
                var viewModel = new HeadMasterViewModel
                {
                    MainHeads = _context.MainHeads.Where(m => m.Active == "Y").ToList(),
                    SubHeads = _context.SubHeads.Where(s => s.Active == "Y").ToList(),
                    ChildSubHeads = _context.ChildSubHeads.Where(c => c.Active == "Y").ToList(),
                    AlbumSizes = _context.Albums.Where(a => a.Active == "Y").ToList(),
                    Rates = _context.RateMasters.Where(r => r.Active == "Y").ToList()
                };

                return View(viewModel);
            }

            [HttpPost]
            public async Task<IActionResult> AddSubHead(int mainHeadId, string subHeadName)
            {
                if (string.IsNullOrWhiteSpace(subHeadName))
                    return BadRequest("SubHead name is required.");

                var subHead = new SubHeadDetails
                {
                    MainHeadId = mainHeadId,
                    SubHeadName = subHeadName,
                    Active = "Y",
                    Status = true,
                    MachineId = 1
                };

                _context.SubHeads.Add(subHead);
                await _context.SaveChangesAsync();

                return Json(new { success = true, subHead });
            }

            [HttpPost]
            public async Task<IActionResult> AddChildSubHead(int subHeadId, string childSubHeadName)
            {
                var newChild = new ChildSubHead
                {
                    SubHeadId = subHeadId,
                    ChildSubHeadName = childSubHeadName,
                    Active = "Y"
                };

                _context.ChildSubHeads.Add(newChild);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            [HttpGet]
            public IActionResult GetRates(int subHeadId, int sizeId)
            {
                var rates = _context.RateMasters
                    .Where(r => r.SubHeadId == subHeadId && r.SizeId == sizeId && r.Active == "Y")
                    .Select(r => new
                    {
                        name = r.Details ?? "",
                        rate = r.Rate
                    }).ToList();

                return Json(rates);
            }
        

            [HttpPost]
            public async Task<IActionResult> AddRate(int subHeadId, int sizeId, decimal rate, string name)
            {
                //Console.WriteLine($"Received AddRate: subHeadId={subHeadId}, sizeId={sizeId}, rate={rate}, name={name}");
                var subHead = await _context.SubHeads
                .Where(s => s.SubHeadId == subHeadId)
                .Select(s => new { s.MainHeadId, s.SubHeadName })
                .FirstOrDefaultAsync();

                var size = await _context.Albums
                .Where(a => a.SizeId == sizeId)
                .Select(a => a.Size)
                .FirstOrDefaultAsync();

                var newRate = new RateMaster
                {
                SubHeadId = subHeadId,
                SizeId = sizeId,
                MainHeadId = subHead.MainHeadId,
                Details = $"{subHead.SubHeadName.ToUpper()} - {size.ToUpper()}",
                Rate = rate,
                Active = "Y"
                };

                _context.RateMasters.Add(newRate);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }


            [HttpGet]
            public IActionResult GetSubHeadsAndRates(int mainHeadId, int sizeId)
            {
            var subheads = _context.SubHeads
                .Where(s => s.MainHeadId == mainHeadId && s.Active == "Y")
                .Select(s => new
                {
                    s.SubHeadId,
                    s.SubHeadName,
                    Rate = _context.RateMasters
                        .Where(r => r.SubHeadId == s.SubHeadId && r.SizeId == sizeId && r.Active == "Y")
                        .Select(r => r.Rate)
                        .FirstOrDefault(),
                    HasRate = _context.RateMasters
                        .Any(r => r.SubHeadId == s.SubHeadId && r.SizeId == sizeId && r.Active == "Y"),
                    ChildSubHeads = _context.ChildSubHeads
                        .Where(c => c.SubHeadId == s.SubHeadId && c.Active == "Y")
                        .Select(c => new {
                            c.ChildSubHeadId,
                            c.ChildSubHeadName
                        }).ToList()
                })
                .ToList();

                return Json(subheads);
            }



            [HttpGet]
            public IActionResult GetChildSubHeads(int subHeadId)
            {
                var childList = _context.ChildSubHeads
                .Where(c => c.SubHeadId == subHeadId && c.Active == "Y")
                .Select(c => new
                {
                    c.ChildSubHeadId,
                    c.ChildSubHeadName
                }).ToList();

                return Json(childList);
            }

        }
    }
