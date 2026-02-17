using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
 [Table("employeeRoles")]
 public class EmployeeRole
 {
 [Key]
 [Column("roleId")]
 public int RoleId { get; set; }

 [Required]
 [MaxLength(4)]
 [Column("companyId")]
 public string CompanyId { get; set; } = string.Empty;

 [Required]
 [MaxLength(100)]
 [Column("roleName")]
 public string RoleName { get; set; } = string.Empty;

 [Column("basicRate", TypeName = "decimal(18,2)")]
 public decimal? BasicRate { get; set; }

 [MaxLength(50)]
 [Column("rateType")]
 public string? RateType { get; set; }

 [MaxLength(500)]
 [Column("description")]
 public string? Description { get; set; }

 [Required]
 [Column("isActive")]
 public bool IsActive { get; set; } = true;

 [Required]
 [Column("createdAt")]
 public DateTime CreatedAt { get; set; }

 [Column("updatedAt")]
 public DateTime? UpdatedAt { get; set; }

 // Navigation
 [ForeignKey("CompanyId")]
 public virtual Company Company { get; set; } = null!;

 // Employees with this role
 public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
 }
}
