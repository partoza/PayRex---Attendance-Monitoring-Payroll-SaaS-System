using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class SettingsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SettingsModel> _logger;

        [TempData] public string? StatusMessage { get; set; }

        public List<PlanItem> Plans { get; set; } = new();
        public List<NotificationItem> SystemNotifications { get; set; } = new();

        public SettingsModel(IHttpClientFactory httpClientFactory, ILogger<SettingsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                var planResp = await client.GetAsync("api/superadmin/plans");
                if (planResp.IsSuccessStatusCode)
                {
                    var json = await planResp.Content.ReadAsStringAsync();
                    Plans = JsonSerializer.Deserialize<List<PlanItem>>(json, opts) ?? new();
                }

                var notifResp = await client.GetAsync("api/superadmin/system-notifications");
                if (notifResp.IsSuccessStatusCode)
                {
                    var json = await notifResp.Content.ReadAsStringAsync();
                    SystemNotifications = JsonSerializer.Deserialize<List<NotificationItem>>(json, opts) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error loading system settings"); }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePlanAsync(int planId, string name, decimal price, int maxEmployees, string? description)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { name, price, maxEmployees, description };
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/superadmin/plans/{planId}", body);
            StatusMessage = response.IsSuccessStatusCode ? "Plan updated successfully" : "Failed to update plan";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddNotificationAsync(string title, string message, string? type, string? targetRoles)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { title, message, type = type ?? "info", targetRoles };
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/superadmin/system-notifications", body);
            StatusMessage = response.IsSuccessStatusCode ? "Notification created successfully" : "Failed to create notification";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleNotificationAsync(int notificationId, bool isActive)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { isActive };
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/superadmin/system-notifications/{notificationId}/toggle", body);
            StatusMessage = response.IsSuccessStatusCode ? "Notification updated" : "Failed to update notification";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteNotificationAsync(int notificationId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/superadmin/system-notifications/{notificationId}");
            StatusMessage = response.IsSuccessStatusCode ? "Notification deleted" : "Failed to delete notification";

            return RedirectToPage();
        }

        public class PlanItem
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = "";
            public int MaxEmployees { get; set; }
            public decimal Price { get; set; }
            public string BillingCycle { get; set; } = "";
            public string Status { get; set; } = "";
            public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
        }

        public class NotificationItem
        {
            public int NotificationId { get; set; }
            public string Title { get; set; } = "";
            public string Message { get; set; } = "";
            public string Type { get; set; } = "info";
            public string? TargetRoles { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
