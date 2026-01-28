using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Usuario del sistema (SuperAdmin o usuario de hotel)
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public int? HotelId { get; set; }  // NULL = SuperAdmin

        // INFORMACIÓN PERSONAL
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }

        // AUTENTICACIÓN
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;

        // ROL Y PERMISOS
        public UserRole UserRole { get; set; }
        public int? CustomRoleId { get; set; }  // Si UserRole = Custom
        public decimal MaxDiscountPercent { get; set; } = 0;  // Límite de descuento sin aprobación

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
