using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de hoteles
    /// </summary>
    public interface IHotelService
    {
        // CRUD
        Task<(bool Success, int HotelId, string? ErrorMessage)> CreateHotelAsync(
            string hotelName,
            string legalName,
            string? rfc,
            string email,
            string phone,
            bool isMotel,
            int createdBy
        );

        Task<(bool Success, string? ErrorMessage)> UpdateHotelAsync(
            int hotelId,
            string hotelName,
            string? phone,
            string? email,
            string? city,
            string? state,
            int updatedBy
        );

        // CONSULTAS
        Task<HotelDto?> GetHotelByIdAsync(int hotelId);
        Task<List<HotelDto>> GetAllHotelsAsync();
        Task<List<HotelDto>> GetActiveHotelsAsync();

        // CONFIGURACIÓN
        Task<bool> UpdateHotelSettingsAsync(int hotelId, string settingsJson, int updatedBy);
        Task<string?> GetHotelSettingsAsync(int hotelId);

        // VALIDACIONES
        Task<bool> RFCExistsAsync(string rfc, int? excludeHotelId = null);
    }
}
