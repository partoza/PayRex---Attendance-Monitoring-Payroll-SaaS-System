using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("payslips")]
    public class Payslip
    {
        [Key]
     [Column("payslipId")]
        public int PayslipId { get; set; }

    [Required]
        [Column("payrollSummaryId")]
        public int PayrollSummaryId { get; set; }

        [MaxLength(500)]
        [Column("pdfPath")]
        public string? PdfPath { get; set; }

        [Required]
     [Column("generatedAt")]
        public DateTime GeneratedAt { get; set; }

        [Required]
        [Column("released")]
  public bool Released { get; set; } = false;

  [Column("releasedAt")]
        public DateTime? ReleasedAt { get; set; }

        [Column("isArchived")]
        public bool IsArchived { get; set; } = false;

        [Column("archivedAt")]
        public DateTime? ArchivedAt { get; set; }

        // Navigation properties
        [ForeignKey("PayrollSummaryId")]
        public virtual PayrollSummary PayrollSummary { get; set; } = null!;
    }
}
