using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    public class AddEmployeeModel : PageModel
    {
        [BindProperty]
        public EmployeeInputModel Input { get; set; } = new EmployeeInputModel();

        [TempData]
        public string StatusMessage { get; set; }

        public void OnGet(string employeeNumber)
        {
            if (!string.IsNullOrEmpty(employeeNumber))
            {
                Input.EmployeeCode = employeeNumber;
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Implement server-side creation logic (call API or database)
            StatusMessage = "Employee creation is not implemented on server-side yet.";

            return RedirectToPage("Employees");
        }
    }

    public class EmployeeInputModel
    {
        public string EmployeeCode { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        public string ContactNumber { get; set; }
        public string CivilStatus { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        public int? RoleId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        public string TIN { get; set; }
        public string SSS { get; set; }
        public string PhilHealth { get; set; }
        public string PagIbig { get; set; }

        public IFormFile ProfilePhoto { get; set; }
        public IFormFile Signature { get; set; }
    }
}
