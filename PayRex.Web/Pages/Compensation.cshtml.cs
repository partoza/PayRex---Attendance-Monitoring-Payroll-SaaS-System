using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class CompensationModel : PageModel
    {
        public List<DeductionItem> Deductions { get; set; } = new();
        public List<BenefitItem> Benefits { get; set; } = new();

        public void OnGet()
        {
            // Static demo data for Deductions
            Deductions = new List<DeductionItem>
            {
                new DeductionItem { Id = "DED-001", EmployeeName = "John Doe", Type = "Tardiness", Amount = 150.00m, Status = "Applied", Date = DateTime.Now.AddDays(-2) },
                new DeductionItem { Id = "DED-002", EmployeeName = "Maria Santos", Type = "Absence", Amount = 1200.00m, Status = "Deducted", Date = DateTime.Now.AddDays(-5) },
                new DeductionItem { Id = "DED-003", EmployeeName = "Juan Cruz", Type = "Cash Advance", Amount = 5000.00m, Status = "Pending", Date = DateTime.Now.AddDays(-1) },
                new DeductionItem { Id = "DED-004", EmployeeName = "Ana Reyes", Type = "Loan Payment", Amount = 2500.00m, Status = "Deducted", Date = DateTime.Now.AddDays(-10) }
            };

            // Static demo data for Benefits
            Benefits = new List<BenefitItem>
            {
                new BenefitItem { Id = "BEN-001", EmployeeName = "John Doe", Type = "Rice Allowance", Amount = 2000.00m, Frequency = "Monthly", Status = "Active" },
                new BenefitItem { Id = "BEN-002", EmployeeName = "Maria Santos", Type = "Performance Bonus", Amount = 5000.00m, Frequency = "One-time", Status = "Approved" },
                new BenefitItem { Id = "BEN-003", EmployeeName = "Juan Cruz", Type = "13th Month Pay", Amount = 25000.00m, Frequency = "Annual", Status = "Pending" },
                new BenefitItem { Id = "BEN-004", EmployeeName = "Ana Reyes", Type = "Laundry Allowance", Amount = 500.00m, Frequency = "Monthly", Status = "Active" }
            };
        }

        public class DeductionItem
        {
            public string Id { get; set; } = "";
            public string EmployeeName { get; set; } = "";
            public string Type { get; set; } = "";
            public decimal Amount { get; set; }
            public string Status { get; set; } = "";
            public DateTime Date { get; set; }
        }

        public class BenefitItem
        {
            public string Id { get; set; } = "";
            public string EmployeeName { get; set; } = "";
            public string Type { get; set; } = "";
            public decimal Amount { get; set; }
            public string Frequency { get; set; } = "";
            public string Status { get; set; } = "";
        }
    }
}
