using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.User;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Interface para el servicio de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(
            CreateUserDto dto,
            Guid createdBy);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(UpdateUserDto dto);

        /// <summary>
        /// Elimina un usuario (soft delete)
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId);

        // ====================================================================
        // CONSULTAS
        // ====================================================================

        /// <summary>
        /// Obtiene un usuario por su ID con información completa
        /// </summary>
        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        Task<UserResponseDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Obtiene todos los usuarios de un hotel
        /// </summary>
        Task<List<UserResponseDto>> GetUsersByHotelAsync(Guid hotelId);

        /// <summary>
        /// Obtiene solo los usuarios activos de un hotel
        /// </summary>
        Task<List<UserResponseDto>> GetActiveUsersAsync(Guid hotelId);

        // ====================================================================
        // GESTIÓN DE ESTADO
        // ====================================================================

        /// <summary>
        /// Activa un usuario
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> ActivateUserAsync(Guid userId);

        /// <summary>
        /// Desactiva un usuario
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> DeactivateUserAsync(Guid userId);

        // ====================================================================
        // GESTIÓN DE ROLES
        // ====================================================================

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> AssignRoleAsync(Guid userId, Guid roleId);

        // ====================================================================
        // GESTIÓN DE CONTRASEÑAS
        // ====================================================================

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto);

        /// <summary>
        /// Restablece la contraseña de un usuario (por un administrador)
        /// </summary>
        Task<(bool Success, string NewPassword, string? ErrorMessage)> ResetPasswordAsync(Guid userId);

        // ====================================================================
        // PERMISOS
        // ====================================================================

        /// <summary>
        /// Obtiene los permisos efectivos de un usuario (heredados de su rol)
        /// </summary>
        Task<List<string>> GetEffectivePermissionsAsync(Guid userId);

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        /// <summary>
        /// Verifica si un email ya existe
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    }
}