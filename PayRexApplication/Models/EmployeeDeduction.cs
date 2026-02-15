using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("employeeDeductions")]
    public class EmployeeDeduction
    {
        [Key]
    [Column("deductionId")]
        public int DeductionId { get; set; }

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

  [Required]
        [Column("recurring")]
     public bool Recurring { get; set; } = false;

        [Column("startDate")]
   public DateOnly? StartDate { get; set; }

  [Column("endDate")]
        public DateOnly? EndDate { get; set; }

 [MaxLength(500)]
        [Column("description")]
       public string? Description { get; set; }

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
