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
        public List<GrowthDataPoint> GrowthData { get; set; } = new();
        public List<AuditLogItem> RecentActivity { get; set; } = new();
        public List<SystemNotificationItem> SystemNotifications { get; set; } = new();

        // New: strongly-typed KPI cards for the view
        public List<KpiCard> KpiCards { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? From { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? To { get; set; }

        public FinanceSummaryDto? FinanceSummary { get; set; }

        public CompanyFinanceDto CompanyFinance { get; set; } = new();

        public string UserFirstName { get; set; } = "User";
        public string UserRole { get; set; } = "Admin";
        public string? SubscriptionWarning { get; set; }

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
                UserRole = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "Admin";
            }
            catch { }

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var queryParams = $"?from={From}&to={To}";
                var kpiResponse = await client.GetAsync($"api/superadmin/dashboard{queryParams}");
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
                        GrowthData = kpis.GrowthData ?? new();
                    }
                }

                // Fetch real audit logs for Recent Activity
                var auditResp = await client.GetAsync("api/superadmin/audit-logs?page=1&pageSize=4");
                if (auditResp.IsSuccessStatusCode)
                {
                    var json = await auditResp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var auditLogList = JsonSerializer.Deserialize<AuditLogListDto>(json, opts);
                    RecentActivity = auditLogList?.Items ?? new();
                }

                // Fetch latest 2 System Notifications
                var sysNotifResp = await client.GetAsync("api/superadmin/system-notifications");
                if (sysNotifResp.IsSuccessStatusCode)
                {
                    var json = await sysNotifResp.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var sysNotifs = JsonSerializer.Deserialize<List<SystemNotificationItem>>(json, opts) ?? new();
                    SystemNotifications = sysNotifs.OrderByDescending(n => n.CreatedAt).Take(4).ToList();
                }

                var notifResponse = await client.GetAsync("api/superadmin/notifications");
                if (notifResponse.IsSuccessStatusCode)
                {
                    var json = await notifResponse.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Notifications = JsonSerializer.Deserialize<List<NotificationItem>>(json, opts) ?? new();
                }

                // Fetch Finance Summary for the report (SuperAdmin-level)
                var financeResponse = await client.GetAsync($"api/superadmin/finance/summary?fromDate={From}&toDate={To}");
                if (financeResponse.IsSuccessStatusCode)
                {
                    var json = await financeResponse.Content.ReadAsStringAsync();
                    FinanceSummary = JsonSerializer.Deserialize<FinanceSummaryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Fetch company-level subscription finance (cost tracking for Admin)
                var compFinanceResp = await client.GetAsync("api/billing/finance-summary");
                if (compFinanceResp.IsSuccessStatusCode)
                {
                    var json = await compFinanceResp.Content.ReadAsStringAsync();
                    CompanyFinance = JsonSerializer.Deserialize<CompanyFinanceDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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

                // If Admin, fetch own subscription for expiry/downgrade notifications
                if (UserRole == "Admin")
                {
                    var subResp = await client.GetAsync("api/billing/subscription");
                    if (subResp.IsSuccessStatusCode)
                    {
                        var json = await subResp.Content.ReadAsStringAsync();
                        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var sub = JsonSerializer.Deserialize<SubscriptionInfoItem>(json, opts);
                        if (sub != null)
                        {
                            if (sub.DaysRemaining <= 5 && !sub.IsExpired)
                            {
                                var planType = sub.IsTrialing ? "trial" : $"{sub.PlanName} subscription";
                                SubscriptionWarning = $"Your {planType} expires in {sub.DaysRemaining} day{(sub.DaysRemaining == 1 ? "" : "s")}. Please renew soon.";
                            }
                            if (sub.PendingDowngradePlanId.HasValue && !string.IsNullOrEmpty(sub.PendingDowngradePlanName))
                            {
                                Notifications.Insert(0, new NotificationItem
                                {
                                    Type = "PendingDowngrade",
                                    Message = $"Your plan is scheduled to downgrade to {sub.PendingDowngradePlanName} on {sub.EndDate:MMM dd, yyyy}.",
                                    Timestamp = DateTime.UtcNow
                                });
                            }
                        }
                    }
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
                    ButtonText = "View companies",
                    IconPath = "M4 4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v12a1 1 0 1 1 0 2h-3a1 1 0 0 1-1-1v-2a1 1 0 0 0-1-1H9a1 1 0 0 0-1 1v2a1 1 0 0 1-1 1H4a1 1 0 1 1 0-2V4zm3 1h2v2H7V5zm2 4H7v2h2V9zm2-4h2v2h-2V5zm2 4h-2v2h2V9z"
                },
                new KpiCard
                {
                    Title = "Total Users",
                    Value = TotalUsers,
                    BadgeText = $"{ActiveUsers} active",
                    BadgeColor = "blue",
                    IconColor = "green",
                    Href = "/Admin/Users",
                    ButtonText = "Manage users",
                    IconPath = "M10 9a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm-7 9a7 7 0 1 1 14 0H3z"
                },
                new KpiCard
                {
                    Title = "Total Employees",
                    Value = TotalEmployees,
                    BadgeText = "across companies",
                    BadgeColor = "yellow",
                    IconColor = "blue",
                    Href = "/Admin/Reports",
                    ButtonText = "View reports",
                    IconPath = "M6 6V5a3 3 0 0 1 3-3h2a3 3 0 0 1 3 3v1h2a2 2 0 0 1 2 2v3.57A22.952 22.952 0 0 1 10 13a22.952 22.952 0 0 1-8-1.43V8a2 2 0 0 1 2-2h2zm2-1a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v1H8V5zm1 5a1 1 0 0 1 1-1h.01a1 1 0 1 1 0 2H10a1 1 0 0 1-1-1z M2 13.692V16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2.308A24.974 24.974 0 0 1 10 15c-2.796 0-5.487-.46-8-1.308z"
                },
                new KpiCard
                {
                    Title = "Monthly Revenue",
                    Value = $"₱{MonthlyRevenue:N2}",
                    BadgeText = $"{PendingInvoices} pending",
                    BadgeColor = "yellow",
                    IconColor = "yellow",
                    Href = "/Admin/Billing",
                    ButtonText = "View billing",
                    IconPath = "M4 4a2 2 0 0 0-2 2v4a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2H4zm2 6a2 2 0 1 1 2-2 2 2 0 0 1-2 2zm8-2a2 2 0 1 1-2 2 2 2 0 0 1 2-2zm-4 8a6 6 0 1 0 0-12 6 6 0 0 0 0 12z"
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
            public List<GrowthDataPoint> GrowthData { get; set; } = new();
        }

        public class GrowthDataPoint
        {
            public string Label { get; set; } = string.Empty;
            public int UserCount { get; set; }
            public decimal Revenue { get; set; }
        }

        public class AuditLogListDto
        {
            public List<AuditLogItem> Items { get; set; } = new();
            public int TotalCount { get; set; }
        }

        public class AuditLogItem
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string Action { get; set; } = string.Empty;
            public string? UserEmail { get; set; }
            public string? UserName { get; set; }
            public string? IpAddress { get; set; }
            public string? Details { get; set; }
            public string? Role { get; set; }
            public string? CompanyName { get; set; }
        }

        public class SystemNotificationItem
        {
            public int NotificationId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string? TargetRoles { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class NotificationItem
        {
            public string Type { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        public class SubscriptionInfoItem
        {
            public int PlanId { get; set; }
            public string PlanName { get; set; } = string.Empty;
            public int DaysRemaining { get; set; }
            public bool IsExpired { get; set; }
            public bool IsTrialing { get; set; }
            public DateTime EndDate { get; set; }
            public int? PendingDowngradePlanId { get; set; }
            public string? PendingDowngradePlanName { get; set; }
        }

        public class CompanyItem
        {
            public int CompanyId { get; set; }
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
            public string ButtonText { get; set; } = string.Empty;
            public string IconPath { get; set; } = string.Empty;
        }

        public class FinanceSummaryDto
        {
            public decimal TotalIncome { get; set; }
            public decimal TotalDeductions { get; set; }
            public decimal TotalVat { get; set; }
            public decimal NetProfit { get; set; }
            public int EntryCount { get; set; }
        }

        public class CompanyFinanceDto
        {
            public decimal TotalSubscriptionCost { get; set; }
            public int EntryCount { get; set; }
        }
    }
}
