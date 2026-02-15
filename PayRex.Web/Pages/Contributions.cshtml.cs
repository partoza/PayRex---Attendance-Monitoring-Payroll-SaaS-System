using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Employee")]
    public class ContributionsModel : PageModel
    {
        public List<ContributionItem> Contributions { get; set; } = new();
        public ContributionStats Stats { get; set; } = new();

        public void OnGet()
        {
            // Static demo data
            Contributions = new List<ContributionItem>
            {
                new ContributionItem { EmployeeName = "Juan Cruz", Type = "SSS", EmployeeShare = 1350.00m, EmployerShare = 2550.00m, Period = "Jan 2026", Status = "Remitted" },
                new ContributionItem { EmployeeName = "Juan Cruz", Type = "PhilHealth", EmployeeShare = 450.00m, EmployerShare = 450.00m, Period = "Jan 2026", Status = "Remitted" },
                new ContributionItem { EmployeeName = "Juan Cruz", Type = "Pag-IBIG", EmployeeShare = 100.00m, EmployerShare = 100.00m, Period = "Jan 2026", Status = "Remitted" },
                
                new ContributionItem { EmployeeName = "Maria Santos", Type = "SSS", EmployeeShare = 1350.00m, EmployerShare = 2550.00m, Period = "Jan 2026", Status = "Remitted" },
                new ContributionItem { EmployeeName = "Maria Santos", Type = "PhilHealth", EmployeeShare = 675.00m, EmployerShare = 675.00m, Period = "Jan 2026", Status = "Remitted" },
                new ContributionItem { EmployeeName = "Maria Santos", Type = "Pag-IBIG", EmployeeShare = 100.00m, EmployerShare = 100.00m, Period = "Jan 2026", Status = "Remitted" },

                new ContributionItem { EmployeeName = "Jose Reyes", Type = "SSS", EmployeeShare = 1575.00m, EmployerShare = 2975.00m, Period = "Jan 2026", Status = "Pending" },
                new ContributionItem { EmployeeName = "Jose Reyes", Type = "PhilHealth", EmployeeShare = 525.00m, EmployerShare = 525.00m, Period = "Jan 2026", Status = "Pending" },
                new ContributionItem { EmployeeName = "Jose Reyes", Type = "Pag-IBIG", EmployeeShare = 100.00m, EmployerShare = 100.00m, Period = "Jan 2026", Status = "Pending" }
            };

            Stats = new ContributionStats 
            {
                TotalSSS = 15600.00m,
                TotalPhilHealth = 5200.00m,
                TotalPagIBIG = 2400.00m
            };
        }

        public class ContributionStats
        {
            public decimal TotalSSS { get; set; }
            public decimal TotalPhilHealth { get; set; }
            public decimal TotalPagIBIG { get; set; }
        }

        public class ContributionItem
        {
            public string EmployeeName { get; set; } = "";
            public string Type { get; set; } = "";
            public decimal EmployeeShare { get; set; }
            public decimal EmployerShare { get; set; }
            public string Period { get; set; } = "";
            public string Status { get; set; } = ""; // Remitted, Pending
        }
    }
}
