using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PKFAuditManagement
{
    public class RoleRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<IdentityUser> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                var roles = await userManager.GetRolesAsync(user);

                // Only redirect when accessing the root or login page
                if (context.Request.Path == "/" || context.Request.Path.StartsWithSegments("/Identity/Account/Login"))
                {
                    if (roles.Contains("Admin"))
                    {
                        context.Response.Redirect("/Admin/AdminDashboard");
                        return;
                    }
                    else if (roles.Contains("Auditor"))
                    {
                        context.Response.Redirect("/Home/Dashboard");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
