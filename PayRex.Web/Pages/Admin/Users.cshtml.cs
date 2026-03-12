using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRex.Web.QuestPdf;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
   private readonly ILogger<UsersModel> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _db;

    public List<UserItem> Users { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }

        public UsersModel(IHttpClientFactory httpClientFactory, ILogger<UsersModel> logger, IWebHostEnvironment env, AppDbContext db)
        {
     _httpClientFactory = httpClientFactory;
     _logger = logger;
            _env = env;
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
     if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

          var client = _httpClientFactory.CreateClient("PayRexApi");
     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

      try
   {
    var response = await client.GetAsync("api/superadmin/users");
   if (response.IsSuccessStatusCode)
       {
    var json = await response.Content.ReadAsStringAsync();
        Users = JsonSerializer.Deserialize<List<UserItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
   }
  }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin users"); }
  return Page();
 }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
                return RedirectToPage("/Auth/Login");

            var search = Request.Form["search"].ToString() ?? string.Empty;
            var roleFilter = Request.Form["role"].ToString() ?? string.Empty;
            var statusFilter = Request.Form["status"].ToString() ?? string.Empty;
            var companyFilter = Request.Form["company"].ToString() ?? string.Empty;

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/superadmin/users");
            var users = new List<UserItem>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                users = JsonSerializer.Deserialize<List<UserItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(u => ($"{u.FirstName} {u.LastName}").Contains(search, StringComparison.OrdinalIgnoreCase) || (u.Email ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(roleFilter))
                users = users.Where(u => string.Equals(u.Role, roleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                users = users.Where(u => u.Status == statusFilter).ToList();
            if (!string.IsNullOrWhiteSpace(companyFilter))
                users = users.Where(u => u.CompanyName == companyFilter).ToList();

            string issuerName = "Administrator";
            string issuerPosition = "Super Admin";
            string? issuerSignatureUrl = null;
            int issuingUserId = 0;
            int issuerCompanyId = 0;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var given = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                var family = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
                if (!string.IsNullOrEmpty(given))
                    issuerName = !string.IsNullOrEmpty(family) ? $"{given} {family}" : given;
                int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "uid")?.Value, out issuingUserId);
                int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "companyId")?.Value, out issuerCompanyId);
            }
            catch { }

            if (issuingUserId > 0)
                issuerSignatureUrl = await _db.Users.AsNoTracking()
                    .Where(u => u.UserId == issuingUserId)
                    .Select(u => u.SignatureUrl)
                    .FirstOrDefaultAsync();

            var issuerCompany = issuerCompanyId > 0
                ? await _db.Companies.AsNoTracking().Where(c => c.CompanyId == issuerCompanyId).FirstOrDefaultAsync()
                : null;

            var companyName    = issuerCompany?.CompanyName ?? "PayRex";
            var companyAddress = issuerCompany?.Address;
            var companyEmail   = issuerCompany?.ContactEmail;
            var companyPhone   = issuerCompany?.ContactPhone;

            var roleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Admin"] = "Administration",
                ["HR"] = "Human Resource Manager",
                ["Hr"] = "Human Resource Manager",
                ["Accountant"] = "Accountant",
                ["Employee"] = "Employee",
                ["SuperAdmin"] = "Super Admin"
            };

            var rows = users.Select(u => new UserExportRow
            {
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                Role = roleMap.TryGetValue(u.Role ?? "", out var rd) ? rd : (u.Role ?? "—"),
                Company = u.CompanyName ?? "—",
                Status = u.Status,
                CreatedAt = u.CreatedAt.ToString("MMM dd, yyyy")
            }).ToList();

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) parts.Add($"search: \"{search}\"");
            if (!string.IsNullOrWhiteSpace(roleFilter)) parts.Add($"role: {roleFilter}");
            if (!string.IsNullOrWhiteSpace(statusFilter)) parts.Add($"status: {statusFilter}");
            if (!string.IsNullOrWhiteSpace(companyFilter)) parts.Add($"company: {companyFilter}");
            var filterDesc = parts.Any() ? $"Filtered by {string.Join(", ", parts)}" : "All platform users";

            var generator = new UsersPdfGenerator();

            byte[]? logoBytes = null;
            if (!string.IsNullOrWhiteSpace(issuerCompany?.LogoUrl))
            {
                try
                {
                    using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    logoBytes = await http.GetByteArrayAsync(issuerCompany.LogoUrl);
                }
                catch { }
            }
            if (logoBytes == null)
            {
                try
                {
                    var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
                    if (System.IO.File.Exists(logoPath))
                        logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
                }
                catch { }
            }

            var pdfBytes = generator.Generate(new UsersPdfGeneratorOptions
            {
                IssuerName = issuerName,
                IssuerPosition = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                CompanyEmail = companyEmail,
                CompanyPhone = companyPhone,
                FilterDescription = filterDesc,
                Users = rows,
                LogoBytes = logoBytes
            });

            var filename = $"PayRex_Users_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int userId, string currentStatus)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newStatus = currentStatus == "Active" ? "Suspended" : "Active";
            var body = new StringContent(JsonSerializer.Serialize(new { status = newStatus }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/superadmin/users/{userId}/status", body);
            StatusMessage = response.IsSuccessStatusCode ? $"User status changed to {newStatus}" : "Failed to update user status";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync(int userId, string newRole)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new StringContent(JsonSerializer.Serialize(new { role = newRole }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/superadmin/users/{userId}/role", body);
            StatusMessage = response.IsSuccessStatusCode ? $"User role updated to {newRole}" : "Failed to update user role";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int userId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"api/superadmin/users/{userId}/reset-password", null);
            StatusMessage = response.IsSuccessStatusCode ? $"Password reset successfully (PayRex_{DateTime.Now.Year}!)" : "Failed to reset password";

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
            public int CompanyId { get; set; }
         public string? CompanyName { get; set; }
           public DateTime CreatedAt { get; set; }
               public string? ProfileImageUrl { get; set; }
            }
    }
}
