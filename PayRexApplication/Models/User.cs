namespace PayRexApplication.Models
{
    using PayRexApplication.Enums;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Defines the <see cref="User" />
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>
        /// Gets or sets the UserId
        /// </summary>
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the CompanyId
        /// </summary>
        [Required]
        [MaxLength(4)]
        [Column("companyId")]
        public string CompanyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the FirstName
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("firstName")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the LastName
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("lastName")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PasswordHash
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        [Required]
        [Column("role")]
        public UserRole Role { get; set; }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        [Required]
        [Column("status")]
        public UserStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the CreatedAt
        /// </summary>
        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the UpdatedAt
        /// </summary>
        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Profile Picture (Cloudinary URL)

        /// <summary>
        /// Gets or sets the ProfileImageUrl
        /// </summary>
        [MaxLength(512)]
        [Column("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }

        // TOTP (Two-Factor Authentication) fields

        /// <summary>
        /// Gets or sets a value indicating whether IsTwoFactorEnabled
        /// </summary>
        [Column("isTwoFactorEnabled")]
        public bool IsTwoFactorEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the TotpSecretKey
        /// </summary>
        [MaxLength(2048)]
        [Column("totpSecretKey")]
        public string? TotpSecretKey { get; set; }

        // Recovery codes for 2FA (stored as comma-separated hashed values)

        /// <summary>
        /// Gets or sets the RecoveryCodesHash
        /// </summary>
        [MaxLength(2048)]
        [Column("recoveryCodesHash")]
        public string? RecoveryCodesHash { get; set; }

        // Password Reset fields

        /// <summary>
        /// Gets or sets the PasswordResetTokenHash
        /// </summary>
        [MaxLength(256)]
        [Column("passwordResetTokenHash")]
        public string? PasswordResetTokenHash { get; set; }

        /// <summary>
        /// Gets or sets the PasswordResetTokenExpiry
        /// </summary>
        [Column("passwordResetTokenExpiry")]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Security: Last password change timestamp

        /// <summary>
        /// Gets or sets the LastPasswordChangeAt
        /// </summary>
        [Column("lastPasswordChangeAt")]
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user must change their password on next login.
        /// Used for newly created employee accounts with default passwords.
        /// </summary>
        [Column("mustChangePassword")]
        public bool MustChangePassword { get; set; } = false;

        // Navigation properties

        /// <summary>
        /// Gets or sets the Company
        /// </summary>
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        /// <summary>
        /// Gets or sets the PayrollApprovals
        /// </summary>
        public virtual ICollection<PayrollApproval> PayrollApprovals { get; set; } = new List<PayrollApproval>();

        /// <summary>
        /// Gets or sets the AuditLogs
        /// </summary>
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
