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
    [Authorize(Roles = "SuperAdmin")]
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
        public async Task<IActionResult> GetDashboard()
        {
            var kpis = await _service.GetDashboardKpisAsync();
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

        [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
        {
      var companies = await _service.GetCompaniesAsync();
          return Ok(companies);
 }

        [HttpGet("billing")]
        public async Task<IActionResult> GetBilling()
        {
    var billing = await _service.GetBillingAsync();
            return Ok(billing);
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

        private string? GetIp()
        {
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    return !string.IsNullOrEmpty(forwarded) ? forwarded.Split(',')[0].Trim() : HttpContext.Connection.RemoteIpAddress?.ToString();
        }

  private string? GetUserAgent() => HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
    }

    // Request DTOs for the controller
    public class SetStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class SetCompanyActiveRequest
    {
 public bool IsActive { get; set; }
    }

    public class SetPlanUserLimitRequest
    {
        public int? Limit { get; set; }
    }
}
