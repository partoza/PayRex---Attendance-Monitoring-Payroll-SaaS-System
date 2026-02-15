using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    /// <summary>
    /// Tracks login attempts for account lockout protection and audit
    /// </summary>
    [Table("userLoginAttempts")]
    public class UserLoginAttempt
    {
        [Key]
        [Column("attemptId")]
        public int AttemptId { get; set; }

        [Column("userId")]
   public int? UserId { get; set; }

[Required]
     [MaxLength(256)]
        [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(45)]
   [Column("ipAddress")]
     public string? IpAddress { get; set; }

 /// <summary>
        /// Whether the login attempt was successful.
        /// </summary>
  [Column("success")]
        public bool Success { get; set; }

 /// <summary>
        /// Reason for failure (e.g. "InvalidCredentials", "AccountLocked", "Suspended").
        /// </summary>
        [MaxLength(256)]
        [Column("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// User-Agent header from the login request.
        /// </summary>
     [MaxLength(1024)]
        [Column("userAgent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Timestamp of this specific attempt.
  /// </summary>
[Column("timestamp")]
        public DateTime Timestamp { get; set; }

 [Required]
        [Column("attemptCount")]
        public int AttemptCount { get; set; }

   [Required]
  [Column("isLocked")]
      public bool IsLocked { get; set; }

        [Column("lockUntil")]
     public DateTime? LockUntil { get; set; }

        [Column("lastAttemptAt")]
        public DateTime? LastAttemptAt { get; set; }

        [Required]
        [Column("createdAt")]
public DateTime CreatedAt { get; set; }

        // Navigation property (optional - user may not exist yet)
      [ForeignKey("UserId")]
      public virtual User? User { get; set; }
    }
}
