using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DapperContext _context;

        public RoleRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid roleId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_role_id", roleId);

            var result = await connection.QueryFirstOrDefaultAsync<Role>(
                "sp_role_get_by_id",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<IEnumerable<Role>> GetByHotelAsync(Guid hotelId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_hotel_id", hotelId);

            var result = await connection.QueryAsync<Role>(
                "sp_role_get_by_hotel",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<IEnumerable<Role>> GetAllActiveAsync(Guid hotelId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_hotel_id", hotelId);

            var result = await connection.QueryAsync<Role>(
                "sp_role_get_all_active",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<Guid> CreateAsync(Role role)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_hotel_id", role.HotelId);
            parameters.Add("p_name", role.Name);
            parameters.Add("p_description", role.Description);
            parameters.Add("p_max_discount_percent", role.MaxDiscountPercent);
            parameters.Add("p_is_system_role", role.IsSystemRole);
            parameters.Add("p_is_manager_role", role.IsManagerRole);
            parameters.Add("p_created_by", role.CreatedBy);

            var roleId = await connection.ExecuteScalarAsync<Guid>(
                "sp_role_create",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return roleId;
        }

        public async Task UpdateAsync(Role role)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_role_id", role.RoleId);
            parameters.Add("p_name", role.Name);
            parameters.Add("p_description", role.Description);
            parameters.Add("p_max_discount_percent", role.MaxDiscountPercent);

            await connection.ExecuteAsync(
                "sp_role_update",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteAsync(Guid roleId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_role_id", roleId);

            await connection.ExecuteAsync(
                "sp_role_delete",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> NameExistsAsync(Guid hotelId, string name, Guid? excludeRoleId = null)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_hotel_id", hotelId);
            parameters.Add("p_name", name);
            parameters.Add("p_exclude_role_id", excludeRoleId);

            var exists = await connection.ExecuteScalarAsync<bool>(
                "sp_role_name_exists",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return exists;
        }

        // ============================================================================
        // GESTIÓN DE PERMISOS
        // ============================================================================

        public async Task<IEnumerable<Guid>> GetRolePermissionsAsync(Guid roleId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_role_id", roleId);

            var result = await connection.QueryAsync<Guid>(
                @"SELECT permission_id 
                  FROM role_permission 
                  WHERE role_id = @p_role_id",
                parameters
            );

            return result;
        }

        public async Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            using var connection = _context.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Eliminar permisos actuales
                await connection.ExecuteAsync(
                    "DELETE FROM role_permission WHERE role_id = @RoleId",
                    new { RoleId = roleId },
                    transaction
                );

                // 2. Insertar nuevos permisos
                if (permissionIds.Any())
                {
                    var rolePermissions = permissionIds.Select(permId => new
                    {
                        RolePermissionId = Guid.NewGuid(),
                        RoleId = roleId,
                        PermissionId = permId,
                        CreatedAt = DateTime.UtcNow
                    });

                    await connection.ExecuteAsync(
                        @"INSERT INTO role_permission (role_permission_id, role_id, permission_id, created_at) 
                          VALUES (@RolePermissionId, @RoleId, @PermissionId, @CreatedAt)",
                        rolePermissions,
                        transaction
                    );
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}