using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public CompanyProfile Profile { get; set; } = new();

        public List<RoleConfig> Roles { get; set; } = new();

        public void OnGet()
        {
            // Static demo data
            Profile = new CompanyProfile
            {
                CompanyName = "PayRex Corp.",
                Address = "123 Business St., Makati City, Philippines",
                ContactEmail = "admin@payrex.com",
                ContactPhone = "+63 2 8123 4567",
                Tin = "123-456-789-000"
            };

            Roles = new List<RoleConfig>
            {
                new RoleConfig { RoleName = "Admin", BasicRate = 15000.00m, RateType = "Semi-Monthly", Description = "System Administrators" },
                new RoleConfig { RoleName = "HR", BasicRate = 22500.00m, RateType = "Semi-Monthly", Description = "Human Resource Managers" },
                new RoleConfig { RoleName = "Employee", BasicRate = 17500.00m, RateType = "Semi-Monthly", Description = "Regular Staff" },
                new RoleConfig { RoleName = "Intern", BasicRate = 500.00m, RateType = "Daily", Description = "OJT / Interns" }
            };
        }

        public class CompanyProfile
        {
            public string CompanyName { get; set; } = "";
            public string Address { get; set; } = "";
            public string ContactEmail { get; set; } = "";
            public string ContactPhone { get; set; } = "";
            public string Tin { get; set; } = "";
        }

        public class RoleConfig
        {
            public string RoleName { get; set; } = "";
            public decimal BasicRate { get; set; }
            public string RateType { get; set; } = "";
            public string Description { get; set; } = "";
        }
    }
}
