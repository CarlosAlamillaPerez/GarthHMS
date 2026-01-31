using System;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
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

        public async Task<(bool Success, UserDto? User, string? ErrorMessage)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found {Email}", loginDto.Email);
                    return (false, null, "Email o contraseña incorrectos");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: Inactive user {Email}", loginDto.Email);
                    return (false, null, "Usuario inactivo. Contacte al administrador");
                }

                var isPasswordValid = await VerifyPasswordAsync(loginDto.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password {Email}", loginDto.Email);
                    return (false, null, "Email o contraseña incorrectos");
                }

                await _userRepository.UpdateLastLoginAsync(user.UserId);

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    HotelId = user.HotelId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    AvatarUrl = user.PhotoUrl,
                    UserRole = Core.Enums.UserRole.SuperAdmin,
                    UserRoleText = "SuperAdmin",
                    IsActive = user.IsActive,
                    MaxDiscountPercent = 100
                };

                _logger.LogInformation("User logged in: {Email}", loginDto.Email);
                return (true, userDto, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Email}", loginDto.Email);
                return (false, null, "Error al iniciar sesión. Intente nuevamente");
            }
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

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
                _logger.LogError(ex, "Error validating password: {Email}", email);
                return false;
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
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
                    AvatarUrl = user.PhotoUrl,
                    UserRole = Core.Enums.UserRole.SuperAdmin,
                    UserRoleText = "SuperAdmin",
                    IsActive = user.IsActive,
                    MaxDiscountPercent = 100
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Change password failed: User not found {UserId}", userId);
                    return false;
                }

                var isCurrentPasswordValid = await VerifyPasswordAsync(currentPassword, user.PasswordHash);

                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning("Change password failed: Invalid password {UserId}", userId);
                    return false;
                }

                var newPasswordHash = await HashPasswordAsync(newPassword);

                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Password changed: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password: {UserId}", userId);
                return false;
            }
        }

        public Task<string> HashPasswordAsync(string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            return Task.FromResult(hash);
        }

        public Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(hash))
                {
                    return Task.FromResult(false);
                }

                var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return Task.FromResult(false);
            }
        }
    }
}