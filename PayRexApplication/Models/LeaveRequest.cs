using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayRexApplication.Enums;

namespace PayRexApplication.Models
{
    [Table("leaveRequests")]
    public class LeaveRequest
    {
        [Key]
        [Column("leaveRequestId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeaveRequestId { get; set; }

        [Required]
        [Column("employeeId")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Required]
        [Column("leaveType")]
        public LeaveType LeaveType { get; set; }

        [Required]
        [Column("startDate")]
        public DateOnly StartDate { get; set; }

        [Required]
        [Column("endDate")]
        public DateOnly EndDate { get; set; }

        [Required]
        [Column("totalDays")]
        public int TotalDays { get; set; }

        [MaxLength(500)]
        [Column("reason")]
        public string? Reason { get; set; }

        [Required]
        [Column("status")]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [Required]
        [Column("isArchived")]
        public bool IsArchived { get; set; } = false;

        [Column("reviewedBy")]
        public int? ReviewedBy { get; set; }

        [MaxLength(500)]
        [Column("reviewRemarks")]
        public string? ReviewRemarks { get; set; }

        [Column("reviewedAt")]
        public DateTime? ReviewedAt { get; set; }

        [Required]
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual User? Reviewer { get; set; }
    }
}
