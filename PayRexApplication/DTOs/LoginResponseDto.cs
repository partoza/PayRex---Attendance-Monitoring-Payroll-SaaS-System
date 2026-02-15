namespace PayRexApplication.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        // TOTP properties
        public bool RequireTotp { get; set; }
        public string? Message { get; set; }

        // Lockout properties
        public bool IsLockedOut { get; set; }
        public int LockoutRemainingSeconds { get; set; }

        // Password change requirement
        public bool MustChangePassword { get; set; }
    }
}
