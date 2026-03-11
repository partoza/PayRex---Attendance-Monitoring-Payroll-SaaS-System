using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class EditEmployeeModel : PageModel
    {
        private readonly AppDbContext _db;

        public EditEmployeeModel(AppDbContext db)
        {
            _db = db;
        }

        public int EmployeeNumber { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? ContactNumber { get; set; }
        public string? CivilStatus { get; set; }
        public DateTime? Birthdate { get; set; }
        public int? RoleId { get; set; }
        public decimal BasicRate { get; set; }
        public DateTime StartDate { get; set; }
        public string? Tin { get; set; }
        public string? Sss { get; set; }
        public string? PhilHealth { get; set; }
        public string? PagIbig { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SignatureUrl { get; set; }
        public PayRexApplication.Enums.EmployeeStatus Status { get; set; }

        public async Task<IActionResult> OnGetAsync(int? employeeNumber)
        {
            if (!employeeNumber.HasValue) return RedirectToPage("/Employees");

            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId)) return Forbid();

            var emp = await _db.Employees
                .Include(e => e.Role)
                .Include(e => e.Company)
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber.Value && e.CompanyId == companyId);

            if (emp == null) return RedirectToPage("/Employees");

            EmployeeNumber = emp.EmployeeNumber;
            EmployeeCode = emp.EmployeeCode;
            CompanyName = emp.Company?.CompanyName ?? "";
            FirstName = emp.FirstName;
            LastName = emp.LastName;
            Email = emp.Email;
            ContactNumber = emp.ContactNumber;
            CivilStatus = emp.CivilStatus;
            Birthdate = emp.Birthdate;
            RoleId = emp.RoleId;
            BasicRate = emp.Role?.BasicRate ?? 0m;
            StartDate = emp.StartDate;
            Tin = emp.TIN;
            Sss = emp.SSS;
            PhilHealth = emp.PhilHealth;
            PagIbig = emp.PagIbig;
            ProfileImageUrl = emp.User?.ProfileImageUrl;
            SignatureUrl = emp.User?.SignatureUrl;
            Status = emp.Status;

            return Page();
        }
    }
}
