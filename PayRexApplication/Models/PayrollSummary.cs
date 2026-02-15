using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
 [Table("payrollSummaries")]
    public class PayrollSummary
    {
     [Key]
        [Column("payrollSummaryId")]
  public int PayrollSummaryId { get; set; }

 [Required]
      [Column("payrollPeriodId")]
  public int PayrollPeriodId { get; set; }

     [Required]
   [Column("employeeId")]
   public int EmployeeId { get; set; }

        [Required]
       [Column("grossPay", TypeName = "decimal(18,2)")]
 public decimal GrossPay { get; set; }

      [Required]
   [Column("totalDeductions", TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

  [Required]
    [Column("netPay", TypeName = "decimal(18,2)")]
 public decimal NetPay { get; set; }

        [Column("basicPay", TypeName = "decimal(18,2)")]
 public decimal? BasicPay { get; set; }

      [Column("overtimePay", TypeName = "decimal(18,2)")]
 public decimal? OvertimePay { get; set; }

 [Column("holidayPay", TypeName = "decimal(18,2)")]
   public decimal? HolidayPay { get; set; }

     [Column("allowances", TypeName = "decimal(18,2)")]
    public decimal? Allowances { get; set; }

   [Required]
    [Column("createdAt")]
  public DateTime CreatedAt { get; set; }

    [Column("updatedAt")]
  public DateTime? UpdatedAt { get; set; }

        // Navigation properties
      [ForeignKey("PayrollPeriodId")]
      public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

      [ForeignKey("EmployeeId")]
 public virtual Employee Employee { get; set; } = null!;

        public virtual Payslip? Payslip { get; set; }
    }
}
