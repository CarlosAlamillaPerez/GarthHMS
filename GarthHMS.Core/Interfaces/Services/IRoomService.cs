// GarthHMS.Core/Interfaces/Services/IRoomService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Room;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de habitaciones con lógica de negocio
    /// </summary>
    public interface IRoomService
    {
        // ====================================================================
        // CRUD
        // ====================================================================

        /// <summary>
        /// Obtiene todas las habitaciones del hotel actual
        /// </summary>
        Task<IEnumerable<RoomResponseDto>> GetAllAsync();

        /// <summary>
        /// Obtiene todas las habitaciones activas del hotel actual
        /// </summary>
        Task<IEnumerable<RoomResponseDto>> GetAllActiveAsync();

        /// <summary>
        /// Obtiene una habitación por su ID
        /// </summary>
        Task<RoomResponseDto?> GetByIdAsync(Guid roomId);

        /// <summary>
        /// Crea una nueva habitación
        /// Validaciones:
        /// - Número de habitación único por hotel
        /// - Tipo de habitación debe existir y pertenecer al mismo hotel
        /// - Piso válido (>= 0)
        /// </summary>
        Task<Guid> CreateAsync(CreateRoomDto dto);

        /// <summary>
        /// Actualiza una habitación existente
        /// Validaciones:
        /// - Habitación debe existir
        /// - Número de habitación único (excluyendo la actual)
        /// - Multi-tenancy
        /// </summary>
        Task UpdateAsync(UpdateRoomDto dto);

        /// <summary>
        /// Elimina (soft delete) una habitación
        /// Validaciones:
        /// - No se puede eliminar si está ocupada (current_stay_id != null)
        /// - No se puede eliminar si tiene reservas futuras
        /// </summary>
        Task DeleteAsync(Guid roomId);

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        /// <summary>
        /// Obtiene habitaciones por tipo
        /// </summary>
        Task<IEnumerable<RoomResponseDto>> GetByTypeAsync(Guid roomTypeId);

        /// <summary>
        /// Obtiene habitaciones por estado
        /// </summary>
        Task<IEnumerable<RoomResponseDto>> GetByStatusAsync(RoomStatus status);

        /// <summary>
        /// Obtiene habitaciones disponibles
        /// </summary>
        Task<IEnumerable<RoomResponseDto>> GetAvailableAsync();

        // ====================================================================
        // GESTIÓN DE ESTADOS
        // ====================================================================

        /// <summary>
        /// Cambia el estado de una habitación
        /// Validaciones:
        /// - Habitación debe existir
        /// - Transiciones de estado válidas
        /// </summary>
        Task UpdateStatusAsync(Guid roomId, RoomStatus newStatus);

        /// <summary>
        /// Marca habitación como en mantenimiento
        /// </summary>
        Task SetMaintenanceAsync(Guid roomId, string? notes);

        /// <summary>
        /// Marca habitación como disponible
        /// Limpia current_stay_id
        /// </summary>
        Task SetAvailableAsync(Guid roomId);
    }
}