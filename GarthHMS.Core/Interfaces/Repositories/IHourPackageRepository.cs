// GarthHMS.Core/Interfaces/Repositories/IHourPackageRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de paquetes de horas
    /// Basado en los 8 stored procedures de PostgreSQL
    /// </summary>
    public interface IHourPackageRepository
    {
        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        /// <summary>
        /// Obtiene un paquete de horas por su ID
        /// SP: sp_hourpackage_get_by_id
        /// </summary>
        Task<HourPackage?> GetByIdAsync(Guid hourPackageId);

        /// <summary>
        /// Crea un nuevo paquete de horas
        /// SP: sp_hourpackage_create
        /// </summary>
        Task<Guid> CreateAsync(HourPackage hourPackage);

        /// <summary>
        /// Actualiza un paquete de horas existente
        /// SP: sp_hourpackage_update
        /// </summary>
        Task UpdateAsync(HourPackage hourPackage);

        /// <summary>
        /// Elimina (soft delete) un paquete de horas
        /// SP: sp_hourpackage_delete
        /// </summary>
        Task DeleteAsync(Guid hourPackageId);

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        /// <summary>
        /// Obtiene todos los paquetes de horas de un hotel
        /// SP: sp_hourpackage_get_by_hotel
        /// </summary>
        Task<IEnumerable<HourPackage>> GetByHotelAsync(Guid hotelId);

        /// <summary>
        /// Obtiene todos los paquetes activos de un hotel
        /// SP: sp_hourpackage_get_all_active
        /// </summary>
        Task<IEnumerable<HourPackage>> GetAllActiveAsync(Guid hotelId);

        /// <summary>
        /// Obtiene paquetes por tipo de habitación
        /// SP: sp_hourpackage_get_by_room_type
        /// </summary>
        Task<IEnumerable<HourPackage>> GetByRoomTypeAsync(Guid roomTypeId);

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        /// <summary>
        /// Verifica si existe un paquete con las mismas características
        /// SP: sp_hourpackage_exists
        /// </summary>
        Task<bool> ExistsAsync(Guid hotelId, Guid roomTypeId, int hours, Guid? excludeHourPackageId = null);
    }
}