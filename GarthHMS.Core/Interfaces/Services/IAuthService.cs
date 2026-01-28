using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de autenticación
    /// </summary>
    public interface IAuthService
    {
        // LOGIN/LOGOUT
        Task<(bool Success, UserDto? User, string? ErrorMessage)> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();

        // VALIDACIONES
        Task<bool> ValidatePasswordAsync(string email, string password);
        Task<UserDto?> GetCurrentUserAsync(int userId);

        // PASSWORD
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hash);
    }
}
