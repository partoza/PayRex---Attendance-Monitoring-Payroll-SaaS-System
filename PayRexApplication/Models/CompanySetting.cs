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
        [MaxLength(4)]
        [Column("companyId")]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [Column("payrollCycle")]
        public PayrollCycle PayrollCycle { get; set; }

        [Required]
        [Column("workHoursPerDay", TypeName = "decimal(4,2)")]
        public decimal WorkHoursPerDay { get; set; } = 8.0m;

        [Required]
        [Column("overtimeRate", TypeName = "decimal(4,2)")]
        public decimal OvertimeRate { get; set; } = 1.25m;

        [Required]
        [Column("lateGraceMinutes")]
        public int LateGraceMinutes { get; set; } = 15;

        [Column("nightDifferentialRate", TypeName = "decimal(4,2)")]
        public decimal NightDifferentialRate { get; set; } = 1.10m;

        [Column("holidayRate", TypeName = "decimal(4,2)")]
        public decimal HolidayRate { get; set; } = 2.00m;

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
