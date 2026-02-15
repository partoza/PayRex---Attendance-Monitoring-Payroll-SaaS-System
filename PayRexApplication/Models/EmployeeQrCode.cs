using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("employeeQrCodes")]
   public class EmployeeQrCode
    {
    [Key]
   [Column("qrId")]
        public int QrId { get; set; }

   [Required]
      [Column("employeeId")]
    public int EmployeeId { get; set; }

     [Required]
      [MaxLength(500)]
    [Column("qrValue")]
  public string QrValue { get; set; } = string.Empty;

    [Required]
     [Column("issuedAt")]
    public DateTime IssuedAt { get; set; }

  [Required]
  [Column("isActive")]
  public bool IsActive { get; set; } = true;

       [Column("expiresAt")]
      public DateTime? ExpiresAt { get; set; }

    // Navigation properties
        [ForeignKey("EmployeeId")]
  public virtual Employee Employee { get; set; } = null!;
    }
}
