using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AuditLogsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuditLogsModel> _logger;

        public List<AuditLogItem> AuditItems { get; set; } = new();
        public AuditStats Stats { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 15;
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Action { get; set; }
        [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
        public string? UserRole { get; set; }

        public AuditLogsModel(IHttpClientFactory httpClientFactory, ILogger<AuditLogsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                UserRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            }
            catch { }

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // SuperAdmin sees all companies; Admin/HR/Accountant see only their company's data
            var isSuperAdmin = string.Equals(UserRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            var baseEndpoint = isSuperAdmin ? "api/superadmin/audit-logs" : "api/admin/audit-logs";

            try
            {
                var queryParams = $"page={CurrentPage}&pageSize={PageSize}";
                if (!string.IsNullOrEmpty(Search)) queryParams += $"&search={Uri.EscapeDataString(Search)}";
                if (!string.IsNullOrEmpty(Action)) queryParams += $"&action={Uri.EscapeDataString(Action)}";
                if (!string.IsNullOrEmpty(FromDate)) queryParams += $"&from={Uri.EscapeDataString(FromDate)}";
                if (!string.IsNullOrEmpty(ToDate)) queryParams += $"&to={Uri.EscapeDataString(ToDate)}";

                var response = await client.GetAsync($"{baseEndpoint}?{queryParams}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuditLogListResponse>(json, opts);
                    if (result != null)
                    {
                        AuditItems = result.Items ?? new();
                        // Convert UTC timestamps to Philippine Time (UTC+8)
                        var pht = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        foreach (var item in AuditItems)
                            item.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(
                                DateTime.SpecifyKind(item.Timestamp, DateTimeKind.Utc), pht);
                        TotalCount = result.TotalCount;
                        Stats = result.Stats ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit logs");
            }

            return Page();
        }

        public class AuditLogItem
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string Action { get; set; } = "";
            public string? UserEmail { get; set; }
            public string? UserName { get; set; }
            public string? IpAddress { get; set; }
            public string? Details { get; set; }
            public string? Role { get; set; }
            public int? CompanyId { get; set; }
            public string? CompanyName { get; set; }
        }

        public class AuditStats
        {
            public int FailedLogins { get; set; }
            public int Registrations { get; set; }
            public int PasswordResets { get; set; }
            public int SuccessfulLogins { get; set; }
        }

        public class AuditLogListResponse
        {
            public List<AuditLogItem>? Items { get; set; }
            public int TotalCount { get; set; }
            public AuditStats? Stats { get; set; }
        }
    }
}
