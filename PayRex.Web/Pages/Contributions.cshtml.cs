using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Accountant,Employee")]
    public class ContributionsModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        public List<ContributionDto> Contributions { get; set; } = new();
        public List<PayrollPeriodDto> Periods { get; set; } = new();
        public int? SelectedPeriodId { get; set; }
        public string CurrentPeriod { get; set; } = "No Period Selected";
        public ContributionStats Stats { get; set; } = new();

        public ContributionsModel(IPayrollApiService payroll) => _payroll = payroll;

        public async Task OnGetAsync(int? periodId)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var isEmployeeView = Request.Cookies["PayRex.ViewMode"] == "employee";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var selfOnly = isEmployeeView || role == "Employee";
            Periods = await _payroll.GetPeriodsAsync(token);

            if (periodId.HasValue)
            {
                SelectedPeriodId = periodId;
            }
            else if (Periods.Any())
            {
                SelectedPeriodId = Periods.First().PayrollPeriodId;
            }

            if (SelectedPeriodId.HasValue)
            {
                var period = Periods.FirstOrDefault(p => p.PayrollPeriodId == SelectedPeriodId.Value);
                CurrentPeriod = period?.PeriodName ?? "Unknown Period";
            }

            Contributions = await _payroll.GetContributionsAsync(token, SelectedPeriodId, selfOnly);

            Stats = new ContributionStats
            {
                TotalSSS = Contributions.Where(c => c.Type == "SSS").Sum(c => c.EmployeeShare + c.EmployerShare),
                TotalPhilHealth = Contributions.Where(c => c.Type == "PhilHealth").Sum(c => c.EmployeeShare + c.EmployerShare),
                TotalPagIBIG = Contributions.Where(c => c.Type is "PagIBIG" or "Pag-IBIG" or "PagIig" or "PagIbig").Sum(c => c.EmployeeShare + c.EmployerShare),
                TotalTax = Contributions.Where(c => c.Type is "Tax" or "BIR" or "Withholding Tax").Sum(c => c.EmployeeShare)
            };
        }

        public class ContributionStats
        {
            public decimal TotalSSS { get; set; }
            public decimal TotalPhilHealth { get; set; }
            public decimal TotalPagIBIG { get; set; }
            public decimal TotalTax { get; set; }
        }
    }
}
