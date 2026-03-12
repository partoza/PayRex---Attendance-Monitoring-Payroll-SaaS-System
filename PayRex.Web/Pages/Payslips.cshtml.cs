using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.DTOs;
using PayRex.Web.QuestPdf;
using PayRex.Web.Services;
using System.IdentityModel.Tokens.Jwt;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Accountant,Employee")]
    public class PayslipsModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        private readonly IAuthApiService _auth;
        public List<PayslipDto> Payslips { get; set; } = new();
        public CompanyProfileDto? Company { get; set; }
        [TempData] public string? StatusMessage { get; set; }

        public PayslipsModel(IPayrollApiService payroll, IAuthApiService auth)
        {
            _payroll = payroll;
            _auth = auth;
        }

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            Company = await _auth.GetCompanyProfileAsync(token);
            var isEmployeeView = Request.Cookies["PayRex.ViewMode"] == "employee";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var selfOnly = isEmployeeView || role == "Employee";
            Payslips = await _payroll.GetPayslipsAsync(token, selfOnly);
        }

        public async Task<IActionResult> OnPostArchivePeriodAsync(string periodName)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            if (string.IsNullOrWhiteSpace(periodName)) { StatusMessage = "Period name is required."; return RedirectToPage(); }
            var (ok, msg) = await _payroll.ArchivePayslipsByPeriodAsync(token, periodName);
            StatusMessage = ok ? $"Period \"{periodName}\" archived successfully." : (msg ?? "Archive failed.");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportPayslipsPdfAsync(string? periodFilter)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            Company = await _auth.GetCompanyProfileAsync(token);
            var isEmployeeView = Request.Cookies["PayRex.ViewMode"] == "employee";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var selfOnly = isEmployeeView || role == "Employee";
            var allSlips = await _payroll.GetPayslipsAsync(token, selfOnly);

            var filtered = !string.IsNullOrWhiteSpace(periodFilter)
                ? allSlips.Where(p => p.PeriodName == periodFilter).ToList()
                : allSlips;

            // Resolve issuer name from JWT claims
            var givenName  = User.FindFirst("given_name")?.Value ?? "";
            var familyName = User.FindFirst("family_name")?.Value ?? "";
            var issuerName = $"{givenName} {familyName}".Trim();
            if (string.IsNullOrWhiteSpace(issuerName)) issuerName = User.Identity?.Name ?? "Administrator";

            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(Company?.LogoUrl))
            {
                try
                {
                    using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                    logoBytes = await http.GetByteArrayAsync(Company.LogoUrl);
                }
                catch { /* ignore */ }
            }

            var rows = filtered.Select(s => new PayslipReportRow
            {
                EmployeeName   = s.EmployeeName,
                Period         = s.PeriodName ?? "",
                BasicPay       = $"₱{(s.BasicPay ?? 0):N2}",
                GrossPay       = $"₱{s.GrossPay:N2}",
                TotalDeductions = $"₱{s.TotalDeductions:N2}",
                NetPay         = $"₱{s.NetPay:N2}",
                Status         = "Paid"
            }).ToList();

            var opts = new PayslipReportOptions
            {
                CompanyName    = Company?.CompanyName ?? "PayRex Inc.",
                CompanyAddress = Company?.Address,
                PeriodFilter   = string.IsNullOrWhiteSpace(periodFilter) ? "All Periods" : periodFilter,
                IssuerName     = issuerName,
                IssuerPosition = role,
                Rows           = rows,
                LogoBytes      = logoBytes
            };

            var gen  = new PayslipReportPdfGenerator();
            var pdf  = gen.Generate(opts);
            var fileName = $"Payslips_{(string.IsNullOrWhiteSpace(periodFilter) ? "All" : periodFilter.Replace(" ","_"))}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }
}
