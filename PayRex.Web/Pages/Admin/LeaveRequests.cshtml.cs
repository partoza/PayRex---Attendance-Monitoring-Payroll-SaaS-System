using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "Admin,HR,Accountant")]
    public class LeaveRequestsModel : PageModel
    {
        private readonly IPayrollApiService _payroll;

        public List<LeaveRequestDto> AllRequests { get; set; } = new();
        public List<LeaveRequestDto> Requests { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string? ActiveStatus { get; set; }
        [TempData] public string? StatusMessage { get; set; }

        public LeaveRequestsModel(IPayrollApiService payroll) => _payroll = payroll;

        public async Task OnGetAsync(string? status)
        {
            ActiveStatus = status ?? "Pending";
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            
            // Unfiltered collection for the aggregate KPIs
            AllRequests = await _payroll.GetLeaveRequestsAsync(token, null);
            
            // Filtered collection for the data table
            Requests = ActiveStatus == "All" ? AllRequests : AllRequests.Where(r => r.Status == ActiveStatus).ToList();
        }

        public string FormatDuration(int days)
        {
            if (days >= 365)
            {
                var years = days / 365;
                return $"{years} year{(years > 1 ? "s" : "")}";
            }
            if (days >= 30)
            {
                var months = days / 30;
                return $"{months} month{(months > 1 ? "s" : "")}";
            }
            return $"{days} day{(days > 1 ? "s" : "")}";
        }

        public async Task<IActionResult> OnPostReviewAsync(int id, bool approved, string? remarks)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var (ok, msg) = await _payroll.ReviewLeaveRequestAsync(token, id, new { approved, remarks });
            StatusMessage = ok ? $"success:{msg}" : $"error:{msg}";
            return RedirectToPage(new { status = ActiveStatus });
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var (ok, msg) = await _payroll.ArchiveLeaveRequestAsync(token, id);
            StatusMessage = ok ? $"success:{msg}" : $"error:{msg}";
            return RedirectToPage(new { status = ActiveStatus });
        }
    }
}
