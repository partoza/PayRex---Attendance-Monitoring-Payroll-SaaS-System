using System.ComponentModel.DataAnnotations;

namespace PayRexApplication.DTOs
{
    /// <summary>
    /// DTO for updating user profile information
  /// </summary>
 public class UpdateProfileDto
    {
    [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
      public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
     [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string? Email { get; set; }
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
  public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
      public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
          ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string NewPassword { get; set; } = string.Empty;

      [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for profile picture upload response
    /// </summary>
    public class ProfileImageResponseDto
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO for user profile response
    /// </summary>
    public class UserProfileResponseDto
 {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? ProfileImageUrl { get; set; }
     public bool IsTwoFactorEnabled { get; set; }
        public DateTime? LastPasswordChangeAt { get; set; }
    }

    /// <summary>
    /// DTO for TOTP setup response with recovery codes
    /// </summary>
  public class TotpSetupWithRecoveryDto
    {
        public string? SecretKey { get; set; }
     public string? QrCodeUri { get; set; }
      public string? ManualEntryKey { get; set; }
    }

    /// <summary>
    /// DTO for enabling TOTP with verification
    /// </summary>
    public class EnableTotpWithVerificationDto
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "TOTP code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "TOTP code must be 6 digits")]
        public string TotpCode { get; set; } = string.Empty;
  }

    /// <summary>
    /// DTO for TOTP enable response with recovery codes
    /// </summary>
    public class TotpEnableResponseDto
 {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? RecoveryCodes { get; set; }
    }
}
