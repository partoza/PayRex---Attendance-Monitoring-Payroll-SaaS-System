using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Employee")]
    public class PayslipsModel : PageModel
    {
        public List<PayslipItem> Payslips { get; set; } = new();

        public void OnGet()
        {
            // Static demo data
            Payslips = new List<PayslipItem>
            {
                new PayslipItem { 
                    Id = "PS-2026-02-A", 
                    EmployeeName = "Juan Cruz", 
                    Period = "Jan 16 - Jan 31, 2026", 
                    BasicPay = 7500.00m, 
                    Deductions = 1500.00m, 
                    NetPay = 6000.00m, 
                    Status = "Paid" 
                },
                new PayslipItem { 
                    Id = "PS-2026-02-B", 
                    EmployeeName = "Juan Cruz", 
                    Period = "Jan 01 - Jan 15, 2026", 
                    BasicPay = 7500.00m, 
                    Deductions = 1625.00m, 
                    NetPay = 5875.00m, 
                    Status = "Paid" 
                },
                new PayslipItem { 
                    Id = "PS-2026-01-A", 
                    EmployeeName = "Maria Santos", 
                    Period = "Jan 16 - Jan 31, 2026", 
                    BasicPay = 11250.00m, 
                    Deductions = 2200.00m, 
                    NetPay = 9050.00m, 
                    Status = "Paid" 
                },
                new PayslipItem { 
                    Id = "PS-2026-01-B", 
                    EmployeeName = "Maria Santos", 
                    Period = "Jan 01 - Jan 15, 2026", 
                    BasicPay = 11250.00m, 
                    Deductions = 2350.00m, 
                    NetPay = 8900.00m, 
                    Status = "Paid" 
                }
            };
        }

        public class PayslipItem
        {
            public string Id { get; set; } = "";
            public string EmployeeName { get; set; } = "";
            public string Period { get; set; } = "";
            public decimal BasicPay { get; set; }
            public decimal Deductions { get; set; }
            public decimal NetPay { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
