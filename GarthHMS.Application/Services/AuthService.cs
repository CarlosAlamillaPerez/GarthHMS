using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio para autenticación y gestión de sesiones
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // LOGIN/LOGOUT

        public async Task<(bool Success, UserDto? User, string? ErrorMessage)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", loginDto.Email);
                    return (false, null, "Email o contraseña incorrectos");
                }

                // Verificar que el usuario esté activo
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User {Email} is inactive", loginDto.Email);
                    return (false, null, "Usuario inactivo. Contacte al administrador");
                }

                // Verificar contraseña
                var isPasswordValid = await VerifyPasswordAsync(loginDto.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Email}", loginDto.Email);
                    return (false, null, "Email o contraseña incorrectos");
                }

                // Actualizar último login
                await _userRepository.UpdateLastLoginAsync(user.UserId);

                // Crear DTO para respuesta
                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    HotelId = user.HotelId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    UserRole = user.UserRole,
                    UserRoleText = user.UserRole.ToString(),
                    IsActive = user.IsActive,
                    MaxDiscountPercent = user.MaxDiscountPercent
                };

                _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
                return (true, userDto, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", loginDto.Email);
                return (false, null, "Error al iniciar sesión. Intente nuevamente");
            }
        }

        public Task LogoutAsync()
        {
            // En este caso, el logout se maneja en el controller limpiando las cookies
            // Este método existe por si necesitamos lógica adicional (como registrar auditoría)
            return Task.CompletedTask;
        }

        // VALIDACIONES

        public async Task<bool> ValidatePasswordAsync(string email, string password)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return false;

                return await VerifyPasswordAsync(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for email {Email}", email);
                return false;
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    return null;

                return new UserDto
                {
                    UserId = user.UserId,
                    HotelId = user.HotelId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    UserRole = user.UserRole,
                    UserRoleText = user.UserRole.ToString(),
                    IsActive = user.IsActive,
                    MaxDiscountPercent = user.MaxDiscountPercent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user {UserId}", userId);
                return null;
            }
        }

        // PASSWORD

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Change password failed: User {UserId} not found", userId);
                    return false;
                }

                // Verificar contraseña actual
                var isCurrentPasswordValid = await VerifyPasswordAsync(currentPassword, user.PasswordHash);

                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning("Change password failed: Invalid current password for user {UserId}", userId);
                    return false;
                }

                // Hashear nueva contraseña
                var newPasswordHash = await HashPasswordAsync(newPassword);

                // Actualizar contraseña
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = userId;

                var result = await _userRepository.UpdateAsync(user);

                if (result)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        public Task<string> HashPasswordAsync(string password)
        {
            // Usar BCrypt para hashear contraseñas
            var hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            return Task.FromResult(hash);
        }

        public Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            try
            {
                var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
                return Task.FromResult(isValid);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
