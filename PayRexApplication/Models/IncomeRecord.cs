using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    [Table("incomeRecords")]
    public class IncomeRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("source")]
        public string Source { get; set; } = string.Empty;

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;
    }
}
