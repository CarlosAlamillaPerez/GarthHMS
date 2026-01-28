using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de tipos de habitación
    /// </summary>
    public interface IRoomTypeService
    {
        // CRUD
        Task<(bool Success, int RoomTypeId, string? ErrorMessage)> CreateRoomTypeAsync(
            int hotelId,
            string typeName,
            string? description,
            int maxOccupancy,
            decimal basePrice,
            decimal? weekendPrice,
            string? amenitiesJson,
            int createdBy
        );

        Task<(bool Success, string? ErrorMessage)> UpdateRoomTypeAsync(
            int roomTypeId,
            string typeName,
            string? description,
            int maxOccupancy,
            decimal basePrice,
            decimal? weekendPrice,
            string? amenitiesJson,
            int updatedBy
        );

        Task<(bool Success, string? ErrorMessage)> DeleteRoomTypeAsync(int roomTypeId, int deletedBy);

        // CONSULTAS
        Task<RoomTypeDto?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<List<RoomTypeDto>> GetRoomTypesByHotelAsync(int hotelId);
        Task<List<RoomTypeDto>> GetActiveRoomTypesAsync(int hotelId);

        // ESTADO
        Task<bool> ActivateRoomTypeAsync(int roomTypeId, int updatedBy);
        Task<bool> DeactivateRoomTypeAsync(int roomTypeId, int updatedBy);

        // VALIDACIONES
        Task<bool> TypeNameExistsAsync(int hotelId, string typeName, int? excludeRoomTypeId = null);

        // ESTADÍSTICAS
        Task<(int Total, int Available)> GetRoomCountsAsync(int roomTypeId);
    }
}
