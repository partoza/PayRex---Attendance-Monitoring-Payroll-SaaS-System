using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class UsersModel : PageModel
    {
        public List<UserItem> Users { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }

        public void OnGet()
        {
            // Static demo data for HR Management
            Users = new List<UserItem>
            {
                new UserItem { UserId = "1001", Name = "Juan Cruz", Role = "Admin", Email = "juan.cruz@company.com", Department = "Administration", Status = "Active", DateHired = new DateTime(2023, 1, 10) },
                new UserItem { UserId = "1002", Name = "Maria Santos", Role = "HR", Email = "maria.santos@company.com", Department = "Human Resources", Status = "Active", DateHired = new DateTime(2023, 3, 15) },
                new UserItem { UserId = "1003", Name = "Jose Reyes", Role = "Employee", Email = "jose.reyes@company.com", Department = "IT Department", Status = "Active", DateHired = new DateTime(2023, 6, 1) },
                new UserItem { UserId = "1004", Name = "Ana Dizon", Role = "Employee", Email = "ana.dizon@company.com", Department = "Sales", Status = "On Leave", DateHired = new DateTime(2023, 8, 20) },
                new UserItem { UserId = "1005", Name = "Pedro Garcia", Role = "Employee", Email = "pedro.garcia@company.com", Department = "Marketing", Status = "Active", DateHired = new DateTime(2023, 9, 5) },
                new UserItem { UserId = "1006", Name = "Luz Ramos", Role = "Employee", Email = "luz.ramos@company.com", Department = "Finance", Status = "Active", DateHired = new DateTime(2023, 11, 12) },
                new UserItem { UserId = "1007", Name = "Antonio Lim", Role = "Employee", Email = "antonio.lim@company.com", Department = "Operations", Status = "Resigned", DateHired = new DateTime(2022, 2, 1) },
                new UserItem { UserId = "1008", Name = "Rosa Torres", Role = "Employee", Email = "rosa.torres@company.com", Department = "IT Department", Status = "Active", DateHired = new DateTime(2024, 1, 5) },
                new UserItem { UserId = "1009", Name = "Luis Tan", Role = "Employee", Email = "luis.tan@company.com", Department = "Sales", Status = "Terminated", DateHired = new DateTime(2022, 3, 10) },
                new UserItem { UserId = "1010", Name = "Elena Gomez", Role = "Employee", Email = "elena.gomez@company.com", Department = "Marketing", Status = "Active", DateHired = new DateTime(2024, 4, 22) }
            };
        }

        public class UserItem
        {
            public string UserId { get; set; } = "";
            public string Name { get; set; } = "";
            public string Role { get; set; } = "";
            public string Email { get; set; } = "";
            public string Department { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime DateHired { get; set; }
        }
    }
}
