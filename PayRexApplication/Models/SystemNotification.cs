using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("systemNotifications")]
    public class SystemNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("notificationId")]
        public int NotificationId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("type")]
        public string Type { get; set; } = "info"; // info, warning, success

        [MaxLength(200)]
        [Column("targetRoles")]
        public string? TargetRoles { get; set; } // comma-separated: "Admin,HR,Employee" or null = all

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("createdBy")]
        public int? CreatedBy { get; set; }
    }
}
