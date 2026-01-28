using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de tipos de habitación
    /// </summary>
    public interface IRoomTypeRepository
    {
        // CRUD
        Task<RoomType?> GetByIdAsync(int roomTypeId);
        Task<int> CreateAsync(RoomType roomType);
        Task<bool> UpdateAsync(RoomType roomType);
        Task<bool> DeleteAsync(int roomTypeId);

        // CONSULTAS
        Task<List<RoomType>> GetByHotelIdAsync(int hotelId);
        Task<List<RoomType>> GetAllActiveAsync(int hotelId);
        Task<bool> TypeNameExistsAsync(int hotelId, string typeName, int? excludeRoomTypeId = null);

        // ESTADÍSTICAS
        Task<int> GetTotalRoomsCountAsync(int roomTypeId);
        Task<int> GetAvailableRoomsCountAsync(int roomTypeId);
    }
}
