using System;

namespace GarthHMS.Core.DTOs.Role
{
    public class RoleResponseDto
    {
        public Guid RoleId { get; set; }
        public Guid HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxDiscountPercent { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsManagerRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Propiedades calculadas para la UI
        public string StatusBadge => IsActive ? "Activo" : "Inactivo";
        public string StatusColor => IsActive ? "success" : "secondary";
        public string RoleTypeBadge => IsSystemRole ? "Sistema" : (IsManagerRole ? "Gerencial" : "Personalizado");
        public string RoleTypeColor => IsSystemRole ? "primary" : (IsManagerRole ? "warning" : "info");
        public string DiscountDisplay => $"{MaxDiscountPercent}%";
    }
}