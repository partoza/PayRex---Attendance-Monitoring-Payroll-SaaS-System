using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.Enums;
using PayRex.Web.Services;

namespace PayRex.Web.Pages.Archives
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IAttendanceApiService _attendanceApi;
        private readonly IPayrollApiService _payrollApi;

        public IndexModel(AppDbContext db, IAttendanceApiService attendanceApi, IPayrollApiService payrollApi)
        {
            _db = db;
            _attendanceApi = attendanceApi;
            _payrollApi = payrollApi;
        }

        public List<EmployeesModel.EmployeeItem> Employees { get; set; } = new();
        public List<EmployeesModel.EmployeeItem> InactiveEmployees { get; set; } = new();
        public List<EmployeesModel.RoleItem> Roles { get; set; } = new();
        public PayRex.Web.Services.AttendanceArchiveResponse AttendanceArchives { get; set; } = new();
        public List<LeaveRequestDto> ArchivedLeaves { get; set; } = new();

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
                .Where(e => e.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();

            Employees = items.Where(e => e.Status == EmployeeStatus.Active).Select(e => new EmployeesModel.EmployeeItem
            {
                EmployeeNumber = e.EmployeeNumber,
                Id = string.IsNullOrWhiteSpace(e.EmployeeCode) ? e.EmployeeNumber.ToString() : e.EmployeeCode,
                Name = (e.FirstName + " " + e.LastName).Trim(),
                Position = e.Role?.RoleName ?? "",
                Department = e.Company?.CompanyName ?? "",
                BasicRate = e.Role?.BasicRate ?? 0m,
                PayType = e.Role?.RateType ?? "",
                EmploymentStatus = e.Status.ToString(),
                Email = e.Email,
                ProfileImageUrl = e.User?.ProfileImageUrl
            }).ToList();

            InactiveEmployees = items.Where(e => e.Status != EmployeeStatus.Active).Select(e => new EmployeesModel.EmployeeItem
            {
                EmployeeNumber = e.EmployeeNumber,
                Id = string.IsNullOrWhiteSpace(e.EmployeeCode) ? e.EmployeeNumber.ToString() : e.EmployeeCode,
                Name = (e.FirstName + " " + e.LastName).Trim(),
                Position = e.Role?.RoleName ?? "",
                Department = e.Company?.CompanyName ?? "",
                BasicRate = e.Role?.BasicRate ?? 0m,
                PayType = e.Role?.RateType ?? "",
                EmploymentStatus = e.Status.ToString(),
                Email = e.Email,
                ProfileImageUrl = e.User?.ProfileImageUrl
            }).ToList();

            var roles = await _db.EmployeeRoles.Where(r => r.IsActive && r.CompanyId == companyId).AsNoTracking().ToListAsync();
            Roles = roles.Select(r => new EmployeesModel.RoleItem
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                BasicRate = r.BasicRate ?? 0m
            }).OrderBy(r => r.RoleName).ToList();

            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (!string.IsNullOrEmpty(token))
            {
                AttendanceArchives = await _attendanceApi.GetArchivedRecordsAsync(token);
                ArchivedLeaves = await _payrollApi.GetArchivedLeaveRequestsAsync(token);
            }
        }

        public async Task<IActionResult> OnPostActivateAsync(int employeeNumber)
        {
            var companyId = GetCompanyId();
            var e = await _db.Employees.FirstOrDefaultAsync(emp => emp.EmployeeNumber == employeeNumber && emp.CompanyId == companyId);
            if (e == null) return Forbid();
            e.Status = EmployeeStatus.Active;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
