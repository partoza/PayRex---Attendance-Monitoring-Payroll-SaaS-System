using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PayRex.Web.Configuration;
using PayRexApplication.Data;
using System.Security.Claims;
using System.Linq;

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
        private static readonly ILookup<string, string> PageModuleMap = MenuConfiguration.GetPageToModuleMap();

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
            => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // Skip for SuperAdmin and Employee (their page access is governed by [Authorize] attributes only)
            if (string.IsNullOrEmpty(role) || role == "SuperAdmin" || role == "Employee")
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
                var moduleNames = PageModuleMap[path];

                if (moduleNames.Any())
                {
                    bool hasExplicitDeny = false;
                    bool hasAnyAllow = false;

                    foreach (var moduleName in moduleNames)
                    {
                        var perm = permissions.FirstOrDefault(p =>
                            string.Equals(p.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));

                        if (perm != null)
                        {
                            if (perm.CanAdd || perm.CanUpdate || perm.CanInactivate)
                            {
                                hasAnyAllow = true;
                                break; // Access allowed by at least one module
                            }
                            else
                            {
                                hasExplicitDeny = true;
                            }
                        }
                    }

                    // Block only if there's an explicit "all-false" record and NO other record allows it
                    if (hasExplicitDeny && !hasAnyAllow)
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
