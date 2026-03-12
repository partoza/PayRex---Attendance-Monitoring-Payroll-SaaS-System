using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRex.Web.QuestPdf;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class BillingModel : PageModel
 {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BillingModel> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

  public List<BillingItem> Invoices { get; set; } = new();
 public List<PlanItem> Plans { get; set; } = new();

    public BillingModel(IHttpClientFactory httpClientFactory, ILogger<BillingModel> logger, IWebHostEnvironment env, AppDbContext db)
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
          var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

   try
            {
                var billingTask = client.GetAsync("api/superadmin/billing");
  var plansTask = client.GetAsync("api/superadmin/plans");
await Task.WhenAll(billingTask, plansTask);

  if (billingTask.Result.IsSuccessStatusCode)
                Invoices = JsonSerializer.Deserialize<List<BillingItem>>(await billingTask.Result.Content.ReadAsStringAsync(), opts) ?? new();

     if (plansTask.Result.IsSuccessStatusCode)
         Plans = JsonSerializer.Deserialize<List<PlanItem>>(await plansTask.Result.Content.ReadAsStringAsync(), opts) ?? new();
  }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin billing"); }

   return Page();
  }

        public async Task<IActionResult> OnPostExportInvoicesPdfAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
                return RedirectToPage("/Auth/Login");

            var search = Request.Form["exportSearch"].ToString() ?? string.Empty;
            var statusFilter = Request.Form["exportStatus"].ToString() ?? string.Empty;
            var dateFrom = Request.Form["exportDateFrom"].ToString() ?? string.Empty;
            var dateTo = Request.Form["exportDateTo"].ToString() ?? string.Empty;

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var invoices = new List<BillingItem>();
            try
            {
                var response = await client.GetAsync("api/superadmin/billing");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    invoices = JsonSerializer.Deserialize<List<BillingItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error fetching invoices for PDF export"); }

            if (!string.IsNullOrWhiteSpace(search))
                invoices = invoices.Where(i =>
                    i.InvoiceNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(statusFilter))
                invoices = invoices.Where(i => i.Status == statusFilter).ToList();

            if (DateTime.TryParse(dateFrom, out var fromDt))
                invoices = invoices.Where(i => i.CreatedAt.Date >= fromDt.Date).ToList();

            if (DateTime.TryParse(dateTo, out var toDt))
                invoices = invoices.Where(i => i.CreatedAt.Date <= toDt.Date).ToList();

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

            var rows = invoices.Select(i => new BillingInvoiceExportRow
            {
                InvoiceNumber = i.InvoiceNumber,
                CompanyName = i.CompanyName,
                Amount = $"\u20b1{i.Amount:N2}",
                VatAmount = $"\u20b1{i.VatAmount:N2}",
                Total = $"\u20b1{(i.Amount + i.VatAmount):N2}",
                Status = i.Status,
                CreatedAt = i.CreatedAt.ToString("MMM dd, yyyy"),
                PeriodStart = i.PeriodStart.HasValue ? i.PeriodStart.Value.ToString("MMM dd, yyyy") : "—",
                DueDate = i.DueDate.ToString("MMM dd, yyyy")
            }).ToList();

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) parts.Add($"search: \"{search}\"");
            if (!string.IsNullOrWhiteSpace(statusFilter)) parts.Add($"status: {statusFilter}");
            if (!string.IsNullOrWhiteSpace(dateFrom)) parts.Add($"from: {dateFrom}");
            if (!string.IsNullOrWhiteSpace(dateTo)) parts.Add($"to: {dateTo}");
            var filterDesc = parts.Any() ? $"Filtered by {string.Join(", ", parts)}" : "All invoices";

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

            var generator = new BillingInvoicePdfGenerator();
            var pdfBytes = generator.Generate(new BillingInvoicePdfGeneratorOptions
            {
                IssuerName = issuerName,
                IssuerPosition = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                CompanyEmail = companyEmail,
                CompanyPhone = companyPhone,
                FilterDescription = filterDesc,
                Invoices = rows,
                LogoBytes = logoBytes
            });

            var filename = $"PayRex_Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
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
     public DateTime? PeriodStart { get; set; }
     public DateTime? PeriodEnd { get; set; }
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
    }
}
