using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("employees")]
    public class Employee
    {
  [Key]
     [Column("employeeId")]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(4)]
      [Column("companyId")]
     public string CompanyId { get; set; } = string.Empty;

        [Required]
     [MaxLength(50)]
   [Column("employeeCode")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
  [MaxLength(100)]
 [Column("firstName")]
      public string FirstName { get; set; } = string.Empty;

      [Required]
   [MaxLength(100)]
   [Column("lastName")]
public string LastName { get; set; } = string.Empty;

       [MaxLength(100)]
   [Column("position")]
        public string? Position { get; set; }

        [MaxLength(100)]
     [Column("department")]
 public string? Department { get; set; }

   [Required]
        [Column("salaryRate", TypeName = "decimal(18,2)")]
     public decimal SalaryRate { get; set; }

        [Required]
     [Column("employmentType")]
      public EmploymentType EmploymentType { get; set; }

    [Required]
     [Column("status")]
     public EmployeeStatus Status { get; set; }

        [MaxLength(256)]
  [Column("email")]
        public string? Email { get; set; }

     [MaxLength(50)]
    [Column("phoneNumber")]
  public string? PhoneNumber { get; set; }

 [Column("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

     [Column("hireDate")]
        public DateTime? HireDate { get; set; }

     [Required]
        [Column("createdAt")]
  public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
      [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

     public virtual EmployeeQrCode? EmployeeQrCode { get; set; }
   public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
     public virtual ICollection<AttendanceScan> AttendanceScans { get; set; } = new List<AttendanceScan>();
        public virtual ICollection<PayrollSummary> PayrollSummaries { get; set; } = new List<PayrollSummary>();
   public virtual ICollection<GovernmentContribution> GovernmentContributions { get; set; } = new List<GovernmentContribution>();
        public virtual ICollection<EmployeeDeduction> EmployeeDeductions { get; set; } = new List<EmployeeDeduction>();
     public virtual ICollection<EmployeeBenefit> EmployeeBenefits { get; set; } = new List<EmployeeBenefit>();
    }
}
