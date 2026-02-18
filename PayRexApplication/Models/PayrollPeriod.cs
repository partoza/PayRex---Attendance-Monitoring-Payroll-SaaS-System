using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("payrollPeriods")]
    public class PayrollPeriod
  {
    [Key]
  [Column("payrollPeriodId")]
     public int PayrollPeriodId { get; set; }

  [Required]
     [Column("companyId")]
        public int CompanyId { get; set; }

      [Required]
     [Column("startDate")]
  public DateOnly StartDate { get; set; }

   [Required]
        [Column("endDate")]
public DateOnly EndDate { get; set; }

      [Required]
     [Column("status")]
        public PayrollPeriodStatus Status { get; set; }

  [MaxLength(100)]
    [Column("periodName")]
   public string? PeriodName { get; set; }

[Required]
 [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

     [Column("updatedAt")]
   public DateTime? UpdatedAt { get; set; }

        // Navigation properties
    [ForeignKey("CompanyId")]
     public virtual Company Company { get; set; } = null!;

      public virtual ICollection<PayrollSummary> PayrollSummaries { get; set; } = new List<PayrollSummary>();
  public virtual ICollection<PayrollApproval> PayrollApprovals { get; set; } = new List<PayrollApproval>();
     public virtual ICollection<GovernmentContribution> GovernmentContributions { get; set; } = new List<GovernmentContribution>();
    }
}
