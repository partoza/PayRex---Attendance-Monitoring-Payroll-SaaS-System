using System.ComponentModel.DataAnnotations;

namespace PayRexApplication.DTOs
{
    /// <summary>
    /// DTO for forgot password request
    /// </summary>
    public class ForgotPasswordDto
    {
 [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for reset password request
    /// </summary>
    public class ResetPasswordDto
    {
   [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset token is required")]
     public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
     [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
         ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string NewPassword { get; set; } = string.Empty;

     [Required(ErrorMessage = "Please confirm your password")]
     [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
  }
}
