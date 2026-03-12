using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRex.Web.QuestPdf;
using System.IdentityModel.Tokens.Jwt;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    [IgnoreAntiforgeryToken]
    public class FinanceModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FinanceModel> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public List<FinanceEntryItem> Entries { get; set; } = new();
        public FinanceSummaryItem Summary { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }
        [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

        public FinanceModel(IHttpClientFactory httpClientFactory, ILogger<FinanceModel> logger, IWebHostEnvironment env, AppDbContext db)
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
                var qs = new List<string>();
                if (!string.IsNullOrEmpty(FromDate)) qs.Add($"fromDate={Uri.EscapeDataString(FromDate)}");
                if (!string.IsNullOrEmpty(ToDate)) qs.Add($"toDate={Uri.EscapeDataString(ToDate)}");
                var qsStr = qs.Count > 0 ? "?" + string.Join("&", qs) : "";

                var entriesTask = client.GetAsync($"api/superadmin/finance{qsStr}");
                var summaryTask = client.GetAsync($"api/superadmin/finance/summary{qsStr}");
                await Task.WhenAll(entriesTask, summaryTask);

                if (entriesTask.Result.IsSuccessStatusCode)
                    Entries = JsonSerializer.Deserialize<List<FinanceEntryItem>>(await entriesTask.Result.Content.ReadAsStringAsync(), opts) ?? new();

                if (summaryTask.Result.IsSuccessStatusCode)
                    Summary = JsonSerializer.Deserialize<FinanceSummaryItem>(await summaryTask.Result.Content.ReadAsStringAsync(), opts) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error loading finance data"); }

            return Page();
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var qs = new List<string>();
            if (!string.IsNullOrEmpty(FromDate)) qs.Add($"fromDate={Uri.EscapeDataString(FromDate)}");
            if (!string.IsNullOrEmpty(ToDate)) qs.Add($"toDate={Uri.EscapeDataString(ToDate)}");
            var qsStr = qs.Count > 0 ? "?" + string.Join("&", qs) : "";

            var entriesResp = await client.GetAsync($"api/superadmin/finance{qsStr}");
            var summaryResp = await client.GetAsync($"api/superadmin/finance/summary{qsStr}");

            var entries = entriesResp.IsSuccessStatusCode 
                ? JsonSerializer.Deserialize<List<FinanceEntryItem>>(await entriesResp.Content.ReadAsStringAsync(), jsonOpts) ?? new()
                : new();
            
            var summary = summaryResp.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<FinanceSummaryItem>(await summaryResp.Content.ReadAsStringAsync(), jsonOpts) ?? new()
                : new();

            // Forecasting Logic
            string forecastText;
            string summaryConclusion;

            if (summary.NetProfit > 0)
            {
                var projected = summary.NetProfit * 1.15m;
                forecastText = $"Based on the current net profit of ₱{summary.NetProfit:N2}, we project a 15% growth for the next period, estimated at ₱{projected:N2}. This trend indicates a strong market position and efficient cost management.";
                summaryConclusion = "The platform is currently operating with a healthy profit margin. Continued focus on expanding the user base while maintaining current cost levels will further strengthen the financial outlook.";
            }
            else if (summary.NetProfit < 0)
            {
                forecastText = "Current data shows a net loss. A recovery plan is projected to reach break-even within the next quarter through a targeted 20% reduction in operational expenditures and optimized resource allocation.";
                summaryConclusion = "Financial performance is currently below targets. Immediate attention to cost-scaling and review of high-expense categories is recommended to restore profitability.";
            }
            else
            {
                forecastText = "Financial stability is maintained. Future projections remain pending further market developments and expansion of service offerings.";
                summaryConclusion = "The platform is currently at break-even. Efficiency improvements in operational processes could shift the performance towards a positive margin.";
            }

            // Identification
            string issuerName = "Administrator";
            string issuerPosition = "Super Admin";
            string? issuerSignatureUrl = null;
            int issuingUserId = 0;
            int issuerCompanyId = 0;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var given = jwt.Claims.FirstOrDefault(c => c.Type == "given_name" || c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                var family = jwt.Claims.FirstOrDefault(c => c.Type == "family_name" || c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
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

            // Logo: Cloudinary first, local fallback
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

            var generator = new FinancePdfGenerator();
            var pdfOptions = new FinancePdfGeneratorOptions
            {
                CompanyName = companyName,
                CompanyTagline = "Financial Management & Payroll Platform",
                CompanyAddress = companyAddress,
                CompanyPhone = companyPhone,
                CompanyEmail = companyEmail,
                Period = (string.IsNullOrEmpty(FromDate) && string.IsNullOrEmpty(ToDate)) ? "All Time" : $"{FromDate ?? "..."} to {ToDate ?? "..."}",
                TotalIncome = summary.TotalIncome,
                TotalCost = summary.TotalDeductions,
                TotalVat = summary.TotalVat,
                NetProfit = summary.NetProfit,
                ForecastText = forecastText,
                SummaryConclusion = summaryConclusion,
                IssuerName = issuerName,
                IssuerPosition = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                LogoBytes = logoBytes,
                Entries = entries.Select(e => new FinanceExportRow
                {
                    Date = e.CreatedAt.ToString("MMM dd, yyyy"),
                    Type = e.Type,
                    Description = e.Description,
                    Category = e.Category ?? "—",
                    Amount = e.Amount,
                    Vat = e.VatAmount
                }).ToList()
            };

            var pdfBytes = generator.Generate(pdfOptions);
            return new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"PayRex_Finance_Report_{DateTime.Now:yyyyMMdd}.pdf"
            };
        }

        public async Task<IActionResult> OnPostAddEntryAsync(string type, string description, decimal amount, string? category, string? reference)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new StringContent(JsonSerializer.Serialize(new { type, description, amount, category, reference }), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/superadmin/finance", body);
            var message = response.IsSuccessStatusCode ? "Finance entry added successfully" : "Failed to add finance entry";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return new JsonResult(new { success = response.IsSuccessStatusCode, message });

            StatusMessage = message;
            return RedirectToPage();
        }

        public class FinanceEntryItem
        {
            public int EntryId { get; set; }
            public string Type { get; set; } = "";
            public string Description { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
            public string? Category { get; set; }
            public string? Reference { get; set; }
            public bool IsAutoGenerated { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class FinanceSummaryItem
        {
            public decimal TotalIncome { get; set; }
            public decimal TotalDeductions { get; set; }
            public decimal TotalVat { get; set; }
            public decimal NetProfit { get; set; }
            public int EntryCount { get; set; }
        }
    }
}
