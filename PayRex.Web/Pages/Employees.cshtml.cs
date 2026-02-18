using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;

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

        public async Task OnGetAsync()
        {
            var items = await _db.Employees
                .Include(e => e.Role)
                .Include(e => e.Company)
                .AsNoTracking()
                .ToListAsync();

            Employees = items.Select(e => new EmployeeItem
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
                ProfileImageUrl = e.User?.ProfileImageUrl
            }).ToList();
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
        }
    }
}
