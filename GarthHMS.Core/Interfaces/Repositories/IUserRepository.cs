using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de usuarios
    /// </summary>
    public interface IUserRepository
    {
        // AUTENTICACIÓN
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> ValidateCredentialsAsync(string email, string passwordHash);

        // CRUD
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);

        // CONSULTAS
        Task<List<User>> GetByHotelIdAsync(int hotelId);
        Task<List<User>> GetAllActiveAsync(int hotelId);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);

        // SESIÓN
        Task UpdateLastLoginAsync(int userId);
    }
}
