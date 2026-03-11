using System.ComponentModel.DataAnnotations;

namespace PayRex.Web.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

     [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
     public string Email { get; set; } = string.Empty;

         [Required(ErrorMessage = "Password is required")]
         [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
     public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    }
}
