using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Accountant,Employee")]
    public class PayslipsModel : PageModel
    {
        private readonly IPayrollApiService _payroll;
        public List<PayslipDto> Payslips { get; set; } = new();

        public PayslipsModel(IPayrollApiService payroll) => _payroll = payroll;

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var isEmployeeView = Request.Cookies["PayRex.ViewMode"] == "employee";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var selfOnly = isEmployeeView || role == "Employee";
            Payslips = await _payroll.GetPayslipsAsync(token, selfOnly);
        }
    }
}
