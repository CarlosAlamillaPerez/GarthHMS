using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Role;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    public interface IRoleService
    {
        Task<ServiceResult<RoleResponseDto>> GetByIdAsync(Guid roleId);
        Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetByHotelAsync(Guid hotelId);
        Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllActiveAsync(Guid hotelId);
        Task<ServiceResult<Guid>> CreateAsync(Guid hotelId, CreateRoleDto dto);
        Task<ServiceResult<bool>> UpdateAsync(UpdateRoleDto dto);
        Task<ServiceResult<bool>> DeleteAsync(Guid roleId);

        // Gestión de permisos
        Task<ServiceResult<IEnumerable<PermissionDto>>> GetRolePermissionsAsync(Guid roleId);
        Task<ServiceResult<IEnumerable<PermissionDto>>> GetAllPermissionsAsync();
        Task<ServiceResult<bool>> AssignPermissionsAsync(RolePermissionDto dto);
    }
}