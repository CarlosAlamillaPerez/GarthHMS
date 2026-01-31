using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Procedimientos _procedimientos;

        public UserRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _procedimientos.EjecutarUnicoAsync<User>("sp_user_get_by_id", new
            {
                p_user_id = id
            });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _procedimientos.EjecutarUnicoAsync<User>("sp_user_get_by_email", new
            {
                p_email = email
            });
        }

        public async Task<List<User>> GetByHotelAsync(Guid hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<User>("sp_user_get_by_hotel", new
            {
                p_hotel_id = hotelId
            });
        }

        public async Task<List<User>> GetAllActiveAsync(Guid hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<User>("sp_user_get_all_active", new
            {
                p_hotel_id = hotelId
            });
        }

        public async Task<Guid> CreateAsync(User user)
        {
            var userId = await _procedimientos.EjecutarEscalarAsync<Guid>("sp_user_create", new
            {
                p_hotel_id = user.HotelId,
                p_role_id = user.RoleId,
                p_username = user.Username,
                p_email = user.Email,
                p_password_hash = user.PasswordHash,
                p_must_change_password = user.MustChangePassword,
                p_first_name = user.FirstName,
                p_last_name = user.LastName,
                p_phone = user.Phone,
                p_photo_url = user.PhotoUrl,
                p_created_by = user.CreatedBy
            });

            return userId;
        }

        public async Task UpdateAsync(User user)
        {
            await _procedimientos.EjecutarAsync("sp_user_update", new
            {
                p_user_id = user.UserId,
                p_username = user.Username,
                p_email = user.Email,
                p_first_name = user.FirstName,
                p_last_name = user.LastName,
                p_phone = user.Phone,
                p_photo_url = user.PhotoUrl,
                p_is_active = user.IsActive
            });
        }

        public async Task DeleteAsync(Guid id)
        {
            await _procedimientos.EjecutarAsync("sp_user_delete", new
            {
                p_user_id = id
            });
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            return await _procedimientos.EjecutarEscalarAsync<bool>("sp_user_email_exists", new
            {
                p_email = email,
                p_exclude_user_id = excludeUserId
            });
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            await _procedimientos.EjecutarAsync("sp_user_update_last_login", new
            {
                p_user_id = userId
            });
        }
    }
}