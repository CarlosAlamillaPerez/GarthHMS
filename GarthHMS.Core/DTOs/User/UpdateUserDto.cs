using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.User
{
    /// <summary>
    /// DTO para actualizar un usuario existente
    /// </summary>
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public Guid RoleId { get; set; }

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [Url(ErrorMessage = "El formato de la URL no es válido")]
        [StringLength(500, ErrorMessage = "La URL de la foto no puede exceder 500 caracteres")]
        public string? PhotoUrl { get; set; }

        /// <summary>
        /// Estado activo/inactivo del usuario
        /// </summary>
        public bool IsActive { get; set; }
    }
}