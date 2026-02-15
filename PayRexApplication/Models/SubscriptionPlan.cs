using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("subscriptionPlans")]
    public class SubscriptionPlan
    {
     [Key]
        [Column("planId")]
  public int PlanId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
  public string Name { get; set; } = string.Empty;

        [Required]
        [Column("maxEmployees")]
        public int MaxEmployees { get; set; }

  [Required]
        [Column("price", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

      [Required]
        [Column("billingCycle")]
     public BillingCycle BillingCycle { get; set; }

        [Required]
        [Column("status")]
   public PlanStatus Status { get; set; }

        [Column("description")]
        [MaxLength(500)]
 public string? Description { get; set; }

        /// <summary>
        /// Maximum number of admin/HR users allowed for the plan. Null or -1 = unlimited.
        /// </summary>
  [Column("planUserLimit")]
        public int? PlanUserLimit { get; set; }

        [Required]
        [Column("createdAt")]
     public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
      public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
      public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
