using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para servicios de gestión de habitaciones
    /// </summary>
    public interface IRoomService
    {
        // CRUD
        Task<(bool Success, int RoomId, string? ErrorMessage)> CreateRoomAsync(
            int hotelId,
            int roomTypeId,
            string roomNumber,
            string? floor,
            string? location,
            bool allowsPets,
            bool isSmoking,
            bool isAccessible,
            int createdBy
        );

        Task<(bool Success, string? ErrorMessage)> UpdateRoomAsync(
            int roomId,
            string roomNumber,
            string? floor,
            string? location,
            bool allowsPets,
            bool isSmoking,
            bool isAccessible,
            int updatedBy
        );

        Task<(bool Success, string? ErrorMessage)> DeleteRoomAsync(int roomId, int deletedBy);

        // CONSULTAS
        Task<RoomDto?> GetRoomByIdAsync(int roomId);
        Task<RoomDto?> GetRoomByNumberAsync(int hotelId, string roomNumber);
        Task<List<RoomDto>> GetRoomsByHotelAsync(int hotelId);
        Task<List<RoomDto>> GetRoomsByTypeAsync(int roomTypeId);
        Task<List<RoomDto>> GetRoomsByStatusAsync(int hotelId, RoomStatus status);
        Task<List<RoomDto>> GetActiveRoomsAsync(int hotelId);

        // ESTADO
        Task<(bool Success, string? ErrorMessage)> UpdateRoomStatusAsync(int roomId, RoomStatus newStatus, int updatedBy);
        Task<(bool Success, string? ErrorMessage)> BlockRoomAsync(int roomId, string reason, DateTime? blockedUntil, int updatedBy);
        Task<(bool Success, string? ErrorMessage)> UnblockRoomAsync(int roomId, int updatedBy);

        // DISPONIBILIDAD
        Task<List<RoomDto>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);
        Task<List<RoomDto>> GetAvailableRoomsByTypeAsync(int roomTypeId, DateTime checkIn, DateTime checkOut);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);

        // VALIDACIONES
        Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber, int? excludeRoomId = null);
        Task<bool> CanDeleteRoomAsync(int roomId);
    }
}
