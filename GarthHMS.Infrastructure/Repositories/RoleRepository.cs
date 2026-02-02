using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GarthHMS.Core.DTOs.Role;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly Procedimientos _procedimientos;
        private readonly BaseDeDatos _baseDeDatos;

        public RoleRepository(Procedimientos procedimientos, BaseDeDatos baseDeDatos)
        {
            _procedimientos = procedimientos;
            _baseDeDatos = baseDeDatos;
        }

        public async Task<Role?> GetByIdAsync(Guid roleId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_role_get_by_id",
                new { p_role_id = roleId }
            );

            return result != null ? MapToRole(result) : null;
        }

        public async Task<IEnumerable<Role>> GetByHotelAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_role_get_by_hotel",
                new { p_hotel_id = hotelId }
            );

            return results.Select(MapToRole);
        }

        public async Task<IEnumerable<Role>> GetAllActiveAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_role_get_all_active",
                new { p_hotel_id = hotelId }
            );

            return results.Select(MapToRole);
        }

        public async Task<Guid> CreateAsync(Role role)
        {
            var roleId = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "sp_role_create",
                new
                {
                    p_hotel_id = role.HotelId,
                    p_name = role.Name,
                    p_description = role.Description,
                    p_max_discount_percent = role.MaxDiscountPercent,
                    p_is_system_role = role.IsSystemRole,
                    p_is_manager_role = role.IsManagerRole,
                    p_created_by = role.CreatedBy
                }
            );

            return roleId;
        }

        public async Task UpdateAsync(Role role)
        {
            await _procedimientos.EjecutarAsync(
                "sp_role_update",
                new
                {
                    p_role_id = role.RoleId,
                    p_name = role.Name,
                    p_description = role.Description,
                    p_max_discount_percent = role.MaxDiscountPercent
                }
            );
        }

        public async Task DeleteAsync(Guid roleId)
        {
            await _procedimientos.EjecutarAsync(
                "sp_role_delete",
                new { p_role_id = roleId }
            );
        }

        public async Task<bool> NameExistsAsync(Guid hotelId, string name, Guid? excludeRoleId = null)
        {
            var exists = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_role_name_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_name = name,
                    p_exclude_role_id = excludeRoleId
                }
            );

            return exists;
        }

        // ============================================================================
        // GESTIÓN DE PERMISOS
        // ============================================================================

        public async Task<IEnumerable<Guid>> GetRolePermissionsAsync(Guid roleId)
        {
            using var connection = await _baseDeDatos.GetConnectionAsync();

            var result = await connection.QueryAsync<Guid>(
                @"SELECT permission_id 
                  FROM role_permission 
                  WHERE role_id = @p_role_id",
                new { p_role_id = roleId }
            );

            return result;
        }

        public async Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            using var connection = await _baseDeDatos.GetConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

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

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            using var connection = await _baseDeDatos.GetConnectionAsync();

            var permissions = await connection.QueryAsync<PermissionDto>(
                @"SELECT 
                    permission_id AS PermissionId,
                    code AS Code,
                    module AS Module,
                    name AS Name,
                    description AS Description,
                    category AS Category,
                    display_order AS DisplayOrder
                  FROM permission
                  ORDER BY category, display_order"
            );

            return permissions;
        }

        // ============================================================================
        // MAPPERS
        // ============================================================================

        private Role MapToRole(dynamic result)
        {
            return new Role
            {
                RoleId = Guid.Parse(result.role_id.ToString()),
                HotelId = Guid.Parse(result.hotel_id.ToString()),
                Name = result.name,
                Description = result.description,
                MaxDiscountPercent = result.max_discount_percent,
                IsSystemRole = result.is_system_role,
                IsManagerRole = result.is_manager_role,
                IsActive = result.is_active,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at,
                CreatedBy = result.created_by != null ? Guid.Parse(result.created_by.ToString()) : null
            };
        }
    }
}