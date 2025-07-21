using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using InternalProj.Data;

public class SessionRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public SessionRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var session = context.Session;
        var staffIdStr = session.GetString("StaffId");

        if (!string.IsNullOrEmpty(staffIdStr) && int.TryParse(staffIdStr, out var staffId))
        {
            // Get current session values
            var currentDepartments = session.GetString("UserDepartments");
            var currentDesignations = session.GetString("UserDesignations");

            // Fetch latest data from DB
            var latestDepartments = await db.StaffDepartments
                .Where(sd => sd.StaffId == staffId)
                .Include(sd => sd.Department)
                .Select(sd => sd.Department.Name)
                .ToListAsync();

            var latestDesignations = await db.StaffDesignations
                .Where(sd => sd.StaffId == staffId)
                .Include(sd => sd.Designation)
                .Select(sd => sd.Designation.Name)
                .ToListAsync();

            var joinedDept = string.Join(",", latestDepartments);
            var joinedDesg = string.Join(",", latestDesignations);

            // Update session only if changed
            if (joinedDept != currentDepartments)
                session.SetString("UserDepartments", joinedDept);

            if (joinedDesg != currentDesignations)
                session.SetString("UserDesignations", joinedDesg);
        }

        await _next(context);
    }
}
