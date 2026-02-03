using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.User;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuthService authService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _authService = authService;
            _logger = logger;
        }

        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        public async Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(
            CreateUserDto dto,
            Guid createdBy)
        {
            try
            {
                // Validar que el email no exista
                var emailExists = await _userRepository.EmailExistsAsync(dto.Email);
                if (emailExists)
                {
                    return (false, Guid.Empty, "El email ya está registrado");
                }

                // Validar que el rol exista
                var role = await _roleRepository.GetByIdAsync(dto.RoleId);
                if (role == null)
                {
                    return (false, Guid.Empty, "El rol especificado no existe");
                }

                // Hashear la contraseña
                var passwordHash = await _authService.HashPasswordAsync(dto.Password);

                // Crear el usuario
                var user = new User
                {
                    HotelId = role.HotelId, // El hotel se obtiene del rol
                    RoleId = dto.RoleId,
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    MustChangePassword = dto.MustChangePassword,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Phone = dto.Phone,
                    PhotoUrl = dto.PhotoUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                var userId = await _userRepository.CreateAsync(user);

                if (userId != Guid.Empty)
                {
                    _logger.LogInformation("Usuario creado exitosamente: {Email}", dto.Email);
                    return (true, userId, null);
                }

                return (false, Guid.Empty, "Error al crear el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Email}", dto.Email);
                return (false, Guid.Empty, "Error al crear el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(UpdateUserDto dto)
        {
            try
            {
                // Verificar que el usuario exista
                var existingUser = await _userRepository.GetByIdAsync(dto.UserId);
                if (existingUser == null)
                {
                    return (false, "Usuario no encontrado");
                }

                // Validar que el email no esté en uso por otro usuario
                var emailExists = await _userRepository.EmailExistsAsync(dto.Email, dto.UserId);
                if (emailExists)
                {
                    return (false, "El email ya está en uso por otro usuario");
                }

                // Validar que el rol exista
                var role = await _roleRepository.GetByIdAsync(dto.RoleId);
                if (role == null)
                {
                    return (false, "El rol especificado no existe");
                }

                // Actualizar los datos
                existingUser.Username = dto.Username;
                existingUser.Email = dto.Email;
                existingUser.FirstName = dto.FirstName;
                existingUser.LastName = dto.LastName;
                existingUser.Phone = dto.Phone;
                existingUser.PhotoUrl = dto.PhotoUrl;
                existingUser.RoleId = dto.RoleId;
                existingUser.IsActive = dto.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);

                _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", dto.UserId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {UserId}", dto.UserId);
                return (false, "Error al actualizar el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                await _userRepository.DeleteAsync(userId);

                _logger.LogInformation("Usuario eliminado (soft delete): {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {UserId}", userId);
                return (false, "Error al eliminar el usuario");
            }
        }

        // ====================================================================
        // CONSULTAS
        // ====================================================================

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                return await MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario: {UserId}", userId);
                return null;
            }
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                    return null;

                return await MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
                return null;
            }
        }

        public async Task<List<UserResponseDto>> GetUsersByHotelAsync(Guid hotelId)
        {
            try
            {
                var users = await _userRepository.GetByHotelAsync(hotelId);
                var userDtos = new List<UserResponseDto>();

                foreach (var user in users)
                {
                    var dto = await MapToResponseDto(user);
                    if (dto != null)
                        userDtos.Add(dto);
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios del hotel: {HotelId}", hotelId);
                return new List<UserResponseDto>();
            }
        }

        public async Task<List<UserResponseDto>> GetActiveUsersAsync(Guid hotelId)
        {
            try
            {
                var users = await _userRepository.GetAllActiveAsync(hotelId);
                var userDtos = new List<UserResponseDto>();

                foreach (var user in users)
                {
                    var dto = await MapToResponseDto(user);
                    if (dto != null)
                        userDtos.Add(dto);
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos: {HotelId}", hotelId);
                return new List<UserResponseDto>();
            }
        }

        // ====================================================================
        // GESTIÓN DE ESTADO
        // ====================================================================

        public async Task<(bool Success, string? ErrorMessage)> ActivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Usuario activado: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar usuario: {UserId}", userId);
                return (false, "Error al activar el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeactivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Usuario desactivado: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario: {UserId}", userId);
                return (false, "Error al desactivar el usuario");
            }
        }

        // ====================================================================
        // GESTIÓN DE ROLES
        // ====================================================================

        public async Task<(bool Success, string? ErrorMessage)> AssignRoleAsync(Guid userId, Guid roleId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return (false, "Rol no encontrado");
                }

                // Validar que el rol pertenezca al mismo hotel
                if (user.HotelId != role.HotelId)
                {
                    return (false, "El rol no pertenece al mismo hotel del usuario");
                }

                user.RoleId = roleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Rol asignado al usuario: {UserId} -> {RoleId}", userId, roleId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar rol: {UserId}", userId);
                return (false, "Error al asignar el rol");
            }
        }

        // ====================================================================
        // GESTIÓN DE CONTRASEÑAS
        // ====================================================================

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                // Verificar la contraseña actual
                var isValidPassword = await _authService.VerifyPasswordAsync(
                    dto.CurrentPassword, 
                    user.PasswordHash);

                if (!isValidPassword)
                {
                    return (false, "La contraseña actual es incorrecta");
                }

                // Hashear la nueva contraseña
                var newPasswordHash = await _authService.HashPasswordAsync(dto.NewPassword);

                user.PasswordHash = newPasswordHash;
                user.MustChangePassword = false; // Ya cambió la contraseña
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Contraseña cambiada exitosamente: {UserId}", dto.UserId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña: {UserId}", dto.UserId);
                return (false, "Error al cambiar la contraseña");
            }
        }

        public async Task<(bool Success, string NewPassword, string? ErrorMessage)> ResetPasswordAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, string.Empty, "Usuario no encontrado");
                }

                // Generar una contraseña temporal
                var tempPassword = GenerateTemporaryPassword();

                // Hashear la contraseña temporal
                var passwordHash = await _authService.HashPasswordAsync(tempPassword);

                user.PasswordHash = passwordHash;
                user.MustChangePassword = true; // Debe cambiar la contraseña en el próximo login
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Contraseña restablecida: {UserId}", userId);
                return (true, tempPassword, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña: {UserId}", userId);
                return (false, string.Empty, "Error al restablecer la contraseña");
            }
        }

        // ====================================================================
        // PERMISOS
        // ====================================================================

        public async Task<List<string>> GetEffectivePermissionsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                // Obtener permisos del rol
                var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(user.RoleId);

                return permissions.Select(p => p.Code).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario: {UserId}", userId);
                return new List<string>();
            }
        }

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email, excludeUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar email: {Email}", email);
                return false;
            }
        }

        // ====================================================================
        // MÉTODOS PRIVADOS - MAPEO
        // ====================================================================

        private async Task<UserResponseDto?> MapToResponseDto(User user)
        {
            try
            {
                // Obtener información del rol
                var role = await _roleRepository.GetByIdAsync(user.RoleId);
                if (role == null)
                    return null;

                return new UserResponseDto
                {
                    // IDs
                    UserId = user.UserId,
                    HotelId = user.HotelId,
                    RoleId = user.RoleId,

                    // Autenticación
                    Username = user.Username,
                    Email = user.Email,
                    MustChangePassword = user.MustChangePassword,

                    // Información Personal
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    PhotoUrl = user.PhotoUrl,

                    // Información del Rol
                    RoleName = role.Name,
                    MaxDiscountPercent = role.MaxDiscountPercent,
                    IsManagerRole = role.IsManagerRole,

                    // Estado y Seguridad
                    IsActive = user.IsActive,
                    IsActiveText = user.IsActive ? "Activo" : "Inactivo",
                    LastLoginAt = user.LastLoginAt,
                    LastLoginText = user.LastLoginAt.HasValue 
                        ? user.LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm") 
                        : "Nunca",
                    FailedLoginAttempts = user.FailedLoginAttempts,
                    LockedUntil = user.LockedUntil,
                    IsLocked = user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow,

                    // Auditoría
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    CreatedBy = user.CreatedBy,
                    CreatedByName = "" // TODO: Obtener nombre del usuario que lo creó si es necesario
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mapear usuario a DTO: {UserId}", user.UserId);
                return null;
            }
        }

        private string GenerateTemporaryPassword()
        {
            // Generar una contraseña temporal de 8 caracteres
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}