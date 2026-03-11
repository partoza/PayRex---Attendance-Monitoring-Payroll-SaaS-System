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

        /// <summary>
        /// Number of hours after ScheduledTimeIn during which Time In is still accepted.
        /// After this window the Time In is locked. 0 = no cutoff.
        /// </summary>
        [Column("timeInCutoffHours")]
        public int TimeInCutoffHours { get; set; } = 0;

        // Work Schedule
        [Column("scheduledTimeIn")]
        public TimeSpan? ScheduledTimeIn { get; set; }

        [Column("scheduledTimeOut")]
        public TimeSpan? ScheduledTimeOut { get; set; }

        // Vacation Leave
        [Column("vacationLeaveDays")]
        public int VacationLeaveDays { get; set; } = 5;

        /// <summary>
        /// 0 = Monthly, 1 = Yearly
        /// </summary>
        [Column("vacationLeaveResetType")]
        public int VacationLeaveResetType { get; set; } = 0;

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
