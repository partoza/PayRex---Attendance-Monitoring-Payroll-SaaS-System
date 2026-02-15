using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("auditLogs")]
    public class AuditLog
    {
   [Key]
[Column("auditId")]
        public int AuditId { get; set; }

        [MaxLength(4)]
      [Column("companyId")]
        public string? CompanyId { get; set; }

        [Column("userId")]
        public int? UserId { get; set; }

        /// <summary>
        /// The role of the user who performed the action
     /// </summary>
        [MaxLength(50)]
    [Column("role")]
        public string? Role { get; set; }

   [Required]
   [MaxLength(100)]
      [Column("action")]
      public string Action { get; set; } = string.Empty;

     /// <summary>
        /// The entity that was affected by the action (e.g., "User", "Employee", "Company")
        /// </summary>
        [MaxLength(200)]
      [Column("entityAffected")]
        public string? EntityAffected { get; set; }

    [MaxLength(100)]
        [Column("target")]
        public string? Target { get; set; }

        [MaxLength(500)]
     [Column("targetId")]
        public string? TargetId { get; set; }

        [Column("oldValues")]
        public string? OldValues { get; set; }

       [Column("newValues")]
        public string? NewValues { get; set; }

  [MaxLength(50)]
     [Column("ipAddress")]
 public string? IpAddress { get; set; }

        [MaxLength(500)]
   [Column("userAgent")]
     public string? UserAgent { get; set; }

      [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

   // Navigation properties
   [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
