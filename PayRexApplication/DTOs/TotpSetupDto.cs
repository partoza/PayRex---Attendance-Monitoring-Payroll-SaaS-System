using System.ComponentModel.DataAnnotations;

namespace PayRexApplication.DTOs
{
    /// <summary>
    /// DTO for TOTP setup response
    /// </summary>
public class TotpSetupDto
    {
        /// <summary>
    /// Base32-encoded secret key
     /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
      /// QR Code URI for Google Authenticator
        /// </summary>
        public string QrCodeUri { get; set; } = string.Empty;

        /// <summary>
     /// Manual entry key (formatted for easier reading)
        /// </summary>
        public string ManualEntryKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for verifying TOTP code during login
    /// </summary>
    public class VerifyTotpDto
    {
        [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "TOTP code is required")]
     [StringLength(6, MinimumLength = 6, ErrorMessage = "TOTP code must be 6 digits")]
     [RegularExpression(@"^\d{6}$", ErrorMessage = "TOTP code must be 6 digits")]
        public string TotpCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for enabling TOTP
    /// </summary>
    public class EnableTotpDto
    {
     [Required(ErrorMessage = "TOTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "TOTP code must be 6 digits")]
      [RegularExpression(@"^\d{6}$", ErrorMessage = "TOTP code must be 6 digits")]
        public string TotpCode { get; set; } = string.Empty;
    }
}
