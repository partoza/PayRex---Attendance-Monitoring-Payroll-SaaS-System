using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Employee,HR,Accountant")]
    public class EmployeeQRModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public EmployeeQRModel(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public int EmployeeNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public int Age { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Tin { get; set; }
        public string? Sss { get; set; }
        public string? PhilHealth { get; set; }
        public string? PagIbig { get; set; }

        public string? QrBase64 { get; set; }
        public string? SignatureUrl { get; set; }

        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyContact { get; set; }

        public bool NotFoundEmployee { get; set; }

        public async Task OnGetAsync()
        // ...existing code for OnGetAsync above...
        {
            string? email = null;
            if (Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
                        ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                        ?? jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                }
                catch { }
            }

            if (string.IsNullOrEmpty(email))
            {
                NotFoundEmployee = true;
                return;
            }

            var emp = await _db.Employees
                .Include(e => e.Role)
                .Include(e => e.Company)
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == email);

            if (emp == null)
            {
                NotFoundEmployee = true;
                return;
            }

            EmployeeNumber = emp.EmployeeNumber;
            EmployeeCode = string.IsNullOrWhiteSpace(emp.EmployeeCode) ? emp.EmployeeNumber.ToString() : emp.EmployeeCode;
            Name = (emp.FirstName + " " + emp.LastName).Trim();
            Role = emp.Role?.RoleName ?? string.Empty;
            Salary = emp.Role?.BasicRate ?? 0m;
            StartDate = emp.StartDate;
            ProfileImageUrl = emp.User?.ProfileImageUrl;
            SignatureUrl = emp.User?.SignatureUrl;
            CompanyName = emp.Company?.CompanyName;
            CompanyAddress = emp.Company?.Address;
            CompanyContact = emp.Company?.ContactPhone ?? emp.Company?.ContactEmail;

            if (!string.IsNullOrEmpty(emp.Company?.LogoUrl))
                CompanyLogoUrl = emp.Company.LogoUrl;
            else
            {
                try
                {
                    var logoPath = System.IO.Path.Combine(_env.WebRootPath ?? string.Empty, "images", "logo.png");
                    if (System.IO.File.Exists(logoPath))
                        CompanyLogoUrl = "/images/logo.png";
                }
                catch { }
            }

            Tin = emp.TIN;
            Sss = emp.SSS;
            PhilHealth = emp.PhilHealth;
            PagIbig = emp.PagIbig;

            if (emp.Birthdate.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - emp.Birthdate.Value.Year;
                if (emp.Birthdate.Value.Date > today.AddYears(-age)) age--;
                Age = age;
            }

            // Generate QR for inline display only (no download functionality)
            try
            {
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(EmployeeCode ?? EmployeeNumber.ToString(), QRCodeGenerator.ECCLevel.Q);
                var png = new PngByteQRCode(data);
                var qrBytes = png.GetGraphic(20);
                QrBase64 = Convert.ToBase64String(qrBytes);
            }
            catch
            {
                QrBase64 = null;
            }
        }
    }
}
