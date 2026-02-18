using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("companySettings")]
    public class CompanySetting
    {
        [Key]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Required]
        [Column("payrollCycle")]
        public PayrollCycle PayrollCycle { get; set; }

        // Work hours can be derived from payroll frequency; allow null to use defaults
        [Column("workHoursPerDay", TypeName = "decimal(4,2)")]
        public decimal? WorkHoursPerDay { get; set; } = 8.0m;

        [Required]
        [Column("overtimeRate", TypeName = "decimal(4,2)")]
        public decimal OvertimeRate { get; set; } = 1.25m;

        [Required]
        [Column("lateGraceMinutes")]
        public int LateGraceMinutes { get; set; } = 15;

        [Column("holidayRate", TypeName = "decimal(4,2)")]
        public decimal HolidayRate { get; set; } = 2.00m;

        // Absent rate (multiplier or deduction percent) added to align with UI
        [Column("absentRate", TypeName = "decimal(4,2)")]
        public decimal AbsentRate { get; set; } = 0.00m;

        [Column("rolesJson")]
        [MaxLength(4000)]
        public string? RolesJson { get; set; }

        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;
    }
}
