using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Role;
using GarthHMS.Core.DTOs.Room;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;

namespace GarthHMS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ServiceResult<RoleResponseDto>> GetByIdAsync(Guid roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return ServiceResult<RoleResponseDto>.Failure("Rol no encontrado");
                }

                var dto = MapToResponseDto(role);
                return ServiceResult<RoleResponseDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<RoleResponseDto>.Failure($"Error al obtener el rol: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetByHotelAsync(Guid hotelId)
        {
            try
            {
                var roles = await _roleRepository.GetByHotelAsync(hotelId);
                var dtos = roles.Select(MapToResponseDto);
                return ServiceResult<IEnumerable<RoleResponseDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<RoleResponseDto>>.Failure($"Error al obtener roles: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllActiveAsync(Guid hotelId)
        {
            try
            {
                var roles = await _roleRepository.GetAllActiveAsync(hotelId);
                var dtos = roles.Select(MapToResponseDto);
                return ServiceResult<IEnumerable<RoleResponseDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<RoleResponseDto>>.Failure($"Error al obtener roles activos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Guid>> CreateAsync(Guid hotelId, CreateRoleDto dto)
        {
            try
            {
                // Validar que el nombre no exista
                var nameExists = await _roleRepository.NameExistsAsync(hotelId, dto.Name);
                if (nameExists)
                {
                    return ServiceResult<Guid>.Failure("Ya existe un rol con ese nombre");
                }

                // Crear entidad
                var role = new Role
                {
                    RoleId = Guid.NewGuid(),
                    HotelId = hotelId,
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    MaxDiscountPercent = dto.MaxDiscountPercent,
                    IsSystemRole = dto.IsSystemRole,
                    IsManagerRole = dto.IsManagerRole,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                var roleId = await _roleRepository.CreateAsync(role);
                return ServiceResult<Guid>.Success(roleId, "Rol creado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult<Guid>.Failure($"Error al crear el rol: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateAsync(UpdateRoleDto dto)
        {
            try
            {
                // Verificar que el rol existe
                var existingRole = await _roleRepository.GetByIdAsync(dto.RoleId);
                if (existingRole == null)
                {
                    return ServiceResult<bool>.Failure("Rol no encontrado");
                }

                // Validar que no sea un rol del sistema (no se puede editar)
                if (existingRole.IsSystemRole)
                {
                    return ServiceResult<bool>.Failure("No se pueden editar roles del sistema");
                }

                // Validar que el nombre no exista en otro rol
                var nameExists = await _roleRepository.NameExistsAsync(existingRole.HotelId, dto.Name, dto.RoleId);
                if (nameExists)
                {
                    return ServiceResult<bool>.Failure("Ya existe otro rol con ese nombre");
                }

                // Actualizar entidad
                existingRole.Name = dto.Name.Trim();
                existingRole.Description = dto.Description?.Trim();
                existingRole.MaxDiscountPercent = dto.MaxDiscountPercent;
                existingRole.UpdatedAt = DateTime.UtcNow;

                await _roleRepository.UpdateAsync(existingRole);
                return ServiceResult<bool>.Success(true, "Rol actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al actualizar el rol: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid roleId)
        {
            try
            {
                // Verificar que el rol existe
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return ServiceResult<bool>.Failure("Rol no encontrado");
                }

                // Validar que no sea un rol del sistema
                if (role.IsSystemRole)
                {
                    return ServiceResult<bool>.Failure("No se pueden eliminar roles del sistema");
                }

                // TODO: Validar que no haya usuarios asignados a este rol
                // (implementar cuando tengamos el módulo de usuarios)

                await _roleRepository.DeleteAsync(roleId);
                return ServiceResult<bool>.Success(true, "Rol eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al eliminar el rol: {ex.Message}");
            }
        }

        // ============================================================================
        // GESTIÓN DE PERMISOS
        // ============================================================================

        public async Task<ServiceResult<IEnumerable<PermissionDto>>> GetRolePermissionsAsync(Guid roleId)
        {
            try
            {
                // Obtener todos los permisos del sistema
                var allPermissions = await GetAllPermissionsAsync();
                if (!allPermissions.IsSuccess)
                {
                    return ServiceResult<IEnumerable<PermissionDto>>.Failure(allPermissions.Message);
                }

                // Obtener los permisos asignados a este rol
                var assignedPermissionIds = await _roleRepository.GetRolePermissionsAsync(roleId);
                var assignedSet = new HashSet<Guid>(assignedPermissionIds);

                // Marcar cuáles están asignados
                var permissions = allPermissions.Data.Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    Code = p.Code,
                    Module = p.Module,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    DisplayOrder = p.DisplayOrder,
                    IsAssigned = assignedSet.Contains(p.PermissionId)
                }).ToList();

                return ServiceResult<IEnumerable<PermissionDto>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<PermissionDto>>.Failure($"Error al obtener permisos del rol: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<PermissionDto>>> GetAllPermissionsAsync()
        {
            try
            {
                var permissions = await _roleRepository.GetAllPermissionsAsync();
                return ServiceResult<IEnumerable<PermissionDto>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<PermissionDto>>.Failure($"Error al obtener permisos: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> AssignPermissionsAsync(RolePermissionDto dto)
        {
            try
            {
                // Verificar que el rol existe
                var role = await _roleRepository.GetByIdAsync(dto.RoleId);
                if (role == null)
                {
                    return ServiceResult<bool>.Failure("Rol no encontrado");
                }

                // Validar que no sea un rol del sistema
                if (role.IsSystemRole)
                {
                    return ServiceResult<bool>.Failure("No se pueden modificar los permisos de roles del sistema");
                }

                await _roleRepository.AssignPermissionsAsync(dto.RoleId, dto.PermissionIds);
                return ServiceResult<bool>.Success(true, "Permisos asignados exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error al asignar permisos: {ex.Message}");
            }
        }

        // ============================================================================
        // MAPPERS
        // ============================================================================

        private RoleResponseDto MapToResponseDto(Role role)
        {
            return new RoleResponseDto
            {
                RoleId = role.RoleId,
                HotelId = role.HotelId,
                Name = role.Name,
                Description = role.Description,
                MaxDiscountPercent = role.MaxDiscountPercent,
                IsSystemRole = role.IsSystemRole,
                IsManagerRole = role.IsManagerRole,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}