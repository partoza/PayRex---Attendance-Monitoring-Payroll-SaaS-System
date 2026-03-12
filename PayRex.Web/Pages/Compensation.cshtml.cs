using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRex.Web.Services;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using System.IdentityModel.Tokens.Jwt;
using PayRex.Web.QuestPdf;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,Accountant")]
    [IgnoreAntiforgeryToken]
    public class CompensationModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public List<DeductionDto> Deductions { get; set; } = new();
        public List<BenefitDto> Benefits { get; set; } = new();
        public List<EmployeeListItem> Employees { get; set; } = new();

        [TempData] public string? StatusMessage { get; set; }

        public CompensationModel(IPayrollApiService payroll, AppDbContext db, IWebHostEnvironment env)
        {
            _payroll = payroll;
            _db = db;
            _env = env;
        }

        private int GetCompanyId()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return 0;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var claim = jwt.Claims.FirstOrDefault(c => c.Type == "companyId");
                return claim != null && int.TryParse(claim.Value, out var cid) ? cid : 0;
            }
            catch { return 0; }
        }

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var companyId = GetCompanyId();

            var compensationTask = _payroll.GetCompensationAsync(token);
            Task<List<EmployeeListItem>> employeesTask = companyId > 0
                ? _db.Employees
                    .AsNoTracking()
                    .Where(e => e.CompanyId == companyId && e.Status == EmployeeStatus.Active)
                    .Select(e => new EmployeeListItem { Id = e.EmployeeNumber, Name = e.FirstName + " " + e.LastName })
                    .OrderBy(e => e.Name)
                    .ToListAsync()
                : Task.FromResult(new List<EmployeeListItem>());

            await Task.WhenAll(compensationTask, employeesTask);

            var result = compensationTask.Result;
            Deductions = result.Deductions;
            Benefits = result.Benefits;
            Employees = employeesTask.Result;
        }

        public async Task<IActionResult> OnPostAddDeductionAsync(int employeeId, string name, decimal amount, bool isRecurring)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var res = await _payroll.AddDeductionAsync(token, new { EmployeeId = employeeId, Name = name, Amount = amount, IsRecurring = isRecurring });
            StatusMessage = res.success ? $"success:{res.message}" : $"error:{res.message}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddBenefitAsync(int employeeId, string name, decimal amount)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var res = await _payroll.AddBenefitAsync(token, new { EmployeeId = employeeId, Name = name, Amount = amount });
            StatusMessage = res.success ? $"success:{res.message}" : $"error:{res.message}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var type = Request.Form["exportType"].ToString(); // "deduction" or "benefit"
            
            var result = await _payroll.GetCompensationAsync(token);
            var generator = new CompensationPdfGenerator();
            
            byte[]? logoBytes = null;
            try
            {
                var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                    logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
            }
            catch { }

            string issuerName = "Administrator";
            string issuerPosition = "Admin";
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var given = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                var family = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
                if (!string.IsNullOrEmpty(given))
                    issuerName = !string.IsNullOrEmpty(family) ? $"{given} {family}" : given;
                
                var role = jwt.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                if (!string.IsNullOrEmpty(role)) issuerPosition = role;
            }
            catch { }

            CompensationPdfGeneratorOptions opts;
            if (type == "deduction")
            {
                opts = new CompensationPdfGeneratorOptions
                {
                    Title = "Deductions & Penalties Report",
                    IsDeduction = true,
                    IssuerName = issuerName,
                    IssuerPosition = issuerPosition,
                    LogoBytes = logoBytes,
                    Rows = result.Deductions.Select(d => new CompensationExportRow
                    {
                        EmployeeName = d.EmployeeName,
                        Type = d.Type,
                        Amount = d.Amount,
                        DateOrFrequency = d.CreatedAt.ToShortDateString(),
                        Status = d.IsActive ? "Active" : "Inactive"
                    }).ToList()
                };
            }
            else
            {
                opts = new CompensationPdfGeneratorOptions
                {
                    Title = "Allowances & Benefits Report",
                    IsDeduction = false,
                    IssuerName = issuerName,
                    IssuerPosition = issuerPosition,
                    LogoBytes = logoBytes,
                    Rows = result.Benefits.Select(b => new CompensationExportRow
                    {
                        EmployeeName = b.EmployeeName,
                        Type = b.Type,
                        Amount = b.Amount,
                        DateOrFrequency = b.IsActive ? "Recurring" : "One-time",
                        Status = b.IsActive ? "Active" : "Inactive"
                    }).ToList()
                };
            }

            var pdfBytes = generator.Generate(opts);
            var filename = $"PayRex_{type}_{DateTime.Now:yyyyMMdd}.pdf";
            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
        }

        public class EmployeeListItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}
