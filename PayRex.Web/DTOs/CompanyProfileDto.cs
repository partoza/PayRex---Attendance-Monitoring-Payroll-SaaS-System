using System.Collections.Generic;

namespace PayRex.Web.DTOs
{
    public class CompanyProfileDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Tin { get; set; }
        public string? LogoUrl { get; set; }
        public string? UrlImage { get; set; }

        // Payroll
        public int? PayrollCycle { get; set; }
        public decimal? WorkHoursPerDay { get; set; }
        public decimal? OvertimeRate { get; set; }
        public int? LateGraceMinutes { get; set; }
        public decimal? HolidayRate { get; set; }
        public decimal? AbsentRate { get; set; }

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
        public string? UrlImage { get; set; }

        // Payroll
        public int? PayrollCycle { get; set; }
        public decimal? WorkHoursPerDay { get; set; }
        public decimal? OvertimeRate { get; set; }
        public int? LateGraceMinutes { get; set; }
        public decimal? HolidayRate { get; set; }
        public decimal? AbsentRate { get; set; }

        // Roles as JSON
        public string? RolesJson { get; set; }
    }
}
