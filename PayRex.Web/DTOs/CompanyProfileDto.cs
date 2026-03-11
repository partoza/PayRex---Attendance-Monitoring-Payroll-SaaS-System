using System.Collections.Generic;

namespace PayRex.Web.DTOs
{
    public class CompanyProfileDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Tin { get; set; }
        public string? LogoUrl { get; set; }
        public string? OwnerSignatureUrl { get; set; }

        // Payroll
        public int? PayrollCycle { get; set; }
        public decimal? WorkHoursPerDay { get; set; }
        public decimal? OvertimeRate { get; set; }
        public int? LateGraceMinutes { get; set; }
        public decimal? HolidayRate { get; set; }

        // Work Schedule
        public string? ScheduledTimeIn { get; set; }
        public string? ScheduledTimeOut { get; set; }
        public int? TimeInCutoffHours { get; set; }

        // Vacation Leave
        public int? VacationLeaveDays { get; set; }
        public int? VacationLeaveResetType { get; set; }

        // Roles stored as JSON on server; client can map to UI List<RoleConfig>
        public string? RolesJson { get; set; }
    }

    public class UpdateCompanyRequestDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Tin { get; set; }
        public string? LogoUrl { get; set; }
        public string? OwnerSignatureUrl { get; set; }

        // Payroll
        public int? PayrollCycle { get; set; }
        public decimal? WorkHoursPerDay { get; set; }
        public decimal? OvertimeRate { get; set; }
        public int? LateGraceMinutes { get; set; }
        public decimal? HolidayRate { get; set; }

        // Work Schedule
        public string? ScheduledTimeIn { get; set; }
        public string? ScheduledTimeOut { get; set; }
        public int? TimeInCutoffHours { get; set; }

        // Vacation Leave
        public int? VacationLeaveDays { get; set; }
        public int? VacationLeaveResetType { get; set; }

        // Roles as JSON
        public string? RolesJson { get; set; }
    }
}
