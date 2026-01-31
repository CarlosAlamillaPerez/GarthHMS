using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
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

        public async Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phone,
            Guid hotelId,
            Guid roleId,
            Guid createdBy)
        {
            try
            {
                var emailExists = await _userRepository.EmailExistsAsync(email);
                if (emailExists)
                {
                    return (false, Guid.Empty, "El email ya está registrado");
                }

                var passwordHash = await _authService.HashPasswordAsync(password);

                var user = new User
                {
                    HotelId = hotelId,
                    RoleId = roleId,
                    Username = email.Split('@')[0],
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = passwordHash,
                    MustChangePassword = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                var userId = await _userRepository.CreateAsync(user);

                if (userId != Guid.Empty)
                {
                    _logger.LogInformation("User created: {Email}", email);
                    return (true, userId, null);
                }

                return (false, Guid.Empty, "Error al crear el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", email);
                return (false, Guid.Empty, "Error al crear el usuario");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
            Guid userId,
            string firstName,
            string lastName,
            string? phone)
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
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("User updated: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
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

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("User deleted: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return (false, "Error al eliminar el usuario");
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user == null ? null : MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user: {UserId}", userId);
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
                _logger.LogError(ex, "Error getting user: {Email}", email);
                return null;
            }
        }

        public async Task<List<UserDto>> GetUsersByHotelAsync(Guid hotelId)
        {
            try
            {
                var users = await _userRepository.GetByHotelAsync(hotelId);
                return users.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for hotel: {HotelId}", hotelId);
                return new List<UserDto>();
            }
        }

        public async Task<List<UserDto>> GetActiveUsersAsync(Guid hotelId)
        {
            try
            {
                var users = await _userRepository.GetAllActiveAsync(hotelId);
                return users.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users: {HotelId}", hotelId);
                return new List<UserDto>();
            }
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("User activated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("User deactivated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email, excludeUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email: {Email}", email);
                return false;
            }
        }

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
                AvatarUrl = user.PhotoUrl,
                UserRole = UserRole.SuperAdmin,
                UserRoleText = "SuperAdmin",
                IsActive = user.IsActive,
                MaxDiscountPercent = 100
            };
        }
    }
}