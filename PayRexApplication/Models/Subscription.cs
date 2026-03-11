using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("subscriptions")]
    public class Subscription
    {
        [Key]
        [Column("subscriptionId")]
        public int SubscriptionId { get; set; }

        [Required]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Required]
        [Column("planId")]
        public int PlanId { get; set; }

        [Required]
        [Column("startDate")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("endDate")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("status")]
        public SubscriptionStatus Status { get; set; }

        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [Column("autoRenew")]
        public bool AutoRenew { get; set; } = true;

        [Column("gracePeriodDays")]
        public int GracePeriodDays { get; set; } = 7;

        [Column("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [Column("lastPaymentId")]
        public int? LastPaymentId { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        [ForeignKey("LastPaymentId")]
        public virtual Payment? LastPayment { get; set; }
    }
}
