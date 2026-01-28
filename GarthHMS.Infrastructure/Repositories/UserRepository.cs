using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de usuarios
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly Procedimientos _procedimientos;

        public UserRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // AUTENTICACIÓN

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _procedimientos.EjecutarUnicoAsync<User>(
                "sp_user_get_by_email",
                new { p_email = email }
            );
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _procedimientos.EjecutarUnicoAsync<User>(
                "sp_user_get_by_id",
                new { p_user_id = userId }
            );
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string passwordHash)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_user_validate_credentials",
                new
                {
                    p_email = email,
                    p_password_hash = passwordHash
                }
            );

            return result;
        }

        // CRUD

        public async Task<int> CreateAsync(User user)
        {
            var userId = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_user_create",
                new
                {
                    p_hotel_id = user.HotelId,
                    p_first_name = user.FirstName,
                    p_last_name = user.LastName,
                    p_email = user.Email,
                    p_phone = user.Phone,
                    p_password_hash = user.PasswordHash,
                    p_user_role = user.UserRole,
                    p_custom_role_id = user.CustomRoleId,
                    p_max_discount_percent = user.MaxDiscountPercent,
                    p_avatar_url = user.AvatarUrl,
                    p_is_active = user.IsActive,
                    p_created_by = user.CreatedBy
                }
            );

            return userId;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_user_update",
                new
                {
                    p_user_id = user.UserId,
                    p_first_name = user.FirstName,
                    p_last_name = user.LastName,
                    p_email = user.Email,
                    p_phone = user.Phone,
                    p_max_discount_percent = user.MaxDiscountPercent,
                    p_avatar_url = user.AvatarUrl,
                    p_is_active = user.IsActive,
                    p_updated_by = user.UpdatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_user_delete",
                new { p_user_id = userId }
            );

            return rowsAffected > 0;
        }

        // CONSULTAS

        public async Task<List<User>> GetByHotelIdAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<User>(
                "sp_user_get_by_hotel",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<List<User>> GetAllActiveAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<User>(
                "sp_user_get_all_active",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_user_email_exists",
                new
                {
                    p_email = email,
                    p_exclude_user_id = excludeUserId
                }
            );

            return result;
        }

        // SESIÓN

        public async Task UpdateLastLoginAsync(int userId)
        {
            await _procedimientos.EjecutarAsync(
                "sp_user_update_last_login",
                new { p_user_id = userId }
            );
        }
    }
}