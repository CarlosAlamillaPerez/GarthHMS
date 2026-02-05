// GarthHMS.Core/Interfaces/Services/IHourPackageService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.HourPackage;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para la lógica de negocio de paquetes de horas
    /// </summary>
    public interface IHourPackageService
    {
        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        /// <summary>
        /// Obtiene un paquete de horas por su ID
        /// </summary>
        Task<ServiceResult<HourPackageResponseDto>> GetByIdAsync(Guid hourPackageId);

        /// <summary>
        /// Crea un nuevo paquete de horas
        /// Validaciones:
        /// - No duplicar (hotel + room_type + hours)
        /// - RoomType debe existir y ser del mismo hotel
        /// - Hours debe ser mayor a 0
        /// - Price debe ser mayor a 0
        /// </summary>
        Task<ServiceResult<Guid>> CreateAsync(CreateHourPackageDto dto, Guid hotelId, Guid userId);

        /// <summary>
        /// Actualiza un paquete de horas existente
        /// Validaciones:
        /// - Paquete debe existir
        /// - No duplicar (hotel + room_type + hours)
        /// - Hours debe ser mayor a 0
        /// - Price debe ser mayor a 0
        /// </summary>
        Task<ServiceResult<bool>> UpdateAsync(UpdateHourPackageDto dto, Guid hotelId);

        /// <summary>
        /// Elimina (soft delete) un paquete de horas
        /// Validaciones:
        /// - Paquete debe existir
        /// - No debe tener reservaciones activas (futuro)
        /// </summary>
        Task<ServiceResult<bool>> DeleteAsync(Guid hourPackageId, Guid hotelId);

        // ====================================================================
        // CONSULTAS
        // ====================================================================

        /// <summary>
        /// Lista todos los paquetes de horas del hotel con información del room type
        /// </summary>
        Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetAllByHotelAsync(Guid hotelId);

        /// <summary>
        /// Lista solo paquetes activos del hotel
        /// </summary>
        Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetActiveByHotelAsync(Guid hotelId);

        /// <summary>
        /// Obtiene paquetes por tipo de habitación
        /// </summary>
        Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetByRoomTypeAsync(Guid roomTypeId);

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        /// <summary>
        /// Valida si existe un paquete duplicado
        /// </summary>
        Task<bool> PackageExistsAsync(Guid hotelId, Guid roomTypeId, int hours, Guid? excludeHourPackageId = null);
    }
}