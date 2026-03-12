using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayRexApplication.Enums;
using PayRexApplication.Helpers;
using PayRexApplication.Services;

namespace PayRexApplication.Controllers
{
    /// <summary>
    /// API controller for SuperAdmin management operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _service;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(ISuperAdminService service, ILogger<SuperAdminController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string? from = null, [FromQuery] string? to = null)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(from, out var fd)) fromDate = fd;
            if (DateTime.TryParse(to, out var td)) toDate = td;

            var kpis = await _service.GetDashboardKpisAsync(fromDate, toDate);
            return Ok(kpis);
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifs = await _service.GetNotificationsAsync();
            return Ok(notifs);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _service.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _service.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _service.GetCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("companies/all")]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _service.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("billing")]
        public async Task<IActionResult> GetBilling()
        {
            var billing = await _service.GetBillingAsync();
            return Ok(billing);
        }

        [HttpGet("billing/archived")]
        public async Task<IActionResult> GetArchivedBilling()
        {
            var billing = await _service.GetArchivedBillingAsync();
            return Ok(billing);
        }

        [HttpPost("billing/{invoiceId}/archive")]
        public async Task<IActionResult> ArchiveInvoice(int invoiceId)
        {
            var success = await _service.ArchiveInvoiceAsync(invoiceId);
            if (!success) return NotFound(new { message = "Invoice not found" });
            return Ok(new { message = "Invoice archived successfully" });
        }

        /// <summary>
        /// Public endpoint to get active plans for landing page (no auth required).
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _service.GetPlansAsync();
            return Ok(plans);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _service.GetCurrentSystemSettingsAsync();
            if (settings == null) return NotFound(new { message = "No system settings found" });
            return Ok(settings);
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 15,
            [FromQuery] string? search = null, [FromQuery] string? action = null,
            [FromQuery] string? from = null, [FromQuery] string? to = null,
            [FromQuery] int? companyId = null)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(from, out var fd)) fromDate = fd;
            if (DateTime.TryParse(to, out var td)) toDate = td.Date.AddDays(1);

            var result = await _service.GetAuditLogsAsync(page, pageSize, search, action, fromDate, toDate, companyId);
            return Ok(result);
        }

        [HttpGet("audit-logs/stats")]
        public async Task<IActionResult> GetAuditLogStats([FromQuery] int? companyId = null)
        {
            var stats = await _service.GetAuditLogStatsAsync(companyId);
            return Ok(stats);
        }

        [HttpPost("users/{userId}/status")]
        public async Task<IActionResult> SetUserStatus(int userId, [FromBody] SetStatusRequest request)
        {
            if (!Enum.TryParse<UserStatus>(request.Status, true, out var status))
                return BadRequest(new { message = "Invalid status value" });

            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.SetUserStatusAsync(userId, status, actorId.Value,
              GetIp(), GetUserAgent());

            return result ? Ok(new { message = "User status updated" }) : BadRequest(new { message = "Failed to update user status" });
        }

        [HttpPost("users/{userId}/role")]
        public async Task<IActionResult> SetUserRole(int userId, [FromBody] SetRoleRequest request)
        {
            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.SetUserRoleAsync(userId, request.Role, actorId.Value, GetIp(), GetUserAgent());
            return result ? Ok(new { message = "User role updated" }) : BadRequest(new { message = "Failed to update user role" });
        }

        [HttpPost("users/{userId}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int userId)
        {
            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.ResetUserPasswordAsync(userId, actorId.Value, GetIp(), GetUserAgent());
            return result ? Ok(new { message = "User password reset" }) : BadRequest(new { message = "Failed to reset password" });
        }

        [HttpPost("companies/{companyId}/status")]
        public async Task<IActionResult> SetCompanyStatus(string companyId, [FromBody] SetCompanyActiveRequest request)
        {
            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.SetCompanyStatusAsync(companyId, request.IsActive, actorId.Value,
                 GetIp(), GetUserAgent());

            return result ? Ok(new { message = "Company status updated" }) : BadRequest(new { message = "Failed to update company status" });
        }

        [HttpPost("plans/{planId}/user-limit")]
        public async Task<IActionResult> SetPlanUserLimit(int planId, [FromBody] SetPlanUserLimitRequest request)
        {
            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.SetPlanUserLimitAsync(planId, request.Limit, actorId.Value,
               GetIp(), GetUserAgent());

            return result ? Ok(new { message = "Plan user limit updated" }) : BadRequest(new { message = "Plan not found" });
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingDto dto)
        {
            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            if (actorId == null) return Unauthorized();

            var result = await _service.UpdateSystemSettingsAsync(dto, actorId.Value,
                         GetIp(), GetUserAgent());

            return result ? Ok(new { message = "System settings updated" }) : BadRequest(new { message = "Failed to update settings" });
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _service.GetPermissionsAsync();
            return Ok(permissions);
        }

        [HttpPost("permissions")]
        public async Task<IActionResult> SavePermissions([FromBody] List<PayRexApplication.Services.RolePermissionDto> permissions)
        {
            if (permissions == null || permissions.Count == 0)
                return BadRequest(new { message = "No permissions provided" });

            var result = await _service.SavePermissionsAsync(permissions);
            return result ? Ok(new { message = "Permissions saved successfully" }) : BadRequest(new { message = "Failed to save permissions" });
        }

        private string? GetIp()
        {
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            return !string.IsNullOrEmpty(forwarded) ? forwarded.Split(',')[0].Trim() : HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent() => HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        // ===== Finance Endpoints =====

        [HttpGet("finance")]
        public async Task<IActionResult> GetFinanceEntries([FromQuery] string? type, [FromQuery] string? category, [FromQuery] string? fromDate, [FromQuery] string? toDate)
        {
            DateTime? from = DateTime.TryParse(fromDate, out var fd) ? fd : null;
            DateTime? to = DateTime.TryParse(toDate, out var td) ? td : null;
            var entries = await _service.GetFinanceEntriesAsync(type, category, from, to);
            return Ok(entries);
        }

        [HttpGet("finance/summary")]
        public async Task<IActionResult> GetFinanceSummary([FromQuery] string? fromDate, [FromQuery] string? toDate)
        {
            DateTime? from = DateTime.TryParse(fromDate, out var fd) ? fd : null;
            DateTime? to = DateTime.TryParse(toDate, out var td) ? td : null;
            var summary = await _service.GetFinanceSummaryAsync(from, to);
            return Ok(summary);
        }

        [HttpPost("finance")]
        public async Task<IActionResult> AddFinanceEntry([FromBody] AddFinanceEntryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.Description) || request.Amount <= 0)
                return BadRequest(new { message = "Invalid finance entry" });

            var uid = int.TryParse(User.FindFirst("uid")?.Value, out var userId) ? userId : (int?)null;
            var result = await _service.AddFinanceEntryAsync(request.Type, request.Description, request.Amount, request.Category, request.Reference, uid);
            return result ? Ok(new { message = "Finance entry added" }) : BadRequest(new { message = "Failed to add entry" });
        }

        [HttpPut("plans/{planId}")]
        public async Task<IActionResult> UpdatePlan(int planId, [FromBody] UpdatePlanRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || request.Price < 0 || request.MaxEmployees <= 0)
                return BadRequest(new { message = "Invalid plan data" });

            var result = await _service.UpdatePlanAsync(planId, request.Name, request.Price, request.MaxEmployees, request.Description);
            return result ? Ok(new { message = "Plan updated" }) : NotFound(new { message = "Plan not found" });
        }

        [HttpGet("system-notifications")]
        public async Task<IActionResult> GetSystemNotifications()
        {
            var notifs = await _service.GetSystemNotificationsAsync();
            return Ok(notifs);
        }

        [HttpPost("system-notifications")]
        public async Task<IActionResult> AddSystemNotification([FromBody] AddSystemNotificationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { message = "Title and message are required" });

            var uid = int.TryParse(User.FindFirst("uid")?.Value, out var userId) ? userId : (int?)null;
            var result = await _service.AddSystemNotificationAsync(request.Title, request.Message, request.Type ?? "info", request.TargetRoles, uid);
            return result ? Ok(new { message = "Notification created" }) : BadRequest(new { message = "Failed to create notification" });
        }

        [HttpPut("system-notifications/{id}/toggle")]
        public async Task<IActionResult> ToggleSystemNotification(int id, [FromBody] ToggleNotificationRequest request)
        {
            var result = await _service.ToggleSystemNotificationAsync(id, request.IsActive);
            return result ? Ok(new { message = "Notification updated" }) : NotFound(new { message = "Notification not found" });
        }

        [HttpDelete("system-notifications/{id}")]
        public async Task<IActionResult> DeleteSystemNotification(int id)
        {
            var result = await _service.DeleteSystemNotificationAsync(id);
            return result ? Ok(new { message = "Notification deleted" }) : NotFound(new { message = "Notification not found" });
        }

        [HttpGet("system-notifications/role/{role}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNotificationsForRole(string role)
        {
            int? userId = null;
            if (int.TryParse(User.FindFirst("uid")?.Value, out var uid))
                userId = uid;

            var notifs = await _service.GetActiveNotificationsForRoleAsync(role, userId);
            return Ok(notifs);
        }

        [HttpPost("system-notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            if (!int.TryParse(User.FindFirst("uid")?.Value, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _service.MarkNotificationReadAsync(userId, id);
            return result ? Ok(new { message = "Notification marked as read" }) : BadRequest(new { message = "Failed to mark notification" });
        }

        [HttpPost("system-notifications/read-all")]
        public async Task<IActionResult> MarkAllNotificationsRead([FromBody] MarkAllReadRequest request)
        {
            if (!int.TryParse(User.FindFirst("uid")?.Value, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _service.MarkAllNotificationsReadAsync(userId, request.Role);
            return result ? Ok(new { message = "All notifications marked as read" }) : BadRequest(new { message = "Failed" });
        }
    }

    // Request DTOs for the controller
    public class SetStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class SetRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class SetCompanyActiveRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetPlanUserLimitRequest
    {
        public int? Limit { get; set; }
    }

    public class AddFinanceEntryRequest
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? Reference { get; set; }
    }

    public class UpdatePlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxEmployees { get; set; }
        public string? Description { get; set; }
    }

    public class AddSystemNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? TargetRoles { get; set; }
    }

    public class ToggleNotificationRequest
    {
        public bool IsActive { get; set; }
    }

    public class MarkAllReadRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
