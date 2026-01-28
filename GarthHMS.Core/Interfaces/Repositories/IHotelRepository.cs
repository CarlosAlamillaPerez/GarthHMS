using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de hoteles
    /// </summary>
    public interface IHotelRepository
    {
        // CRUD
        Task<Hotel?> GetByIdAsync(int hotelId);
        Task<int> CreateAsync(Hotel hotel);
        Task<bool> UpdateAsync(Hotel hotel);
        Task<bool> DeleteAsync(int hotelId);

        // CONSULTAS
        Task<List<Hotel>> GetAllAsync();
        Task<List<Hotel>> GetAllActiveAsync();
        Task<Hotel?> GetByRFCAsync(string rfc);
        Task<bool> RFCExistsAsync(string rfc, int? excludeHotelId = null);

        // CONFIGURACIÓN
        Task<bool> UpdateSettingsAsync(int hotelId, string settingsJson);
        Task<string?> GetSettingsAsync(int hotelId);
    }
}
