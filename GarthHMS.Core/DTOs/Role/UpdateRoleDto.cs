using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.Role
{
    public class UpdateRoleDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe estar entre 0 y 100")]
        public int MaxDiscountPercent { get; set; } = 0;
    }
}