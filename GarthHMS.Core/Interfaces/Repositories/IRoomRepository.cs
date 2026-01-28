using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos de habitaciones
    /// </summary>
    public interface IRoomRepository
    {
        // CRUD
        Task<Room?> GetByIdAsync(int roomId);
        Task<int> CreateAsync(Room room);
        Task<bool> UpdateAsync(Room room);
        Task<bool> DeleteAsync(int roomId);

        // CONSULTAS
        Task<List<Room>> GetByHotelIdAsync(int hotelId);
        Task<List<Room>> GetAllActiveAsync(int hotelId);
        Task<List<Room>> GetByRoomTypeIdAsync(int roomTypeId);
        Task<List<Room>> GetByStatusAsync(int hotelId, RoomStatus status);
        Task<Room?> GetByRoomNumberAsync(int hotelId, string roomNumber);
        Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber, int? excludeRoomId = null);

        // ESTADO
        Task<bool> UpdateStatusAsync(int roomId, RoomStatus newStatus, int updatedBy);
        Task<bool> BlockRoomAsync(int roomId, string reason, System.DateTime? blockedUntil, int updatedBy);
        Task<bool> UnblockRoomAsync(int roomId, int updatedBy);

        // DISPONIBILIDAD
        Task<List<Room>> GetAvailableRoomsAsync(int hotelId, System.DateTime checkIn, System.DateTime checkOut);
        Task<List<Room>> GetAvailableRoomsByTypeAsync(int roomTypeId, System.DateTime checkIn, System.DateTime checkOut);
    }
}
