using System.ComponentModel.DataAnnotations;

namespace PayRexApplication.DTOs
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

 // Payroll settings
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

 // Roles JSON for frontend
 public string? RolesJson { get; set; }
 }

 public class UpdateCompanyProfileDto
 {
 [Required]
 public string CompanyName { get; set; } = string.Empty;

 [MaxLength(1000)]
 public string? Address { get; set; }

 [EmailAddress]
 [MaxLength(256)]
 public string? ContactEmail { get; set; }

 [MaxLength(50)]
 public string? ContactPhone { get; set; }

 [MaxLength(50)]
 public string? Tin { get; set; }

 [MaxLength(512)]
 public string? LogoUrl { get; set; }

 [MaxLength(512)]
 public string? OwnerSignatureUrl { get; set; }

 // Payroll settings
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

 // Roles
 public string? RolesJson { get; set; }
 }
}
