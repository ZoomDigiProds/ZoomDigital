using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using InternalProj.Data;

namespace InternalProj.Service
{
    public static class SessionHelper
    {
        public static async Task SetStaffSessionAsync(HttpContext context, ApplicationDbContext db, int staffId)
        {
            var staff = await db.StaffRegs
                .Include(s => s.StaffDepartments)
                    .ThenInclude(sd => sd.Department)
                .Include(s => s.StaffDesignations)
                    .ThenInclude(sd => sd.Designation)
                //.Include(s => s.Credentials) // Needed if you want to reset UserName
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

            if (staff == null) return;

            context.Session.SetString("DeptIds", string.Join(",", staff.StaffDepartments.Select(d => d.DeptId)));
            context.Session.SetString("DesigIds", string.Join(",", staff.StaffDesignations.Select(d => d.DesignationId)));
            context.Session.SetString("StaffName", $"{staff.FirstName} {staff.LastName}".Trim());

            context.Session.SetString("UserDepartments", string.Join(",", staff.StaffDepartments.Select(d => d.Department.Name)));

            context.Session.SetString("UserName", staff.Credentials.FirstOrDefault()?.UserName ?? "");
        }
    }
}
