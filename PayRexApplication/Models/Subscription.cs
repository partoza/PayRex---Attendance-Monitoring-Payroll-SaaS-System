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

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    }
}
