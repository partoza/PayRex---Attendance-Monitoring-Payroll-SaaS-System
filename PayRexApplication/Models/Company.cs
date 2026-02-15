using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("companies")]
    public class Company
  {
        [Key]
        [MaxLength(4)]
    [Column("companyId")]
        public string CompanyId { get; set; } = string.Empty;

   [Required]
        [MaxLength(200)]
      [Column("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Column("planId")]
        public int PlanId { get; set; }

        [Required]
        [Column("status")]
        public CompanyStatus Status { get; set; }

[Required]
        [Column("createdAt")]
      public DateTime CreatedAt { get; set; }

      [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Whether the company is active. When false, all associated users are also deactivated.
        /// </summary>
  [Required]
        [Column("isActive")]
        public bool IsActive { get; set; } = true;

 // Navigation properties
      [ForeignKey("PlanId")]
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        public virtual CompanySetting? CompanySetting { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
public virtual ICollection<PayrollPeriod> PayrollPeriods { get; set; } = new List<PayrollPeriod>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
  }
}
