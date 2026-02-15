namespace PayRex.Web.DTOs
{
    /// <summary>
    /// Full user profile response from API
    /// </summary>
    public class UserProfileDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? CompanyId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public DateTime? LastPasswordChangeAt { get; set; }
    }

    /// <summary>
    /// DTO for TOTP setup response
    /// </summary>
    public class TotpSetupResponseDto
    {
        public string? SecretKey { get; set; }
        public string? QrCodeUri { get; set; }
        public string? ManualEntryKey { get; set; }
    }

    /// <summary>
    /// DTO for enabling TOTP request
    /// </summary>
    public class EnableTotpRequestDto
    {
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

    /// <summary>
    /// DTO for updating profile
    /// </summary>
    public class UpdateProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for profile image upload response
    /// </summary>
    public class ProfileImageResponseDto
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO for 2FA status
    /// </summary>
    public class TwoFactorStatusDto
    {
        public bool IsEnabled { get; set; }
        public bool HasSecretKey { get; set; }
        public int RecoveryCodesRemaining { get; set; }
    }
}
