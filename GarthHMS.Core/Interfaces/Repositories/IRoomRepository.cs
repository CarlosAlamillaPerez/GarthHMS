// GarthHMS.Core/Interfaces/Repositories/IRoomRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de habitaciones
    /// Basado en los 15 stored procedures de PostgreSQL
    /// </summary>
    public interface IRoomRepository
    {
        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        /// <summary>
        /// Obtiene una habitación por su ID
        /// SP: sp_room_get_by_id
        /// </summary>
        Task<Room?> GetByIdAsync(Guid roomId);

        /// <summary>
        /// Crea una nueva habitación
        /// SP: sp_room_create
        /// </summary>
        Task<Guid> CreateAsync(Room room);

        /// <summary>
        /// Actualiza una habitación existente
        /// SP: sp_room_update
        /// </summary>
        Task UpdateAsync(Room room);

        /// <summary>
        /// Elimina (soft delete) una habitación
        /// SP: sp_room_delete
        /// </summary>
        Task DeleteAsync(Guid roomId);

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        /// <summary>
        /// Obtiene todas las habitaciones de un hotel
        /// SP: sp_room_get_by_hotel
        /// </summary>
        Task<IEnumerable<Room>> GetByHotelAsync(Guid hotelId);

        /// <summary>
        /// Obtiene todas las habitaciones activas de un hotel
        /// SP: sp_room_get_all_active
        /// </summary>
        Task<IEnumerable<Room>> GetAllActiveAsync(Guid hotelId);

        /// <summary>
        /// Obtiene habitaciones por tipo
        /// SP: sp_room_get_by_type
        /// </summary>
        Task<IEnumerable<Room>> GetByTypeAsync(Guid roomTypeId);

        /// <summary>
        /// Obtiene habitaciones por estado
        /// SP: sp_room_get_by_status
        /// </summary>
        Task<IEnumerable<Room>> GetByStatusAsync(Guid hotelId, RoomStatus status);

        /// <summary>
        /// Obtiene una habitación por su número
        /// SP: sp_room_get_by_number
        /// </summary>
        Task<Room?> GetByNumberAsync(Guid hotelId, string roomNumber);

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        /// <summary>
        /// Verifica si existe una habitación con ese número
        /// SP: sp_room_number_exists
        /// </summary>
        Task<bool> RoomNumberExistsAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null);

        // ====================================================================
        // GESTIÓN DE ESTADOS
        // ====================================================================

        /// <summary>
        /// Actualiza el estado de una habitación
        /// SP: sp_room_update_status
        /// </summary>
        Task UpdateStatusAsync(Guid roomId, RoomStatus newStatus, Guid changedBy);

        /// <summary>
        /// Marca una habitación como en mantenimiento
        /// SP: sp_room_set_maintenance
        /// </summary>
        Task SetMaintenanceAsync(Guid roomId, string? notes, Guid changedBy);

        /// <summary>
        /// Marca una habitación como disponible
        /// SP: sp_room_set_available
        /// </summary>
        Task SetAvailableAsync(Guid roomId, Guid changedBy);

        // ====================================================================
        // CONSULTAS DE DISPONIBILIDAD
        // ====================================================================

        /// <summary>
        /// Obtiene habitaciones disponibles de un hotel
        /// SP: sp_room_get_available
        /// </summary>
        Task<IEnumerable<Room>> GetAvailableAsync(Guid hotelId);

        /// <summary>
        /// Obtiene habitaciones disponibles de un tipo específico
        /// SP: sp_room_get_available_by_type
        /// </summary>
        Task<IEnumerable<Room>> GetAvailableByTypeAsync(Guid roomTypeId);
    }
}