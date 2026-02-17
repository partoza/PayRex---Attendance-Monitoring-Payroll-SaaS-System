using System.ComponentModel.DataAnnotations;

namespace PayRexApplication.DTOs
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

 // Payroll settings
 public int? PayrollCycle { get; set; }
 public decimal? WorkHoursPerDay { get; set; }
 public decimal? OvertimeRate { get; set; }
 public int? LateGraceMinutes { get; set; }
 public decimal? HolidayRate { get; set; }
 public decimal? AbsentRate { get; set; }

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
	public string? UrlImage { get; set; }

 // Payroll settings
 public int? PayrollCycle { get; set; }
 public decimal? WorkHoursPerDay { get; set; }
 public decimal? OvertimeRate { get; set; }
 public int? LateGraceMinutes { get; set; }
 public decimal? HolidayRate { get; set; }
 public decimal? AbsentRate { get; set; }

 // Roles
 public string? RolesJson { get; set; }
 }
}
