using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
 [Table("employeeBenefits")]
  public class EmployeeBenefit
    {
  [Key]
        [Column("benefitId")]
    public int BenefitId { get; set; }

       [Required]
 [Column("employeeId")]
        public int EmployeeId { get; set; }

   [Required]
     [MaxLength(100)]
 [Column("name")]
public string Name { get; set; } = string.Empty;

      [Required]
        [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

      [MaxLength(500)]
        [Column("description")]
  public string? Description { get; set; }

   [Column("isTaxable")]
  public bool IsTaxable { get; set; } = false;

   [Required]
        [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
     [ForeignKey("EmployeeId")]
       public virtual Employee Employee { get; set; } = null!;
    }
}
