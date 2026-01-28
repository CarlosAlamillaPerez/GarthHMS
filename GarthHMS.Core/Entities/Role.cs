using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Rol personalizado creado por un hotel
    /// Cada hotel puede definir roles con permisos específicos
    /// </summary>
    public class Role
    {
        public int RoleId { get; set; }
        public int HotelId { get; set; }

        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MaxDiscountPercent { get; set; } = 0;

        // PERMISOS (JSON array de PermissionType)
        public string PermissionsJson { get; set; } = "[]";

        public bool IsActive { get; set; } = true;

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
