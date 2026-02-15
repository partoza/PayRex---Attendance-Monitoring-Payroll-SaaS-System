using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardModel> _logger;

        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalEmployees { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalPlans { get; set; }
        public int PendingInvoices { get; set; }

        public List<NotificationItem> Notifications { get; set; } = new();

        // Additional data for charts
        public List<CompanyItem> TopCompanies { get; set; } = new();
        public List<PlanItem> Plans { get; set; } = new();

        // New: strongly-typed KPI cards for the view
        public List<KpiCard> KpiCards { get; set; } = new();

        public string UserFirstName { get; set; } = "User";

        public DashboardModel(IHttpClientFactory httpClientFactory, ILogger<DashboardModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            // Extract first name from JWT token
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var givenName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                if (!string.IsNullOrEmpty(givenName))
                {
                    UserFirstName = givenName;
                }
                else
                {
                    var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        UserFirstName = email.Split('@')[0];
                    }
                }
            }
            catch { }

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var kpiResponse = await client.GetAsync("api/superadmin/dashboard");
                if (kpiResponse.IsSuccessStatusCode)
                {
                    var json = await kpiResponse.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var kpis = JsonSerializer.Deserialize<KpiData>(json, opts);
                    if (kpis != null)
                    {
                        TotalCompanies = kpis.TotalCompanies;
                        ActiveCompanies = kpis.ActiveCompanies;
                        TotalUsers = kpis.TotalUsers;
                        ActiveUsers = kpis.ActiveUsers;
                        TotalEmployees = kpis.TotalEmployees;
                        MonthlyRevenue = kpis.MonthlyRevenue;
                        TotalPlans = kpis.TotalPlans;
                        PendingInvoices = kpis.PendingInvoices;
                    }
                }

                var notifResponse = await client.GetAsync("api/superadmin/notifications");
                if (notifResponse.IsSuccessStatusCode)
                {
                    var json = await notifResponse.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Notifications = JsonSerializer.Deserialize<List<NotificationItem>>(json, opts) ?? new();
                }

                // Fetch companies (for top companies chart)
                var compResp = await client.GetAsync("api/superadmin/companies");
                if (compResp.IsSuccessStatusCode)
                {
                    var json = await compResp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var companies = JsonSerializer.Deserialize<List<CompanyItem>>(json, opts) ?? new();
                    // keep top6 by user count
                    TopCompanies = companies.OrderByDescending(c => c.UserCount).Take(6).ToList();
                }

                // Fetch plans
                var planResp = await client.GetAsync("api/superadmin/plans");
                if (planResp.IsSuccessStatusCode)
                {
                    var json = await planResp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Plans = JsonSerializer.Deserialize<List<PlanItem>>(json, opts) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard data");
            }

            // Build KpiCards for the view (strongly-typed)
            KpiCards = new List<KpiCard>
            {
                new KpiCard
                {
                    Title = "Total Companies",
                    Value = TotalCompanies,
                    BadgeText = $"{ActiveCompanies} active",
                    BadgeColor = "green",
                    IconColor = "blue",
                    Href = "/Admin/Companies",
                    Description = "View companies ?",
                    IconPath = "M44a220002-2h8a2200122v12a1101102h-3a11001-1-1v-2a11000-1-1H9a11000-11v2a11001-11H4a110110-2V4zm31h2v2H7V5zm24H7v2h2V9zm2-4h2v2h-2V5zm24h-2v2h2V9z"
                },
                new KpiCard
                {
                    Title = "Total Users",
                    Value = TotalUsers,
                    BadgeText = $"{ActiveUsers} active",
                    BadgeColor = "green",
                    IconColor = "green",
                    Href = "/Admin/Users",
                    Description = "Manage users ?",
                    IconPath = "M109a330100-63300006zm-79a77011140H3z"
                },
                new KpiCard
                {
                    Title = "Total Employees",
                    Value = TotalEmployees,
                    BadgeText = "across companies",
                    BadgeColor = "gray",
                    IconColor = "purple",
                    Href = "/Admin/Reports",
                    Description = "View reports ?",
                    IconPath = "M66V5a330013-3h2a3300133v1h2a2200122v3.57A22.95222.9520011013a22.9522.95001-8-1.43V8a220012-2h2zm2-1a110011-1h2a1100111v1H8V5zm15a110011-1h.01a1101102H10a11001-1-1z M213.692V16a2200022h12a220002-2v-2.308A24.97424.9740011015c-2.7960-5.487-.46-8-1.308z"
                },
                new KpiCard
                {
                    Title = "Monthly Revenue",
                    Value = $"?{MonthlyRevenue:N2}",
                    BadgeText = $"{PendingInvoices} pending",
                    BadgeColor = "red",
                    IconColor = "yellow",
                    Href = "/Admin/Billing",
                    Description = "View billing ?",
                    IconPath = "M44a22000-22v4a2200022h6a220002-2V6a22000-2-2H4zm26a220112-222001-22zm8-2a22011-22220012-2zm-48a660100-1266000012z"
                }
            };

            return Page();
        }

        public class KpiData
        {
            public int TotalCompanies { get; set; }
            public int ActiveCompanies { get; set; }
            public int TotalUsers { get; set; }
            public int ActiveUsers { get; set; }
            public int TotalEmployees { get; set; }
            public decimal MonthlyRevenue { get; set; }
            public int TotalPlans { get; set; }
            public int PendingInvoices { get; set; }
        }

        public class NotificationItem
        {
            public string Type { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        public class CompanyItem
        {
            public string CompanyId { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public string PlanName { get; set; } = string.Empty;
            public int UserCount { get; set; }
            public int EmployeeCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class PlanItem
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int MaxEmployees { get; set; }
            public decimal Price { get; set; }
            public string BillingCycle { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
        }

        // New strongly-typed KpiCard class
        public class KpiCard
        {
            public string Title { get; set; } = string.Empty;
            public object Value { get; set; } = string.Empty;
            public string BadgeText { get; set; } = string.Empty;
            public string BadgeColor { get; set; } = string.Empty;
            public string IconColor { get; set; } = string.Empty;
            public string Href { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string IconPath { get; set; } = string.Empty;
        }
    }
}
