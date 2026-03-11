using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PayRex.Web.Configuration;
using PayRexApplication.Data;
using System.Security.Claims;

namespace PayRex.Web.Filters
{
    /// <summary>
    /// Global page filter that enforces RolePermission checks for all roles.
    /// SuperAdmin is exempt. Permissions are loaded once per request and
    /// stored in HttpContext.Items for use by the sidebar in _Layout.cshtml.
    /// </summary>
    public class PermissionPageFilter : IAsyncPageFilter
    {
        // Built once from MenuConfiguration — the single source of truth
        private static readonly Dictionary<string, string> PageModuleMap = MenuConfiguration.GetPageToModuleMap();

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
            => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // Skip only for SuperAdmin or unauthenticated
            if (string.IsNullOrEmpty(role) || role == "SuperAdmin")
            {
                await next();
                return;
            }

            // ── Subscription expiry enforcement ──
            // Block expired users from accessing any page except Dashboard, Checkout, and Auth
            var subscriptionStatus = httpContext.User.FindFirst("subscriptionStatus")?.Value;
            var currentPath = httpContext.Request.Path.Value ?? "";
            var allowedWhenExpired = currentPath.Equals("/Dashboard", StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith("/Checkout", StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith("/TestPayment", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(subscriptionStatus, "Expired", StringComparison.OrdinalIgnoreCase) && !allowedWhenExpired)
            {
                context.Result = new RedirectToPageResult("/Dashboard");
                return;
            }

            try
            {
                // Load permissions from DB (one query per request)
                var db = httpContext.RequestServices.GetRequiredService<AppDbContext>();
                var permissions = await db.RolePermissions
                    .AsNoTracking()
                    .Where(p => p.RoleName == role)
                    .ToListAsync();

                // Store for sidebar filtering in _Layout.cshtml
                httpContext.Items["UserPermissions"] = permissions;

                // Check if current page's module is denied
                var path = httpContext.Request.Path.Value ?? "";
                if (PageModuleMap.TryGetValue(path, out var moduleName))
                {
                    var perm = permissions.FirstOrDefault(p =>
                        string.Equals(p.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));

                    // Deny if a record exists with ALL flags false
                    if (perm != null && !perm.CanAdd && !perm.CanUpdate && !perm.CanInactivate)
                    {
                        context.Result = new RedirectToPageResult("/Dashboard");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = httpContext.RequestServices.GetService<ILogger<PermissionPageFilter>>();
                logger?.LogWarning(ex, "Permission check failed, allowing access.");
            }

            await next();
        }
    }
}
