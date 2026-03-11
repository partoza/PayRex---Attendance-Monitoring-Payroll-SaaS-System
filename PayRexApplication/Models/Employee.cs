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
        [Column("employeeNumber")]
        public int EmployeeNumber { get; set; }

        [Required]
        [Column("companyId")]
        public int CompanyId { get; set; }

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

        [Required]
        [MaxLength(256)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("contactNumber")]
        public string? ContactNumber { get; set; }

        [MaxLength(50)]
        [Column("civilStatus")]
        public string? CivilStatus { get; set; }

        [Column("birthdate")]
        public DateTime? Birthdate { get; set; }

        [Required]
        [Column("startDate")]
        public DateTime StartDate { get; set; }

        // SalaryRate removed from Employee - use role rates / payroll tables instead

        [Required]
        [Column("status")]
        public EmployeeStatus Status { get; set; }

        /// <summary>
        /// Monthly salary rate (used for payroll computation)
        /// </summary>
        [Column("salaryRate", TypeName = "decimal(18,2)")]
        public decimal SalaryRate { get; set; }

        // Government Identifiers
        [MaxLength(15)]
        [Column("tin")]
        public string? TIN { get; set; }

        [MaxLength(12)]
        [Column("sss")]
        public string? SSS { get; set; }

        [MaxLength(14)]
        [Column("philHealth")]
        public string? PhilHealth { get; set; }

        [MaxLength(14)]
        [Column("pagIbig")]
        public string? PagIbig { get; set; }

        // Profile and signature moved to User model; Employee no longer stores these URLs

        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Role FK
        [Column("roleId")]
        public int? RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual EmployeeRole? Role { get; set; }

        // User FK - links employee to their User account
        [Column("userId")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

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
