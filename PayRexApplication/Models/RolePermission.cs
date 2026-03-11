using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayRexApplication.Models
{
    /// <summary>
    /// Stores per-role, per-module permissions (Add, Update, Inactivate).
    /// Managed by SuperAdmin.
    /// </summary>
    [Table("rolePermissions")]
    public class RolePermission
    {
        [Key]
        [Column("permissionId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("roleName")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("moduleName")]
        public string ModuleName { get; set; } = string.Empty;

        [Column("canAdd")]
        public bool CanAdd { get; set; }

        [Column("canUpdate")]
        public bool CanUpdate { get; set; }

        [Column("canInactivate")]
        public bool CanInactivate { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
