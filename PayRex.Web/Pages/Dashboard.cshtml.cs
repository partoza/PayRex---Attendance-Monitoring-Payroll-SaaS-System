using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IAuthApiService _authApiService;
        private readonly IAttendanceApiService _attendance;
        private readonly IPayrollApiService _payroll;
        private readonly IBillingApiService _billing;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardModel> _logger;

        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string? UserId { get; set; }
        public string? UserFirstName { get; set; }
        
        public DashboardStats Stats { get; set; } = new();
        public FinanceStats Finance { get; set; } = new();
        public SuperAdminKpisData SuperAdminKpis { get; set; } = new();
        public SuperAdminFinanceData SuperAdminFinance { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();
        public List<AttendanceRecordDto> RecentAttendance { get; set; } = new();
        public List<PayrollPeriodDto> RecentPeriods { get; set; } = new();
        public List<HolidayDto> UpcomingHolidays { get; set; } = new();
        public SubscriptionInfoDto? Subscription { get; set; }
        public List<PlanDto> AvailablePlans { get; set; } = new();
        public bool ShowRenewalModal { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateTo { get; set; }

        public DashboardModel(IAuthApiService authApiService, IAttendanceApiService attendance, IPayrollApiService payroll, IBillingApiService billing, IHttpClientFactory httpClientFactory, ILogger<DashboardModel> logger)
        {
            _authApiService = authApiService;
            _attendance = attendance;
            _payroll = payroll;
            _billing = billing;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Block navigation if password change is required
                var mustChangePassword = jwtToken.Claims.FirstOrDefault(c => c.Type == "mustChangePassword")?.Value;
                if (!string.IsNullOrEmpty(mustChangePassword) && mustChangePassword == "true")
                {
                    return RedirectToPage("/Auth/ChangePassword", new { forced = true });
                }

                UserEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                UserId = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                UserRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                UserFirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value ?? "User";

                // HR/Accountant in Employee View mode should see employee dashboard
                var isEmployeeViewMode = (UserRole == "HR" || UserRole == "Accountant")
                    && Request.Cookies["PayRex.ViewMode"] == "employee";

                // Fetch real data
                if (UserRole == "SuperAdmin")
                {
                    await LoadSuperAdminDataAsync(token);
                    return Page();
                }

                if (UserRole == "Employee" || isEmployeeViewMode)
                {
                    // Override UserRole for the view so it renders employee cards
                    if (isEmployeeViewMode)
                        UserRole = "Employee";

                    // Parallel fetch: attendance, leave balance, payslips
                    var attendanceTask = _attendance.GetAttendanceRecordsAsync(token);
                    var balanceTask = _payroll.GetLeaveBalanceAsync(token);
                    var payslipsTask = _payroll.GetPayslipsAsync(token, selfOnly: true);
                    await Task.WhenAll(attendanceTask, balanceTask, payslipsTask);

                    var attendance = attendanceTask.Result;
                    RecentAttendance = attendance.Take(5).ToList();
                    Stats.PresentDays = attendance.Count(a => a.Status == "Present" || a.Status == "Late");
                    Stats.LateDays = attendance.Count(a => a.Status == "Late");

                    var balance = balanceTask.Result;
                    Stats.LeaveBalance = balance.VacationRemaining + balance.SickRemaining;

                    var payslips = payslipsTask.Result;
                    Stats.EstimatedPay = payslips.FirstOrDefault()?.NetPay ?? 0;
                }
                else
                {
                    // Admin/HR/Accountant stats — parallel fetch
                    var todayStatsTask = _attendance.GetTodayStatsAsync(token);
                    var periodsTask = _payroll.GetPeriodsAsync(token);
                    await Task.WhenAll(todayStatsTask, periodsTask);

                    var todayStats = todayStatsTask.Result;
                    Stats.TotalEmployees = todayStats.TotalEmployees;
                    Stats.PresentToday = todayStats.Present;
                    Stats.OnTimePercentage = todayStats.TotalEmployees > 0 
                        ? (int)Math.Round((double)(todayStats.Present - todayStats.Late) / todayStats.TotalEmployees * 100) : 0;

                    var periods = periodsTask.Result;
                    RecentPeriods = periods.Take(5).ToList();
                    Stats.PendingPayroll = periods.Count(p => p.Status == "Draft" || p.Status == "Computed");
                    Stats.PayrollCost = periods.FirstOrDefault()?.TotalNetPay ?? 0;

                    // Finance stats: aggregate payroll summaries for selected month
                    await LoadFinanceStatsAsync(token, periods);
                }

                // Fetch holidays
                var currentYear = DateTime.Now.Year;
                var holidaysResponse = await _payroll.GetHolidaysAsync(token, currentYear);
                if (holidaysResponse != null)
                {
                    // Assuming holidays is List<HolidayDto>. The API returns them.
                    UpcomingHolidays = holidaysResponse
                        .Where(h => h.Date.Date >= DateTime.Now.Date)
                        .OrderBy(h => h.Date)
                        .Take(5)
                        .ToList();
                }

                // Load subscription status and plans for renewal modal
                if (UserRole == "Admin")
                {
                    try
                    {
                        Subscription = await _billing.GetSubscriptionAsync(token);

                        // Always load plans so the renewal/upgrade modal is available
                        var planClient = _httpClientFactory.CreateClient("PayRexApi");
                        planClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var plansResp = await planClient.GetAsync("api/superadmin/plans");
                        if (plansResp.IsSuccessStatusCode)
                        {
                            var plansJson = await plansResp.Content.ReadAsStringAsync();
                            AvailablePlans = JsonSerializer.Deserialize<List<PlanDto>>(plansJson, jsonOpts) ?? new();
                            // Remove the free/trial plan
                            AvailablePlans = AvailablePlans.Where(p => p.Price > 0).ToList();
                        }

                        // Auto-show modal only when expired
                        ShowRenewalModal = Subscription?.IsExpired == true;
                    }
                    catch { /* ignore */ }
                }

                // Fetch recent audit logs as recent activity
                try
                {
                    var client = _httpClientFactory.CreateClient("PayRexApi");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var resp = await client.GetAsync("api/admin/audit-logs?page=1&pageSize=10");
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<AuditLogResponse>(json, jsonOpts);
                        var pht = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        if (result?.Items != null)
                        {
                            foreach (var log in result.Items.Take(10))
                            {
                                var ts = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc), pht);
                                var typeMap = log.Action?.ToLower() switch
                                {
                                    "login" or "successful login" => "Success",
                                    "failed login" => "error",
                                    _ => "info"
                                };
                                var timeAgo = (DateTime.UtcNow - log.Timestamp).TotalHours < 1 ? "Just now"
                                    : (DateTime.UtcNow - log.Timestamp).TotalHours < 24 ? $"{(int)(DateTime.UtcNow - log.Timestamp).TotalHours}h ago"
                                    : ts.ToString("MMM dd, yyyy");
                                RecentActivities.Add(new ActivityItem
                                {
                                    Title = log.Action ?? "Activity",
                                    Description = log.UserName ?? log.UserEmail ?? "Unknown",
                                    Time = timeAgo,
                                    Type = typeMap
                                });
                            }
                        }
                    }
                }
                catch { /* ignore audit log errors for dashboard */ }
                if (!RecentActivities.Any())
                    RecentActivities.Add(new ActivityItem { Title = "No recent activity", Description = "Activity will appear here", Time = "—", Type = "info" });

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading JWT token");
                Response.Cookies.Delete("PayRex.AuthToken", new CookieOptions { Path = "/" });
                return RedirectToPage("/Auth/Login");
            }
        }

        public class DashboardStats
        {
            // Admin/HR Stats
            public int TotalEmployees { get; set; }
            public int NewHires { get; set; }
            public int PresentToday { get; set; }
            public int OnTimePercentage { get; set; }
            public int PendingPayroll { get; set; }
            public decimal PayrollCost { get; set; }
            
            // Employee Stats
            public int PresentDays { get; set; }
            public int LateDays { get; set; }
            public int LeaveBalance { get; set; }
            public decimal EstimatedPay { get; set; }
        }

        public class FinanceStats
        {
            public decimal GrossIncome { get; set; }
            public decimal NetRevenue { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal GovernmentTaxes { get; set; }
            public string SelectedDateFrom { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            public string SelectedDateTo { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString("yyyy-MM-dd");
        }

        private async Task LoadFinanceStatsAsync(string token, List<PayrollPeriodDto> periods)
        {
            // Determine date range
            DateTime dateFrom, dateTo;
            if (!string.IsNullOrEmpty(DateFrom) && DateTime.TryParse(DateFrom, out var parsedFrom))
                dateFrom = parsedFrom.Date;
            else
                dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!string.IsNullOrEmpty(DateTo) && DateTime.TryParse(DateTo, out var parsedTo))
                dateTo = parsedTo.Date;
            else
                dateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            Finance.SelectedDateFrom = dateFrom.ToString("yyyy-MM-dd");
            Finance.SelectedDateTo = dateTo.ToString("yyyy-MM-dd");

            // Find periods overlapping the date range
            var monthPeriods = periods.Where(p =>
            {
                if (DateTime.TryParse(p.StartDate, out var start) && DateTime.TryParse(p.EndDate, out var end))
                    return start.Date <= dateTo && end.Date >= dateFrom;
                return false;
            }).ToList();

            // Sum up gross, deductions, net from payroll summaries across all matching periods
            foreach (var period in monthPeriods)
            {
                try
                {
                    var summaries = await _payroll.GetSummariesAsync(token, period.PayrollPeriodId);
                    Finance.GrossIncome += summaries.Sum(s => s.GrossPay);
                    Finance.TotalExpenses += summaries.Sum(s => s.TotalDeductions);
                    Finance.NetRevenue += summaries.Sum(s => s.NetPay);
                }
                catch { /* skip failed period */ }
            }

            // Load contributions as government taxes
            try
            {
                var contributions = await _payroll.GetContributionsAsync(token);
                Finance.GovernmentTaxes = contributions.Sum(c => c.EmployeeShare + c.EmployerShare);
            }
            catch { }
        }

        public class ActivityItem
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Time { get; set; } = "";
            public string Type { get; set; } = "info"; // info, success, warning, error, action
        }

        public class SuperAdminKpisData
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

        public class SuperAdminFinanceData
        {
            public decimal TotalIncome { get; set; }
            public decimal TotalDeductions { get; set; }
            public decimal TotalVat { get; set; }
            public decimal NetProfit { get; set; }
            public int EntryCount { get; set; }
        }

        private async Task LoadSuperAdminDataAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Fire all three HTTP requests in parallel
            var kpiTask = client.GetAsync("api/superadmin/dashboard");
            var financeTask = client.GetAsync("api/superadmin/finance/summary");
            var auditTask = client.GetAsync("api/superadmin/audit-logs?page=1&pageSize=10");

            try { await Task.WhenAll(kpiTask, financeTask, auditTask); } catch { /* individual results checked below */ }

            // Process KPIs
            try
            {
                var resp = await kpiTask;
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    SuperAdminKpis = JsonSerializer.Deserialize<SuperAdminKpisData>(json, jsonOpts) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load SuperAdmin KPIs"); }

            // Process finance summary
            try
            {
                var resp = await financeTask;
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    SuperAdminFinance = JsonSerializer.Deserialize<SuperAdminFinanceData>(json, jsonOpts) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load SuperAdmin finance summary"); }

            // Process recent audit logs as recent activity
            try
            {
                var resp = await auditTask;
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuditLogResponse>(json, jsonOpts);
                    var pht = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                    if (result?.Items != null)
                    {
                        foreach (var log in result.Items.Take(10))
                        {
                            var ts = TimeZoneInfo.ConvertTimeFromUtc(
                                DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc), pht);
                            var typeMap = log.Action?.ToLower() switch
                            {
                                "login" or "successful login" => "Success",
                                "failed login" => "error",
                                "registration" => "info",
                                "password reset" => "Warning",
                                _ => "info"
                            };
                            var timeAgo = (DateTime.UtcNow - log.Timestamp).TotalHours < 1 ? "Just now"
                                : (DateTime.UtcNow - log.Timestamp).TotalHours < 24 ? $"{(int)(DateTime.UtcNow - log.Timestamp).TotalHours}h ago"
                                : ts.ToString("MMM dd, yyyy");
                            RecentActivities.Add(new ActivityItem
                            {
                                Title = log.Action ?? "Activity",
                                Description = $"{log.UserName ?? log.UserEmail ?? "Unknown"}" + (log.CompanyName != null ? $" • {log.CompanyName}" : ""),
                                Time = timeAgo,
                                Type = typeMap
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load audit logs"); }

            if (!RecentActivities.Any())
                RecentActivities.Add(new ActivityItem { Title = "No recent activity", Description = "Activity will appear here as actions are performed", Time = "—", Type = "info" });
        }

        private class AuditLogResponse
        {
            public List<AuditLogItemDto>? Items { get; set; }
            public int TotalCount { get; set; }
        }

        private class AuditLogItemDto
        {
            public DateTime Timestamp { get; set; }
            public string? Action { get; set; }
            public string? UserEmail { get; set; }
            public string? UserName { get; set; }
            public string? CompanyName { get; set; }
        }
        public class PlanDto
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = "";
            public int MaxEmployees { get; set; }
            public decimal Price { get; set; }
            public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
            public string BillingCycle { get; set; } = "monthly";
        }
    }
}
