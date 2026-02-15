using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeesModel : PageModel
    {
        public List<EmployeeItem> Employees { get; set; } = new();

        public void OnGet()
        {
            Employees = new List<EmployeeItem>
            {
                new EmployeeItem { Id = "1001", Name = "Juan Cruz", Position = "Admin", Department = "Administration", BasicRate = 2500.00m, PayType = "Daily", EmploymentStatus = "Active", DateHired = new DateTime(2023, 1, 10) },
                new EmployeeItem { Id = "1002", Name = "Maria Santos", Position = "HR", Department = "Human Resources", BasicRate = 45000.00m, PayType = "Monthly", EmploymentStatus = "Active", DateHired = new DateTime(2023, 3, 15) },
                new EmployeeItem { Id = "1003", Name = "Jose Reyes", Position = "Employee", Department = "IT Department", BasicRate = 35000.00m, PayType = "Monthly", EmploymentStatus = "Active", DateHired = new DateTime(2023, 6, 1) },
                new EmployeeItem { Id = "1004", Name = "Ana Dizon", Position = "Employee", Department = "Sales", BasicRate = 600.00m, PayType = "Daily", EmploymentStatus = "On Leave", DateHired = new DateTime(2023, 8, 20) },
                new EmployeeItem { Id = "1005", Name = "Pedro Garcia", Position = "Employee", Department = "Marketing", BasicRate = 28000.00m, PayType = "Monthly", EmploymentStatus = "Active", DateHired = new DateTime(2023, 9, 5) },
                new EmployeeItem { Id = "1006", Name = "Luz Ramos", Position = "Employee", Department = "Finance", BasicRate = 32000.00m, PayType = "Monthly", EmploymentStatus = "Active", DateHired = new DateTime(2023, 11, 12) },
                new EmployeeItem { Id = "1007", Name = "Antonio Lim", Position = "Employee", Department = "Operations", BasicRate = 550.00m, PayType = "Daily", EmploymentStatus = "Resigned", DateHired = new DateTime(2022, 2, 1) },
                new EmployeeItem { Id = "1008", Name = "Rosa Torres", Position = "Employee", Department = "IT Department", BasicRate = 40000.00m, PayType = "Monthly", EmploymentStatus = "Active", DateHired = new DateTime(2024, 1, 5) },
                new EmployeeItem { Id = "1009", Name = "Luis Tan", Position = "Employee", Department = "Sales", BasicRate = 25000.00m, PayType = "Monthly", EmploymentStatus = "Terminated", DateHired = new DateTime(2022, 3, 10) },
                new EmployeeItem { Id = "1010", Name = "Elena Gomez", Position = "Employee", Department = "Marketing", BasicRate = 650.00m, PayType = "Daily", EmploymentStatus = "Active", DateHired = new DateTime(2024, 4, 22) }
            };
        }

        public class EmployeeItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Position { get; set; } = "";
            public string Department { get; set; } = "";
            public decimal BasicRate { get; set; }
            public string PayType { get; set; } = "";
            public string EmploymentStatus { get; set; } = "";
            public DateTime DateHired { get; set; }
        }
    }
}
