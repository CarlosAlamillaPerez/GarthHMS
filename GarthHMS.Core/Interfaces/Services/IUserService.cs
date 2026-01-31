using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        // CRUD
        Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phone,
            Guid hotelId,
            Guid roleId,
            Guid createdBy
        );

        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
            Guid userId,
            string firstName,
            string lastName,
            string? phone
        );

        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId);

        // CONSULTAS
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetUsersByHotelAsync(Guid hotelId);
        Task<List<UserDto>> GetActiveUsersAsync(Guid hotelId);

        // ESTADO
        Task<bool> ActivateUserAsync(Guid userId);
        Task<bool> DeactivateUserAsync(Guid userId);

        // VALIDACIONES
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    }
}