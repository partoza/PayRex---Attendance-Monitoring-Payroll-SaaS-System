using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.Enums;
using PayRex.Web.QuestPdf;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeesModel : PageModel
    {
        private readonly AppDbContext _db;

        public EmployeesModel(AppDbContext db)
        {
            _db = db;
        }

        public List<EmployeeItem> Employees { get; set; } = new();
        public List<RoleItem> Roles { get; set; } = new();

        private int GetCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var cid) ? cid : 0;
        }

        public async Task OnGetAsync()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return;

            var items = await _db.Employees
                .Include(e => e.Role)
                .Include(e => e.Company)
                .Include(e => e.User)
                .Where(e => e.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();

            Employees = items.Where(e => e.Status == EmployeeStatus.Active).Select(e => new EmployeeItem
            {
                Id = string.IsNullOrWhiteSpace(e.EmployeeCode) ? e.EmployeeNumber.ToString() : e.EmployeeCode,
                EmployeeNumber = e.EmployeeNumber,
                Name = (e.FirstName + " " + e.LastName).Trim(),
                Position = e.Role?.RoleName ?? "",
                Department = e.Company?.CompanyName ?? "",
                BasicRate = e.Role?.BasicRate ?? 0m,
                PayType = e.Role?.RateType ?? "",
                EmploymentStatus = e.Status.ToString(),
                DateHired = e.StartDate,
                Email = e.Email,
                ContactNumber = e.ContactNumber,
                CivilStatus = e.CivilStatus,
                Birthdate = e.Birthdate,
                Tin = e.TIN,
                Sss = e.SSS,
                PhilHealth = e.PhilHealth,
                PagIbig = e.PagIbig,
                ProfileImageUrl = e.User?.ProfileImageUrl,
                SignatureUrl = e.User?.SignatureUrl
            }).ToList();

            // Load roles for filters (active roles for this company only)
            var roles = await _db.EmployeeRoles
                .Where(r => r.IsActive && r.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();

            Roles = roles.Select(r => new RoleItem
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                BasicRate = r.BasicRate ?? 0m
            }).OrderBy(r => r.RoleName).ToList();
        }

        public class EmployeeItem
        {
            public int EmployeeNumber { get; set; }
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Position { get; set; } = "";
            public string Department { get; set; } = "";
            public decimal BasicRate { get; set; }
            public string PayType { get; set; } = "";
            public string EmploymentStatus { get; set; } = "";
            public DateTime DateHired { get; set; }
            public string Email { get; set; } = "";
            public string? ContactNumber { get; set; }
            public string? CivilStatus { get; set; }
            public DateTime? Birthdate { get; set; }
            public string? Tin { get; set; }
            public string? Sss { get; set; }
            public string? PhilHealth { get; set; }
            public string? PagIbig { get; set; }
            public string? ProfileImageUrl { get; set; }
            public string? SignatureUrl { get; set; }
        }

        public class RoleItem
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public decimal BasicRate { get; set; }
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            // Read filters from form
            var search = Request.Form["search"].ToString() ?? string.Empty;
            var position = Request.Form["position"].ToString() ?? string.Empty;
            var salaryMin = decimal.TryParse(Request.Form["salaryMin"], out var smin) ? smin : 0m;
            var salaryMax = decimal.TryParse(Request.Form["salaryMax"], out var smax) ? smax : decimal.MaxValue;
            var includeSalary = Request.Form["includeSalary"] == "true";
            var includeGovIds = Request.Form["includeGovIds"] == "true";

            // Scope to this admin's company only
            var exportCompanyId = GetCompanyId();

            // Query employees with role/company
            var query = _db.Employees.Include(e => e.Role).Include(e => e.Company)
                .Where(e => e.CompanyId == exportCompanyId)
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => (e.FirstName + " " + e.LastName).Contains(search) || e.Email.Contains(search));
                
            string activeRoleFilter = "";
            if (!string.IsNullOrWhiteSpace(position))
            {
                query = query.Where(e => e.Role != null && e.Role.RoleName == position);
                activeRoleFilter = position;
            }
                
            string activeSalaryFilter = "";
            bool hasMinSalary = salaryMin > 0m;
            bool hasMaxSalary = salaryMax < decimal.MaxValue;
            if (hasMinSalary && hasMaxSalary)
                activeSalaryFilter = $"a basic salary between ₱{salaryMin:N2} and ₱{salaryMax:N2}";
            else if (hasMinSalary)
                activeSalaryFilter = $"a basic salary of ₱{salaryMin:N2} and above";
            else if (hasMaxSalary)
                activeSalaryFilter = $"a basic salary up to ₱{salaryMax:N2}";
                
            query = query.Where(e => (e.Role != null ? (e.Role.BasicRate ?? 0m) : 0m) >= salaryMin
                                  && (e.Role != null ? (e.Role.BasicRate ?? 0m) : 0m) <= salaryMax);

            var list = await query.Select(e => new {
                IdCode = string.IsNullOrWhiteSpace(e.EmployeeCode) ? e.EmployeeNumber.ToString() : e.EmployeeCode,
                Name = (e.FirstName + " " + e.LastName).Trim(),
                Position = e.Role != null ? e.Role.RoleName : "",
                Salary = e.Role != null ? (e.Role.BasicRate ?? 0m) : 0m,
                PayType = e.Role != null ? e.Role.RateType : "",
                Email = e.Email,
                Contact = e.ContactNumber,
                Birthdate = e.Birthdate,
                CivilStatus = e.CivilStatus,
                StartDate = e.StartDate,
                Tin = e.TIN,
                Sss = e.SSS,
                PhilHealth = e.PhilHealth,
                PagIbig = e.PagIbig
            }).ToListAsync();

            // Fetch full company details for the report
            var company = await _db.Companies
                .AsNoTracking()
                .Where(c => c.CompanyId == exportCompanyId)
                .FirstOrDefaultAsync();

            var companyName    = company?.CompanyName ?? "Company";
            var companyAddress = company?.Address;
            var companyEmail   = company?.ContactEmail;
            var companyPhone   = company?.ContactPhone;
            var companyLogoUrl = company?.LogoUrl;

            // Get issuer name, position, and signature URL from JWT + DB
            string issuerName      = "User";
            string issuerPosition  = User.FindFirst(ClaimTypes.Role)?.Value ?? "Staff";
            string? issuerSignatureUrl = null;
            int issuingUserId      = 0;

            if (Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    var given  = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                    var family = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
                    if (!string.IsNullOrEmpty(given))
                        issuerName = !string.IsNullOrEmpty(family) ? $"{given} {family}" : given;
                    int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "uid")?.Value, out issuingUserId);
                }
                catch { }
            }

            // Load issuer's signature URL directly from DB
            if (issuingUserId > 0)
            {
                issuerSignatureUrl = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == issuingUserId)
                    .Select(u => u.SignatureUrl)
                    .FirstOrDefaultAsync();
            }

            var filename = $"{companyName}_EmployeeRecords_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            var generator = new EmployeePdfGenerator();
            var records = list.Select(e => new EmployeeRecord
            {
                IdCode = e.IdCode,
                Name = e.Name,
                Position = e.Position,
                Email = e.Email,
                Contact = e.Contact,
                Age = e.Birthdate.HasValue ? (int)((DateTime.Now - e.Birthdate.Value).TotalDays / 365.2425) : (int?)null,
                CivilStatus = e.CivilStatus,
                StartDate = e.StartDate,
                Salary = e.Salary,
                PayType = e.PayType ?? "",
                Tin = e.Tin,
                Sss = e.Sss,
                PhilHealth = e.PhilHealth,
                PagIbig = e.PagIbig
            }).ToList();

            var pdfBytes = generator.Generate(new EmployeePdfGeneratorOptions
            {
                CompanyName      = companyName,
                CompanyLogoUrl   = companyLogoUrl,
                CompanyAddress   = companyAddress,
                CompanyEmail     = companyEmail,
                CompanyPhone     = companyPhone,
                IssuerName       = issuerName,
                IssuerPosition   = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                IncludeSalary    = includeSalary,
                IncludeGovIds    = includeGovIds,
                FilterSentence   = BuildEmployeeFilterSentence(companyName, search, activeRoleFilter, activeSalaryFilter),
                Employees        = records
            });

            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
        }

        private static string BuildEmployeeFilterSentence(string companyName, string search, string role, string salary)
        {
            // Base: all or searched
            string subject = string.IsNullOrWhiteSpace(search)
                ? $"All active employees in {companyName}"
                : $"Active employees in {companyName} matching \"{search}\"";

            var qualifiers = new List<string>();
            if (!string.IsNullOrWhiteSpace(role))
                qualifiers.Add($"assigned to the {role} role");
            if (!string.IsNullOrWhiteSpace(salary))
                qualifiers.Add($"with {salary}");

            if (qualifiers.Count == 0)
                return $"{subject}."
                    .Replace("All active employees", "This report shows all active employees");

            return $"{subject} {string.Join(" and ", qualifiers)}.";
        }
    }
}
