using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio para gestión de habitaciones
    /// </summary>
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ILogger<RoomService> _logger;

        public RoomService(
            IRoomRepository roomRepository,
            IRoomTypeRepository roomTypeRepository,
            ILogger<RoomService> logger)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
        }

        // CRUD

    //    public async Task<(bool Success, int RoomId, string? ErrorMessage)> CreateRoomAsync(
    //        int hotelId,
    //        int roomTypeId,
    //        string roomNumber,
    //        string? floor,
    //        string? location,
    //        bool allowsPets,
    //        bool isSmoking,
    //        bool isAccessible,
    //        int createdBy)
    //    {
    //        try
    //        {
    //            // Validar que el número de habitación no exista
    //            var numberExists = await _roomRepository.RoomNumberExistsAsync(hotelId, roomNumber);
    //            if (numberExists)
    //            {
    //                return (false, 0, "Ya existe una habitación con ese número");
    //            }

    //            // Verificar que el tipo de habitación exista
    //            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
    //            if (roomType == null)
    //            {
    //                return (false, 0, "Tipo de habitación no válido");
    //            }

    //            var room = new Room
    //            {
    //                HotelId = hotelId,
    //                RoomTypeId = roomTypeId,
    //                RoomNumber = roomNumber,
    //                Floor = floor,
    //                Location = location,
    //                Status = RoomStatus.Available,
    //                IsSmoking = isSmoking,
    //                IsAccessible = isAccessible,
    //                AllowsPets = allowsPets,
    //                IsBlocked = false,
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                CreatedBy = createdBy
    //            };

    //            var roomId = await _roomRepository.CreateAsync(room);

    //            if (roomId > 0)
    //            {
    //                _logger.LogInformation("Room created: {RoomNumber} for hotel {HotelId} by user {CreatedBy}",
    //                    roomNumber, hotelId, createdBy);
    //                return (true, roomId, null);
    //            }

    //            return (false, 0, "Error al crear la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error creating room {RoomNumber} for hotel {HotelId}", roomNumber, hotelId);
    //            return (false, 0, "Error al crear la habitación");
    //        }
    //    }

    //    public async Task<(bool Success, string? ErrorMessage)> UpdateRoomAsync(
    //        int roomId,
    //        string roomNumber,
    //        string? floor,
    //        string? location,
    //        bool allowsPets,
    //        bool isSmoking,
    //        bool isAccessible,
    //        int updatedBy)
    //    {
    //        try
    //        {
    //            var room = await _roomRepository.GetByIdAsync(roomId);
    //            if (room == null)
    //            {
    //                return (false, "Habitación no encontrada");
    //            }

    //            // Validar número único (excluyendo la actual)
    //            var numberExists = await _roomRepository.RoomNumberExistsAsync(
    //                room.HotelId, roomNumber, roomId);
    //            if (numberExists)
    //            {
    //                return (false, "Ya existe una habitación con ese número");
    //            }

    //            room.RoomNumber = roomNumber;
    //            room.Floor = floor;
    //            room.Location = location;
    //            room.AllowsPets = allowsPets;
    //            room.IsSmoking = isSmoking;
    //            room.IsAccessible = isAccessible;
    //            room.UpdatedAt = DateTime.UtcNow;
    //            room.UpdatedBy = updatedBy;

    //            var result = await _roomRepository.UpdateAsync(room);

    //            if (result)
    //            {
    //                _logger.LogInformation("Room {RoomId} updated by user {UpdatedBy}", roomId, updatedBy);
    //                return (true, null);
    //            }

    //            return (false, "Error al actualizar la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error updating room {RoomId}", roomId);
    //            return (false, "Error al actualizar la habitación");
    //        }
    //    }

    //    public async Task<(bool Success, string? ErrorMessage)> DeleteRoomAsync(int roomId, int deletedBy)
    //    {
    //        try
    //        {
    //            // Verificar que no tenga reservas activas
    //            var canDelete = await CanDeleteRoomAsync(roomId);
    //            if (!canDelete)
    //            {
    //                return (false, "No se puede eliminar. La habitación tiene reservas activas");
    //            }

    //            // Desactivar en lugar de eliminar
    //            var room = await _roomRepository.GetByIdAsync(roomId);
    //            if (room == null)
    //            {
    //                return (false, "Habitación no encontrada");
    //            }

    //            room.IsActive = false;
    //            room.UpdatedAt = DateTime.UtcNow;
    //            room.UpdatedBy = deletedBy;

    //            var result = await _roomRepository.UpdateAsync(room);

    //            if (result)
    //            {
    //                _logger.LogInformation("Room {RoomId} deleted by user {DeletedBy}", roomId, deletedBy);
    //                return (true, null);
    //            }

    //            return (false, "Error al eliminar la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error deleting room {RoomId}", roomId);
    //            return (false, "Error al eliminar la habitación");
    //        }
    //    }

    //    // CONSULTAS

    //    public async Task<RoomDto?> GetRoomByIdAsync(int roomId)
    //    {
    //        try
    //        {
    //            var room = await _roomRepository.GetByIdAsync(roomId);
    //            if (room == null)
    //                return null;

    //            var roomType = await _roomTypeRepository.GetByIdAsync(room.RoomTypeId);
    //            return MapToDto(room, roomType?.TypeName ?? "");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting room {RoomId}", roomId);
    //            return null;
    //        }
    //    }

    //    public async Task<RoomDto?> GetRoomByNumberAsync(int hotelId, string roomNumber)
    //    {
    //        try
    //        {
    //            var room = await _roomRepository.GetByRoomNumberAsync(hotelId, roomNumber);
    //            if (room == null)
    //                return null;

    //            var roomType = await _roomTypeRepository.GetByIdAsync(room.RoomTypeId);
    //            return MapToDto(room, roomType?.TypeName ?? "");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting room by number {RoomNumber}", roomNumber);
    //            return null;
    //        }
    //    }

    //    public async Task<List<RoomDto>> GetRoomsByHotelAsync(int hotelId)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetByHotelIdAsync(hotelId);
    //            var roomTypes = await _roomTypeRepository.GetByHotelIdAsync(hotelId);

    //            return rooms.Select(r => MapToDto(r,
    //                roomTypes.FirstOrDefault(rt => rt.RoomTypeId == r.RoomTypeId)?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting rooms for hotel {HotelId}", hotelId);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    public async Task<List<RoomDto>> GetRoomsByTypeAsync(int roomTypeId)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetByRoomTypeIdAsync(roomTypeId);
    //            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);

    //            return rooms.Select(r => MapToDto(r, roomType?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting rooms for type {RoomTypeId}", roomTypeId);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    public async Task<List<RoomDto>> GetRoomsByStatusAsync(int hotelId, RoomStatus status)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetByStatusAsync(hotelId, status);
    //            var roomTypes = await _roomTypeRepository.GetByHotelIdAsync(hotelId);

    //            return rooms.Select(r => MapToDto(r,
    //                roomTypes.FirstOrDefault(rt => rt.RoomTypeId == r.RoomTypeId)?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting rooms by status {Status}", status);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    public async Task<List<RoomDto>> GetActiveRoomsAsync(int hotelId)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetAllActiveAsync(hotelId);
    //            var roomTypes = await _roomTypeRepository.GetByHotelIdAsync(hotelId);

    //            return rooms.Select(r => MapToDto(r,
    //                roomTypes.FirstOrDefault(rt => rt.RoomTypeId == r.RoomTypeId)?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting active rooms for hotel {HotelId}", hotelId);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    // ESTADO

    //    public async Task<(bool Success, string? ErrorMessage)> UpdateRoomStatusAsync(
    //        int roomId,
    //        RoomStatus newStatus,
    //        int updatedBy)
    //    {
    //        try
    //        {
    //            var result = await _roomRepository.UpdateStatusAsync(roomId, newStatus, updatedBy);

    //            if (result)
    //            {
    //                _logger.LogInformation("Room {RoomId} status changed to {Status} by user {UpdatedBy}",
    //                    roomId, newStatus, updatedBy);
    //                return (true, null);
    //            }

    //            return (false, "Error al actualizar el estado de la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error updating room status {RoomId}", roomId);
    //            return (false, "Error al actualizar el estado de la habitación");
    //        }
    //    }

    //    public async Task<(bool Success, string? ErrorMessage)> BlockRoomAsync(
    //        int roomId,
    //        string reason,
    //        DateTime? blockedUntil,
    //        int updatedBy)
    //    {
    //        try
    //        {
    //            var result = await _roomRepository.BlockRoomAsync(roomId, reason, blockedUntil, updatedBy);

    //            if (result)
    //            {
    //                _logger.LogInformation("Room {RoomId} blocked by user {UpdatedBy}. Reason: {Reason}",
    //                    roomId, updatedBy, reason);
    //                return (true, null);
    //            }

    //            return (false, "Error al bloquear la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error blocking room {RoomId}", roomId);
    //            return (false, "Error al bloquear la habitación");
    //        }
    //    }

    //    public async Task<(bool Success, string? ErrorMessage)> UnblockRoomAsync(int roomId, int updatedBy)
    //    {
    //        try
    //        {
    //            var result = await _roomRepository.UnblockRoomAsync(roomId, updatedBy);

    //            if (result)
    //            {
    //                _logger.LogInformation("Room {RoomId} unblocked by user {UpdatedBy}", roomId, updatedBy);
    //                return (true, null);
    //            }

    //            return (false, "Error al desbloquear la habitación");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error unblocking room {RoomId}", roomId);
    //            return (false, "Error al desbloquear la habitación");
    //        }
    //    }

    //    // DISPONIBILIDAD

    //    public async Task<List<RoomDto>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetAvailableRoomsAsync(hotelId, checkIn, checkOut);
    //            var roomTypes = await _roomTypeRepository.GetByHotelIdAsync(hotelId);

    //            return rooms.Select(r => MapToDto(r,
    //                roomTypes.FirstOrDefault(rt => rt.RoomTypeId == r.RoomTypeId)?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting available rooms for hotel {HotelId}", hotelId);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    public async Task<List<RoomDto>> GetAvailableRoomsByTypeAsync(
    //        int roomTypeId,
    //        DateTime checkIn,
    //        DateTime checkOut)
    //    {
    //        try
    //        {
    //            var rooms = await _roomRepository.GetAvailableRoomsByTypeAsync(roomTypeId, checkIn, checkOut);
    //            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);

    //            return rooms.Select(r => MapToDto(r, roomType?.TypeName ?? "")).ToList();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error getting available rooms for type {RoomTypeId}", roomTypeId);
    //            return new List<RoomDto>();
    //        }
    //    }

    //    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
    //    {
    //        try
    //        {
    //            var room = await _roomRepository.GetByIdAsync(roomId);
    //            if (room == null || !room.IsActive || room.IsBlocked)
    //                return false;

    //            // TODO: Verificar contra reservas existentes
    //            return room.Status == RoomStatus.Available;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error checking room availability {RoomId}", roomId);
    //            return false;
    //        }
    //    }

    //    // VALIDACIONES

    //    public async Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber, int? excludeRoomId = null)
    //    {
    //        try
    //        {
    //            return await _roomRepository.RoomNumberExistsAsync(hotelId, roomNumber, excludeRoomId);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error checking if room number exists {RoomNumber}", roomNumber);
    //            return false;
    //        }
    //    }

    //    public async Task<bool> CanDeleteRoomAsync(int roomId)
    //    {
    //        try
    //        {
    //            // TODO: Verificar contra reservas activas
    //            var room = await _roomRepository.GetByIdAsync(roomId);
    //            return room != null && room.Status == RoomStatus.Available;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error checking if room can be deleted {RoomId}", roomId);
    //            return false;
    //        }
    //    }

    //    // HELPERS

    //    private static RoomDto MapToDto(Room room, string roomTypeName)
    //    {
    //        return new RoomDto
    //        {
    //            RoomId = room.RoomId,
    //            RoomTypeId = room.RoomTypeId,
    //            RoomTypeName = roomTypeName,
    //            RoomNumber = room.RoomNumber,
    //            Floor = room.Floor,
    //            Status = room.Status,
    //            StatusText = room.Status.ToString(),
    //            IsBlocked = room.IsBlocked,
    //            BlockReason = room.BlockReason,
    //            AllowsPets = room.AllowsPets,
    //            IsSmoking = room.IsSmoking,
    //            IsActive = room.IsActive
    //        };
    //    }
    }
}
