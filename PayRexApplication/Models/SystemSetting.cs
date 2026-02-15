using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    /// <summary>
    /// Global system settings for government contribution percentages and platform configuration.
    /// </summary>
    [Table("systemSettings")]
    public class SystemSetting
    {
        [Key]
    [Column("settingId")]
        public int SettingId { get; set; }

        [Required]
        [Column("sssPercentage", TypeName = "decimal(8,4)")]
        public decimal SssPercentage { get; set; }

        [Required]
        [Column("pagIbigPercentage", TypeName = "decimal(8,4)")]
        public decimal PagIbigPercentage { get; set; }

        [Required]
        [Column("philHealthPercentage", TypeName = "decimal(8,4)")]
        public decimal PhilHealthPercentage { get; set; }

        [Required]
        [Column("effectiveDate")]
        public DateTime EffectiveDate { get; set; }

     [MaxLength(1000)]
   [Column("note")]
        public string? Note { get; set; }

  [Required]
 [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

      [Column("updatedAt")]
   public DateTime? UpdatedAt { get; set; }
    }
}
