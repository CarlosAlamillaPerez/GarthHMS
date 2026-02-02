using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Role;
using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid roleId);
        Task<IEnumerable<Role>> GetByHotelAsync(Guid hotelId);
        Task<IEnumerable<Role>> GetAllActiveAsync(Guid hotelId);
        Task<Guid> CreateAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(Guid roleId);
        Task<bool> NameExistsAsync(Guid hotelId, string name, Guid? excludeRoleId = null);

        // Gestión de permisos
        Task<IEnumerable<Guid>> GetRolePermissionsAsync(Guid roleId);
        Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(); // ← NUEVO
    }
}