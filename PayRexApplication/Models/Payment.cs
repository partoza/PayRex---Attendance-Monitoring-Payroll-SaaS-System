using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("paymentId")]
  public int PaymentId { get; set; }

 [Required]
   [Column("invoiceId")]
        public int InvoiceId { get; set; }

  [Required]
 [MaxLength(100)]
      [Column("provider")]
  public string Provider { get; set; } = string.Empty;

      [Required]
[MaxLength(100)]
        [Column("referenceNo")]
        public string ReferenceNo { get; set; } = string.Empty;

   [Required]
   [Column("status")]
        public PaymentStatus Status { get; set; }

       [Column("paidAt")]
      public DateTime? PaidAt { get; set; }

[Column("amount", TypeName = "decimal(18,2)")]
 public decimal? Amount { get; set; }

   [Required]
  [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

     // Navigation properties
        [ForeignKey("InvoiceId")]
 public virtual BillingInvoice BillingInvoice { get; set; } = null!;
    }
}
