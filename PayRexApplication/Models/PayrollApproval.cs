using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("payrollApprovals")]
 public class PayrollApproval
    {
   [Key]
 [Column("approvalId")]
    public int ApprovalId { get; set; }

  [Required]
     [Column("payrollPeriodId")]
        public int PayrollPeriodId { get; set; }

       [Required]
 [Column("approvedBy")]
        public int ApprovedBy { get; set; }

   [Column("approvedAt")]
 public DateTime? ApprovedAt { get; set; }

 [Required]
     [Column("status")]
        public ApprovalStatus Status { get; set; }

     [MaxLength(500)]
     [Column("remarks")]
        public string? Remarks { get; set; }

   [Required]
    [Column("createdAt")]
      public DateTime CreatedAt { get; set; }

 // Navigation properties
   [ForeignKey("PayrollPeriodId")]
       public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

      [ForeignKey("ApprovedBy")]
  public virtual User ApprovedByUser { get; set; } = null!;
    }
}
