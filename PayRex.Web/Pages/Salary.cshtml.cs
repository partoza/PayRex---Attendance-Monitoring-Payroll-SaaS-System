using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Employee")]
    public class SalaryModel : PageModel
    {
        public List<PayrollItem> Payrolls { get; set; } = new();
        public string CurrentPeriod { get; set; } = "Feb 01 - Feb 15, 2026";
        public decimal TotalPayrollCost { get; set; }

        public void OnGet()
        {
            // Demo data generation
            var employees = new[]
            {
                new { Name = "Juan Cruz", Role = "Admin", Rate = 15000m, Days = 11 },
                new { Name = "Maria Santos", Role = "HR", Rate = 22500m, Days = 11 },
                new { Name = "Jose Reyes", Role = "Employee", Rate = 17500m, Days = 11 },
                new { Name = "Pedro Garcia", Role = "Employee", Rate = 14000m, Days = 11 },
                new { Name = "Luz Ramos", Role = "Employee", Rate = 16000m, Days = 11 },
                new { Name = "Rosa Torres", Role = "Employee", Rate = 20000m, Days = 11 },
                new { Name = "Antonio Lim", Role = "Employee", Rate = 13500m, Days = 10 }, // 1 absent
            };

            Payrolls = new List<PayrollItem>();

            foreach (var emp in employees)
            {
                var gross = emp.Rate; // Semi-monthly rate for demo
                var sss = Math.Round(gross * 0.045m, 2);
                var philhealth = Math.Round(gross * 0.025m, 2);
                var pagibig = 100m;
                var tax = Math.Round((gross - sss - philhealth - pagibig) * 0.05m, 2); // Simplified tax
                var late = emp.Name == "Maria Santos" ? 250.00m : 0m; 
                var totalDeductions = sss + philhealth + pagibig + tax + late;
                var net = gross - totalDeductions;

                Payrolls.Add(new PayrollItem
                {
                    EmployeeName = emp.Name,
                    Role = emp.Role,
                    BasicPay = gross,
                    SSS = sss,
                    PhilHealth = philhealth,
                    PagIBIG = pagibig,
                    Tax = tax,
                    LateDeduction = late,
                    TotalDeductions = totalDeductions,
                    NetPay = net,
                    Status = "Draft"
                });
            }

            TotalPayrollCost = Payrolls.Sum(p => p.NetPay);
        }

        public class PayrollItem
        {
            public string EmployeeName { get; set; } = "";
            public string Role { get; set; } = "";
            public decimal BasicPay { get; set; }
            public decimal SSS { get; set; }
            public decimal PhilHealth { get; set; }
            public decimal PagIBIG { get; set; }
            public decimal Tax { get; set; }
            public decimal LateDeduction { get; set; }
            public decimal TotalDeductions { get; set; }
            public decimal NetPay { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
