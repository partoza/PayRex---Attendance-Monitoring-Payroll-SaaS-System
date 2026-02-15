using System.ComponentModel.DataAnnotations;

namespace PayRex.Web.DTOs
{
    public class TotpVerificationDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
      public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "TOTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "TOTP code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "TOTP code must be 6 digits")]
        public string TotpCode { get; set; } = string.Empty;
    }
}