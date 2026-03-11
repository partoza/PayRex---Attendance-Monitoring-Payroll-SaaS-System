using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("billingInvoices")]
    public class BillingInvoice
 {
      [Key]
     [Column("invoiceId")]
        public int InvoiceId { get; set; }

   [Required]
       [Column("companyId")]
 public int CompanyId { get; set; }

    [Required]
     [Column("amount", TypeName = "decimal(18,2)")]
     public decimal Amount { get; set; }

  [Required]
 [Column("dueDate")]
        public DateTime DueDate { get; set; }

   [Required]
  [Column("status")]
        public InvoiceStatus Status { get; set; }

     [MaxLength(100)]
        [Column("invoiceNumber")]
 public string? InvoiceNumber { get; set; }

      [Required]
 [Column("createdAt")]
  public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [Column("subscriptionId")]
        public int? SubscriptionId { get; set; }

        [Column("periodStart")]
        public DateTime? PeriodStart { get; set; }

        [Column("periodEnd")]
        public DateTime? PeriodEnd { get; set; }

        [Column("paidAt")]
        public DateTime? PaidAt { get; set; }

        [Column("vatAmount", TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
