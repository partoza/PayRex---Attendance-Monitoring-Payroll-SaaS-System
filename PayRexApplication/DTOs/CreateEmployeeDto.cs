using System.ComponentModel.DataAnnotations;
using PayRexApplication.Enums;

namespace PayRexApplication.DTOs
{
    public class CreateEmployeeDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(50)]
        public string? CivilStatus { get; set; }

        public DateTime? Birthdate { get; set; }

        public int? RoleId { get; set; }

        public DateTime? StartDate { get; set; }

        // Optional initial status for the employee (used on create/update)
        public EmployeeStatus? Status { get; set; }

        // Government Identifiers
        [MaxLength(15)]
        [RegularExpression(@"^\d{3}-\d{3}-\d{3}-\d{3}$", ErrorMessage = "TIN must be in format XXX-XXX-XXX-XXX")]
        public string? TIN { get; set; }

        [MaxLength(12)]
        [RegularExpression(@"^\d{2}-\d{7}-\d{1}$", ErrorMessage = "SSS must be in format XX-XXXXXXX-X")]
        public string? SSS { get; set; }

        [MaxLength(14)]
        [RegularExpression(@"^\d{2}-\d{9}-\d{1}$", ErrorMessage = "PhilHealth must be in format XX-XXXXXXXXX-X")]
        public string? PhilHealth { get; set; }

        [MaxLength(14)]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}$", ErrorMessage = "Pag-IBIG must be in format XXXX-XXXX-XXXX")]
        public string? PagIbig { get; set; }
    }
}
