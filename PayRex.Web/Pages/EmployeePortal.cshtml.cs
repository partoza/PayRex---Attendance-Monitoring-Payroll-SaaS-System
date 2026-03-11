using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Accountant,Employee")]
    [IgnoreAntiforgeryToken]
    public class EmployeePortalModel : PageModel
    {
        private readonly IPayrollApiService _payroll;

        public List<LeaveRequestDto> LeaveRequests { get; set; } = new();
        public List<PayslipDto> Payslips { get; set; } = new();
        public LeaveBalanceDto Balance { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }

        public EmployeePortalModel(IPayrollApiService payroll) => _payroll = payroll;

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var isEmployeeView = Request.Cookies["PayRex.ViewMode"] == "employee";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var selfOnly = isEmployeeView || role == "Employee";
            var leaveTask = _payroll.GetLeaveRequestsAsync(token, selfOnly: selfOnly);
            var payslipsTask = _payroll.GetPayslipsAsync(token, selfOnly);
            var balanceTask = _payroll.GetLeaveBalanceAsync(token);
            await Task.WhenAll(leaveTask, payslipsTask, balanceTask);
            LeaveRequests = leaveTask.Result;
            Payslips = payslipsTask.Result;
            Balance = balanceTask.Result;
        }

        public async Task<IActionResult> OnPostSubmitLeaveAsync(
            string leaveType, DateOnly startDate, DateOnly endDate, string? reason)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var (ok, msg) = await _payroll.CreateLeaveRequestAsync(token, new
            {
                leaveType,
                startDate,
                endDate,
                reason
            });
            StatusMessage = ok ? $"success:{msg}" : $"error:{msg}";
            return RedirectToPage();
        }
    }
}
