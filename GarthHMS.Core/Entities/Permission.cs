using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Permiso del sistema
    /// Los permisos son globales (no tienen hotel_id)
    /// </summary>
    public class Permission
    {
        [Column("permission_id")]
        public Guid PermissionId { get; set; }

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("module")]
        public string Module { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}