using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PayRex.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IAuthApiService _authApiService;
        private readonly ILogger<DashboardModel> _logger;

        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string? UserId { get; set; }
        public string? UserFirstName { get; set; }
        
        public DashboardStats Stats { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();

        public DashboardModel(IAuthApiService authApiService, ILogger<DashboardModel> logger)
        {
            _authApiService = authApiService;
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

                UserEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                UserId = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                UserRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                UserFirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value ?? "User";

                // Load appropriate dashboard data based on role
                if (UserRole == "Employee")
                {
                    Stats = new DashboardStats
                    {
                        PresentDays = 22,
                        LateDays = 2,
                        LeaveBalance = 15,
                        EstimatedPay = 42500.00m
                    };
                    
                    RecentActivities = new List<ActivityItem>
                    {
                        new ActivityItem { Title = "Payslip Generated", Description = "Your payslip for Jan 16-31 is available", Time = "2 days ago", Type = "success" },
                        new ActivityItem { Title = "Attendance Logged", Description = "Time-in at 8:58 AM", Time = "5 hours ago", Type = "info" },
                        new ActivityItem { Title = "Leave Approved", Description = "Vacation leave for Feb 25 approved", Time = "1 week ago", Type = "success" }
                    };
                }
                else // Admin or HR
                {
                    Stats = new DashboardStats
                    {
                        TotalEmployees = 142,
                        NewHires = 12,
                        PresentToday = 128,
                        OnTimePercentage = 92,
                        PendingPayroll = 2,
                        PayrollCost = 3250000.00m
                    };

                    RecentActivities = new List<ActivityItem>
                    {
                        new ActivityItem { Title = "New Employee Added", Description = "Maria Santos joined as Senior Accountant", Time = "2 hours ago", Type = "info" },
                        new ActivityItem { Title = "Payroll Generated", Description = "Period Feb 1-15 payroll draft created", Time = "4 hours ago", Type = "warning" },
                        new ActivityItem { Title = "Leave Request", Description = "Juan Cruz requested Sick Leave for Feb 12", Time = "Yesterday", Type = "action" },
                        new ActivityItem { Title = "System Update", Description = "Tax rates updated seamlessly", Time = "3 days ago", Type = "success" }
                    };
                }

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

        public class ActivityItem
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Time { get; set; } = "";
            public string Type { get; set; } = "info"; // info, success, warning, error, action
        }
    }
}
