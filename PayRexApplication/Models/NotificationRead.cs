using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("notificationReads")]
    public class NotificationRead
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("userId")]
        public int UserId { get; set; }

        [Required]
        [Column("notificationId")]
        public int NotificationId { get; set; }

        [Column("readAt")]
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("NotificationId")]
        public SystemNotification? Notification { get; set; }
    }
}
