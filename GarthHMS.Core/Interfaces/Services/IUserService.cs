using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        // CRUD
        Task<(bool Success, int UserId, string? ErrorMessage)> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phone,
            int? hotelId,
            UserRole role,
            decimal maxDiscountPercent,
            int createdBy
        );

        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
            int userId,
            string firstName,
            string lastName,
            string? phone,
            decimal maxDiscountPercent,
            int updatedBy
        );

        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(int userId, int deletedBy);

        // CONSULTAS
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetUsersByHotelAsync(int hotelId);
        Task<List<UserDto>> GetActiveUsersAsync(int hotelId);

        // ESTADO
        Task<bool> ActivateUserAsync(int userId, int updatedBy);
        Task<bool> DeactivateUserAsync(int userId, int updatedBy);

        // VALIDACIONES
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    }
}
