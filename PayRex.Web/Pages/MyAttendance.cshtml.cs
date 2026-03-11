using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Employee,HR,Accountant")]
    [IgnoreAntiforgeryToken]
    public class MyAttendanceModel : PageModel
    {
        private readonly IAttendanceApiService _attendanceService;
        private readonly ILogger<MyAttendanceModel> _logger;

        public MyAttendanceModel(IAttendanceApiService attendanceService, ILogger<MyAttendanceModel> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        public List<AttendanceRecordDto> Records { get; set; } = new();
        public MyAttendanceSummary Summary { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["PayRex.AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var from = FromDate ?? DateTime.Today.AddDays(-30);
            var to = ToDate ?? DateTime.Today;

            var result = await _attendanceService.GetMyAttendanceAsync(token, from, to);
            Records = result.Records;
            Summary = result.Summary;

            return Page();
        }
    }
}
