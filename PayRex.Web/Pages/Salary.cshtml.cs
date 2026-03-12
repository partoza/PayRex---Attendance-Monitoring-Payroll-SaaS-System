using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.DTOs;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,Accountant,HR,Employee")]
    public class SalaryModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        private readonly IAuthApiService _auth;
        public List<PayrollSummaryDto> Payrolls { get; set; } = new();
        public List<PayrollPeriodDto> Periods { get; set; } = new();
        public CompanyProfileDto? Company { get; set; }
        public int? SelectedPeriodId { get; set; }
        public string SelectedPeriodStatus { get; set; } = "";
        public string CurrentPeriod { get; set; } = "No Period Selected";
        public decimal TotalPayrollCost { get; set; }

        public SalaryModel(IPayrollApiService payroll, IAuthApiService auth)
        {
            _payroll = payroll;
            _auth = auth;
        }

        public async Task OnGetAsync(int? periodId)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            
            Company = await _auth.GetCompanyProfileAsync(token);

            // Auto-generate draft periods for the current month based on company payroll cycle
            if (User.IsInRole("Admin") || User.IsInRole("Accountant") || User.IsInRole("HR"))
            {
                await _payroll.AutoGeneratePeriodsAsync(token);
            }

            Periods = await _payroll.GetPeriodsAsync(token);

            if (periodId.HasValue)
            {
                SelectedPeriodId = periodId;
            }
            else if (Periods.Any())
            {
                // Select the most actionable period: Computed (pending approval) > Approved (pending release) > Draft > first
                var activePeriod = Periods.FirstOrDefault(p => p.Status == "Computed")
                    ?? Periods.FirstOrDefault(p => p.Status == "Approved")
                    ?? Periods.FirstOrDefault(p => p.Status == "Draft");
                SelectedPeriodId = activePeriod?.PayrollPeriodId ?? Periods.First().PayrollPeriodId;
            }

            if (SelectedPeriodId.HasValue)
            {
                Payrolls = await _payroll.GetSummariesAsync(token, SelectedPeriodId.Value);
                var period = Periods.FirstOrDefault(p => p.PayrollPeriodId == SelectedPeriodId.Value);
                CurrentPeriod = period?.PeriodName ?? "Unknown Period";
                SelectedPeriodStatus = period?.Status ?? "";
                TotalPayrollCost = Payrolls.Sum(p => p.NetPay);
            }
        }
    }
}
