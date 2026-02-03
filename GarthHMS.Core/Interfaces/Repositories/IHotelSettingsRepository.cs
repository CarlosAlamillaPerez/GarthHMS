using System;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IHotelSettingsRepository
    {
        /// <summary>
        /// Obtiene la configuración del hotel por HotelId
        /// </summary>
        Task<HotelSettings?> GetByHotelIdAsync(Guid hotelId);

        /// <summary>
        /// Actualiza la configuración del hotel
        /// </summary>
        Task UpdateAsync(HotelSettings settings);

        /// <summary>
        /// Verifica si existe configuración para el hotel
        /// </summary>
        Task<bool> ExistsAsync(Guid hotelId);

        /// <summary>
        /// Crea la configuración inicial del hotel
        /// </summary>
        Task<Guid> CreateAsync(HotelSettings settings);
    }
}