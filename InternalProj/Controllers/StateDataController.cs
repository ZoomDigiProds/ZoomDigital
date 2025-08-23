using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalProj.Controllers
{
    [DepartmentAuthorize()]
    public class StateDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StateDataController (ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var activeStates = _context.StateMasters.Where(s => s.Active == "Y").ToList();
            var inactiveStates = _context.StateMasters.Where(s => (s.Active == "N")).ToList();

            ViewBag.ActiveStates = activeStates;
            ViewBag.InactiveStates = inactiveStates;

            return View();
        }

        [HttpPost]
        public IActionResult ToggleStates([FromBody] List<int> ids)
        {
            var states = _context.StateMasters.Where(s => ids.Contains(s.Id)).ToList();

            foreach (var state in states)
            {

                state.Active = state.Active == "Y" ? "N" : "Y";

            }

            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult AddState(string stateName, string Active)
        {
            if (!string.IsNullOrWhiteSpace(stateName))
            {
                var newState = new StateMaster
                {
                    Name = stateName,
                    Active = Active
                };

                _context.StateMasters.Add(newState);
                _context.SaveChanges();

                return Json(new { success = true, stateId = newState.Id });
            }

            return Json(new { success = false, message = "State name is required" });
        }


        [HttpPost]
        public IActionResult EditState(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return Json(new { success = false, message = "State name cannot be empty" });
            }

            var state = _context.StateMasters.FirstOrDefault(s => s.Id == id);
            if (state == null)
            {
                return Json(new { success = false, message = "State not found" });
            }

            state.Name = newName;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
