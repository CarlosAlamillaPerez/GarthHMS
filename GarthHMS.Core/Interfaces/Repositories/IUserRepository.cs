using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetByHotelAsync(Guid hotelId);
        Task<List<User>> GetAllActiveAsync(Guid hotelId);
        Task<Guid> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
        Task UpdateLastLoginAsync(Guid userId);
    }
}
