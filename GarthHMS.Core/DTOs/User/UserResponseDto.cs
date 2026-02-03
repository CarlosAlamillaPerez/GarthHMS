using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.DTOs.User
{
    /// <summary>
    /// DTO de respuesta para mostrar información de un usuario
    /// Incluye información del rol y permisos
    /// </summary>
    public class UserResponseDto
    {
        // IDs
        public Guid UserId { get; set; }
        public Guid HotelId { get; set; }
        public Guid RoleId { get; set; }

        // Autenticación
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }

        // Información Personal
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }

        // Información del Rol (JOIN con tabla role)
        public string RoleName { get; set; } = string.Empty;
        public int MaxDiscountPercent { get; set; }
        public bool IsManagerRole { get; set; }

        // Estado y Seguridad
        public bool IsActive { get; set; }
        public string IsActiveText { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public string LastLoginText { get; set; } = string.Empty;
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool IsLocked { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }
}