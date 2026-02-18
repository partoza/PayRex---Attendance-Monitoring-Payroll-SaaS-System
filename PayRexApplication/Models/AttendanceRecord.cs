using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("attendanceRecords")]
   public class AttendanceRecord
    {
   [Key]
        [Column("attendanceId")]
  public int AttendanceId { get; set; }

     [Required]
       [Column("employeeId")]
        public int EmployeeId { get; set; }

     [Required]
      [Column("companyId")]
       public int CompanyId { get; set; }

   [Required]
        [Column("date")]
        public DateOnly Date { get; set; }

        [Column("timeIn")]
    public TimeOnly? TimeIn { get; set; }

    [Column("timeOut")]
 public TimeOnly? TimeOut { get; set; }

        [Required]
   [Column("source")]
        public AttendanceSource Source { get; set; }

   [Required]
  [Column("locked")]
  public bool Locked { get; set; } = false;

    [Column("totalHoursWorked", TypeName = "decimal(5,2)")]
        public decimal? TotalHoursWorked { get; set; }

    [Column("overtimeHours", TypeName = "decimal(5,2)")]
   public decimal? OvertimeHours { get; set; }

 [Column("lateMinutes")]
 public int? LateMinutes { get; set; }

       [Column("undertimeMinutes")]
        public int? UndertimeMinutes { get; set; }

      [Required]
  [Column("createdAt")]
      public DateTime CreatedAt { get; set; }

    [Column("updatedAt")]
      public DateTime? UpdatedAt { get; set; }

        // Navigation properties
    [ForeignKey("EmployeeId")]
     public virtual Employee Employee { get; set; } = null!;

     [ForeignKey("CompanyId")]
     public virtual Company Company { get; set; } = null!;
    }
}
