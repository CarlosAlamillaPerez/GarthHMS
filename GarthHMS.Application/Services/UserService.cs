using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio para gestión de usuarios
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IAuthService authService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        // CRUD

        public async Task<(bool Success, int UserId, string? ErrorMessage)> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phone,
            int? hotelId,
            UserRole role,
            decimal maxDiscountPercent,
            int createdBy)
        {
            try
            {
                // Validar que el email no exista
                var emailExists = await _userRepository.EmailExistsAsync(email);
                if (emailExists)
                {
                    return (false, 0, "El email ya está registrado");
                }

                // Hashear contraseña
                var passwordHash = await _authService.HashPasswordAsync(password);

                // Crear entidad
                var user = new User
                {
                    HotelId = hotelId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = passwordHash,
                    UserRole = role,
                    MaxDiscountPercent = maxDiscountPercent,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                var userId = await _userRepository.CreateAsync(user);

                if (userId > 0)
                {
                    _logger.LogInformation("User created successfully: {Email} by user {CreatedBy}", email, createdBy);
                    return (true, userId, null);
                }

                return (false, 0, "Error al crear el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", email);
                return (false, 0, "Error al crear el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
            int userId,
            string firstName,
            string lastName,
            string? phone,
            decimal maxDiscountPercent,
            int updatedBy)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                user.FirstName = firstName;
                user.LastName = lastName;
                user.Phone = phone;
                user.MaxDiscountPercent = maxDiscountPercent;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                var result = await _userRepository.UpdateAsync(user);

                if (result)
                {
                    _logger.LogInformation("User {UserId} updated successfully by user {UpdatedBy}", userId, updatedBy);
                    return (true, null);
                }

                return (false, "Error al actualizar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return (false, "Error al actualizar el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(int userId, int deletedBy)
        {
            try
            {
                // En lugar de eliminar físicamente, desactivamos el usuario
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = deletedBy;

                var result = await _userRepository.UpdateAsync(user);

                if (result)
                {
                    _logger.LogInformation("User {UserId} deleted (deactivated) by user {DeletedBy}", userId, deletedBy);
                    return (true, null);
                }

                return (false, "Error al eliminar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return (false, "Error al eliminar el usuario");
            }
        }

        // CONSULTAS

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user == null ? null : MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return null;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                return user == null ? null : MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email {Email}", email);
                return null;
            }
        }

        public async Task<List<UserDto>> GetUsersByHotelAsync(int hotelId)
        {
            try
            {
                var users = await _userRepository.GetByHotelIdAsync(hotelId);
                return users.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for hotel {HotelId}", hotelId);
                return new List<UserDto>();
            }
        }

        public async Task<List<UserDto>> GetActiveUsersAsync(int hotelId)
        {
            try
            {
                var users = await _userRepository.GetAllActiveAsync(hotelId);
                return users.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users for hotel {HotelId}", hotelId);
                return new List<UserDto>();
            }
        }

        // ESTADO

        public async Task<bool> ActivateUserAsync(int userId, int updatedBy)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                return await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId, int updatedBy)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                return await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return false;
            }
        }

        // VALIDACIONES

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email, excludeUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email exists {Email}", email);
                return false;
            }
        }

        // HELPERS

        private static UserDto MapToDto(User user)
        {
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
    }
}
