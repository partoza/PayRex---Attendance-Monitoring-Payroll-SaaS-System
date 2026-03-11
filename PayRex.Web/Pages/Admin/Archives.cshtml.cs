using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class ArchivesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ArchivesModel> _logger;

        [TempData] public string? StatusMessage { get; set; }

        public List<UserItem> InactiveUsers { get; set; } = new();
        public List<CompanyItem> InactiveCompanies { get; set; } = new();
        public List<BillingItem> ArchivedInvoices { get; set; } = new();

        public ArchivesModel(IHttpClientFactory httpClientFactory, ILogger<ArchivesModel> logger)
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
                var usersResp = await client.GetAsync("api/superadmin/users/all");
                if (usersResp.IsSuccessStatusCode)
                {
                    var json = await usersResp.Content.ReadAsStringAsync();
                    var allUsers = JsonSerializer.Deserialize<List<UserItem>>(json, opts) ?? new();
                    InactiveUsers = allUsers.Where(u => !string.Equals(u.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var compResp = await client.GetAsync("api/superadmin/companies/all");
                if (compResp.IsSuccessStatusCode)
                {
                    var json = await compResp.Content.ReadAsStringAsync();
                    var allCompanies = JsonSerializer.Deserialize<List<CompanyItem>>(json, opts) ?? new();
                    InactiveCompanies = allCompanies.Where(c => !c.IsActive).ToList();
                }

                var billingResp = await client.GetAsync("api/superadmin/billing/archived");
                if (billingResp.IsSuccessStatusCode)
                {
                    var json = await billingResp.Content.ReadAsStringAsync();
                    ArchivedInvoices = JsonSerializer.Deserialize<List<BillingItem>>(json, opts) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error loading archives"); }

            return Page();
        }

        public async Task<IActionResult> OnPostRestoreUserAsync(int userId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { status = "Active" };
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"api/superadmin/users/{userId}/status", body);
            StatusMessage = response.IsSuccessStatusCode ? "User restored successfully" : "Failed to restore user";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreCompanyAsync(int companyId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { isActive = true };
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"api/superadmin/companies/{companyId}/status", body);
            StatusMessage = response.IsSuccessStatusCode ? "Company restored successfully" : "Failed to restore company";

            return RedirectToPage();
        }

        public class UserItem
        {
            public int UserId { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public string Status { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string? ProfileImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class CompanyItem
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = "";
            public string Status { get; set; } = "";
            public bool IsActive { get; set; }
            public string PlanName { get; set; } = "";
            public int UserCount { get; set; }
            public int EmployeeCount { get; set; }
            public string? LogoUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class BillingItem
        {
            public int InvoiceId { get; set; }
            public string InvoiceNumber { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
            public string Status { get; set; } = "";
            public DateTime DueDate { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
