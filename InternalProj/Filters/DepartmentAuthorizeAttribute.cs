using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InternalProj.Filters
{
    public class DepartmentAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedDepartments;

        public DepartmentAuthorizeAttribute(params string[] allowedDepartments)
        {
            _allowedDepartments = allowedDepartments ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var username = session.GetString("UserName");
            var userDepartments = session.GetString("UserDepartments");

            var isFirstLogin = session.GetString("IsFirstLogin") == "true";
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            if (isFirstLogin && !(controller == "Account" && action == "ResetPassword"))
            {
                var staffId = session.GetString("StaffId");
                context.Result = new RedirectToActionResult("ResetPassword", "Account", new { staffId });
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (string.IsNullOrEmpty(userDepartments))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            var userDeptList = userDepartments
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(d => d.ToUpperInvariant());

            if (userDeptList.Contains("ADMIN"))
            {
                base.OnActionExecuting(context);
                return;
            }

            if (!_allowedDepartments.Any(dep => userDeptList.Contains(dep.ToUpperInvariant())))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}