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
    public class CompaniesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
      private readonly ILogger<CompaniesModel> _logger;
      private readonly IWebHostEnvironment _env;
      private readonly AppDbContext _db;

      public List<CompanyItem> Companies { get; set; } = new();
 [TempData] public string? StatusMessage { get; set; }

     public CompaniesModel(IHttpClientFactory httpClientFactory, ILogger<CompaniesModel> logger, IWebHostEnvironment env, AppDbContext db)
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
                var response = await client.GetAsync("api/superadmin/companies");
       if (response.IsSuccessStatusCode)
     {
     var json = await response.Content.ReadAsStringAsync();
          Companies = JsonSerializer.Deserialize<List<CompanyItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
       }
    }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin companies"); }

  return Page();
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
                return RedirectToPage("/Auth/Login");

            var search = Request.Form["search"].ToString() ?? string.Empty;
            var planFilter = Request.Form["plan"].ToString() ?? string.Empty;
            var statusFilter = Request.Form["status"].ToString() ?? string.Empty;

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/superadmin/companies");
            var companies = new List<CompanyItem>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                companies = JsonSerializer.Deserialize<List<CompanyItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            if (!string.IsNullOrWhiteSpace(search))
                companies = companies.Where(c => (c.CompanyName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) || (c.CompanyCode ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(planFilter))
                companies = companies.Where(c => c.PlanName == planFilter).ToList();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                companies = companies.Where(c => ((c.IsActive || c.Status == "Active") ? "Active" : "Inactive") == statusFilter).ToList();

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

            var rows = companies.Select(c => new CompanyExportRow
            {
                CompanyName = c.CompanyName,
                CompanyCode = c.CompanyCode,
                PlanName = string.IsNullOrWhiteSpace(c.PlanName) ? "—" : c.PlanName,
                UserCount = c.UserCount,
                EmployeeCount = c.EmployeeCount,
                Status = (c.IsActive || c.Status == "Active") ? "Active" : "Inactive",
                CreatedAt = c.CreatedAt.ToString("MMM dd, yyyy")
            }).ToList();

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) parts.Add($"search: \"{search}\"");
            if (!string.IsNullOrWhiteSpace(planFilter)) parts.Add($"plan: {planFilter}");
            if (!string.IsNullOrWhiteSpace(statusFilter)) parts.Add($"status: {statusFilter}");
            var filterDesc = parts.Any() ? $"Filtered by {string.Join(", ", parts)}" : "All registered companies";

            var generator = new CompaniesPdfGenerator();

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

            var pdfBytes = generator.Generate(new CompaniesPdfGeneratorOptions
            {
                IssuerName = issuerName,
                IssuerPosition = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                CompanyEmail = companyEmail,
                CompanyPhone = companyPhone,
                FilterDescription = filterDesc,
                Companies = rows,
                LogoBytes = logoBytes
            });

            var filename = $"PayRex_Companies_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int companyId, bool isActive)
        {
    if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

var client = _httpClientFactory.CreateClient("PayRexApi");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

  var newActive = !isActive;
  var body = new StringContent(JsonSerializer.Serialize(new { isActive = newActive }), Encoding.UTF8, "application/json");

   var response = await client.PostAsync($"api/superadmin/companies/{companyId}/status", body);
  StatusMessage = response.IsSuccessStatusCode
    ? $"Company {(newActive ? "activated" : "deactivated")} successfully. Associated users were also updated."
          : "Failed to update company status";

     return RedirectToPage();
        }

   public class CompanyItem
        {
       public int CompanyId { get; set; }
       public string CompanyCode { get; set; } = "";
     public string CompanyName { get; set; } = "";
   public string Status { get; set; } = "";
    public bool IsActive { get; set; }
        public string PlanName { get; set; } = "";
       public int UserCount { get; set; }
   public int EmployeeCount { get; set; }
 public DateTime CreatedAt { get; set; }
        public string? LogoUrl { get; set; }
   }
    }
}
