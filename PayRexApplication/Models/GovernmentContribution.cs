using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("governmentContributions")]
  public class GovernmentContribution
    {
  [Key]
   [Column("contributionId")]
     public int ContributionId { get; set; }

       [Required]
        [Column("employeeId")]
      public int EmployeeId { get; set; }

       [Required]
      [Column("payrollPeriodId")]
        public int PayrollPeriodId { get; set; }

 [Required]
      [Column("type")]
   public ContributionType Type { get; set; }

    [Required]
    [Column("employeeShare", TypeName = "decimal(18,2)")]
       public decimal EmployeeShare { get; set; }

       [Required]
        [Column("employerShare", TypeName = "decimal(18,2)")]
    public decimal EmployerShare { get; set; }

   [Required]
  [Column("createdAt")]
 public DateTime CreatedAt { get; set; }

        // Navigation properties
       [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

  [ForeignKey("PayrollPeriodId")]
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
 }
}
