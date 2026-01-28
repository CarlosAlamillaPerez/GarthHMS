using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de habitaciones
    /// </summary>
    public class RoomRepository : IRoomRepository
    {
        private readonly Procedimientos _procedimientos;

        public RoomRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // CRUD

        public async Task<Room?> GetByIdAsync(int roomId)
        {
            return await _procedimientos.EjecutarUnicoAsync<Room>(
                "sp_room_get_by_id",
                new { p_room_id = roomId }
            );
        }

        public async Task<int> CreateAsync(Room room)
        {
            var roomId = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_room_create",
                new
                {
                    p_hotel_id = room.HotelId,
                    p_room_type_id = room.RoomTypeId,
                    p_room_number = room.RoomNumber,
                    p_floor = room.Floor,
                    p_location = room.Location,
                    p_status = room.Status,
                    p_is_smoking = room.IsSmoking,
                    p_is_accessible = room.IsAccessible,
                    p_allows_pets = room.AllowsPets,
                    p_is_blocked = room.IsBlocked,
                    p_block_reason = room.BlockReason,
                    p_blocked_until = room.BlockedUntil,
                    p_notes = room.Notes,
                    p_is_active = room.IsActive,
                    p_created_by = room.CreatedBy
                }
            );

            return roomId;
        }

        public async Task<bool> UpdateAsync(Room room)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_room_update",
                new
                {
                    p_room_id = room.RoomId,
                    p_room_number = room.RoomNumber,
                    p_floor = room.Floor,
                    p_location = room.Location,
                    p_is_smoking = room.IsSmoking,
                    p_is_accessible = room.IsAccessible,
                    p_allows_pets = room.AllowsPets,
                    p_notes = room.Notes,
                    p_is_active = room.IsActive,
                    p_updated_by = room.UpdatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int roomId)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_room_delete",
                new { p_room_id = roomId }
            );

            return rowsAffected > 0;
        }

        // CONSULTAS

        public async Task<List<Room>> GetByHotelIdAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_by_hotel",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<List<Room>> GetAllActiveAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_all_active",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<List<Room>> GetByRoomTypeIdAsync(int roomTypeId)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_by_type",
                new { p_room_type_id = roomTypeId }
            );
        }

        public async Task<List<Room>> GetByStatusAsync(int hotelId, RoomStatus status)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_by_status",
                new
                {
                    p_hotel_id = hotelId,
                    p_status = status
                }
            );
        }

        public async Task<Room?> GetByRoomNumberAsync(int hotelId, string roomNumber)
        {
            return await _procedimientos.EjecutarUnicoAsync<Room>(
                "sp_room_get_by_number",
                new
                {
                    p_hotel_id = hotelId,
                    p_room_number = roomNumber
                }
            );
        }

        public async Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber, int? excludeRoomId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_room_number_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_room_number = roomNumber,
                    p_exclude_room_id = excludeRoomId
                }
            );

            return result;
        }

        // ESTADO

        public async Task<bool> UpdateStatusAsync(int roomId, RoomStatus newStatus, int updatedBy)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_room_update_status",
                new
                {
                    p_room_id = roomId,
                    p_new_status = newStatus,
                    p_updated_by = updatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> BlockRoomAsync(int roomId, string reason, DateTime? blockedUntil, int updatedBy)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_room_block",
                new
                {
                    p_room_id = roomId,
                    p_block_reason = reason,
                    p_blocked_until = blockedUntil,
                    p_updated_by = updatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> UnblockRoomAsync(int roomId, int updatedBy)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_room_unblock",
                new
                {
                    p_room_id = roomId,
                    p_updated_by = updatedBy
                }
            );

            return rowsAffected > 0;
        }

        // DISPONIBILIDAD

        public async Task<List<Room>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_available",
                new
                {
                    p_hotel_id = hotelId,
                    p_check_in = checkIn,
                    p_check_out = checkOut
                }
            );
        }

        public async Task<List<Room>> GetAvailableRoomsByTypeAsync(int roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            return await _procedimientos.EjecutarListaAsync<Room>(
                "sp_room_get_available_by_type",
                new
                {
                    p_room_type_id = roomTypeId,
                    p_check_in = checkIn,
                    p_check_out = checkOut
                }
            );
        }
    }
}
