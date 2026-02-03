using System;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.HotelSettings;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    public interface IHotelSettingsService
    {
        /// <summary>
        /// Obtiene la configuración del hotel
        /// </summary>
        Task<ServiceResult<HotelSettingsDto>> GetSettingsAsync(Guid hotelId);

        /// <summary>
        /// Actualiza la configuración del hotel
        /// </summary>
        Task<ServiceResult<bool>> UpdateSettingsAsync(Guid hotelId, UpdateHotelSettingsDto dto, Guid userId);

        /// <summary>
        /// Verifica si existe configuración para el hotel
        /// </summary>
        Task<bool> ExistsAsync(Guid hotelId);
    }
}