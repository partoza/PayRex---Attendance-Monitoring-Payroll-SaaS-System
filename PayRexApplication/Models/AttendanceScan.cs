using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("attendanceScans")]
   public class AttendanceScan
    {
      [Key]
  [Column("scanId")]
        public int ScanId { get; set; }

        [Required]
 [Column("employeeId")]
        public int EmployeeId { get; set; }

    [Required]
 [Column("scanTime")]
        public DateTime ScanTime { get; set; }

       [Required]
  [Column("scanType")]
   public ScanType ScanType { get; set; }

        [MaxLength(100)]
    [Column("deviceId")]
  public string? DeviceId { get; set; }

    [Required]
   [Column("result")]
       public ScanResult Result { get; set; }

   [MaxLength(500)]
  [Column("remarks")]
public string? Remarks { get; set; }

        [Required]
  [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
      [ForeignKey("EmployeeId")]
   public virtual Employee Employee { get; set; } = null!;
    }
}
