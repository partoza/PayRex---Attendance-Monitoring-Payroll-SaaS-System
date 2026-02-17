namespace PayRexApplication.DTOs
{
 public class EmployeeRoleDto
 {
 public int RoleId { get; set; }
 public string RoleName { get; set; } = string.Empty;
 public decimal? BasicRate { get; set; }
 public string? RateType { get; set; }
 public string? Description { get; set; }
 }
}
