using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.User
{
    /// <summary>
    /// DTO para cambiar la contraseña de un usuario
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}