using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRex.Web.Services;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,Accountant")]
    [IgnoreAntiforgeryToken]
    public class CompensationModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        private readonly AppDbContext _db;

        public List<DeductionDto> Deductions { get; set; } = new();
        public List<BenefitDto> Benefits { get; set; } = new();
        public List<EmployeeListItem> Employees { get; set; } = new();

        [TempData] public string? StatusMessage { get; set; }

        public CompensationModel(IPayrollApiService payroll, AppDbContext db)
        {
            _payroll = payroll;
            _db = db;
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

        public class EmployeeListItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}
