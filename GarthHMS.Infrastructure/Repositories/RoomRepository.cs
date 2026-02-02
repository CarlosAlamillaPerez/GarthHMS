// GarthHMS.Infrastructure/Repositories/RoomRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de habitaciones usando Stored Procedures
    /// </summary>
    public class RoomRepository : IRoomRepository
    {
        private readonly Procedimientos _procedimientos;

        public RoomRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        public async Task<Room?> GetByIdAsync(Guid roomId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_room_get_by_id",
                new { p_room_id = roomId }
            );

            return result != null ? MapToRoom(result) : null;
        }

        public async Task<Guid> CreateAsync(Room room)
        {
            var roomId = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "sp_room_create",
                new
                {
                    p_hotel_id = room.HotelId,
                    p_room_type_id = room.RoomTypeId,
                    p_room_number = room.RoomNumber,
                    p_floor = room.Floor,
                    p_notes = room.Notes,
                    p_created_by = room.CreatedBy
                }
            );

            return roomId;
        }

        public async Task UpdateAsync(Room room)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_update",
                new
                {
                    p_room_id = room.RoomId,
                    p_room_number = room.RoomNumber,
                    p_floor = room.Floor,
                    p_notes = room.Notes
                }
            );
        }

        public async Task DeleteAsync(Guid roomId)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_delete",
                new { p_room_id = roomId }
            );
        }

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        public async Task<IEnumerable<Room>> GetByHotelAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_by_hotel",
                new { p_hotel_id = hotelId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        public async Task<IEnumerable<Room>> GetAllActiveAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_all_active",
                new { p_hotel_id = hotelId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        public async Task<IEnumerable<Room>> GetByTypeAsync(Guid roomTypeId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_by_type",
                new { p_room_type_id = roomTypeId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        public async Task<IEnumerable<Room>> GetByStatusAsync(Guid hotelId, RoomStatus status)
        {
            // Convertir el enum a string para PostgreSQL
            var statusString = status.ToString().ToLower();

            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_by_status",
                new
                {
                    p_hotel_id = hotelId,
                    p_status = statusString
                }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        public async Task<Room?> GetByNumberAsync(Guid hotelId, string roomNumber)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_room_get_by_number",
                new
                {
                    p_hotel_id = hotelId,
                    p_room_number = roomNumber
                }
            );

            return result != null ? MapToRoom(result) : null;
        }

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        public async Task<bool> RoomNumberExistsAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null)
        {
            var exists = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_room_number_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_room_number = roomNumber,
                    p_exclude_room_id = excludeRoomId
                }
            );

            return exists;
        }

        // ====================================================================
        // GESTIÓN DE ESTADOS
        // ====================================================================

        public async Task UpdateStatusAsync(Guid roomId, RoomStatus newStatus, Guid changedBy)
        {
            // Convertir el enum a string para PostgreSQL
            var statusString = newStatus.ToString().ToLower();

            await _procedimientos.EjecutarAsync(
                "sp_room_update_status",
                new
                {
                    p_room_id = roomId,
                    p_new_status = statusString,
                    p_changed_by = changedBy
                }
            );
        }

        public async Task SetMaintenanceAsync(Guid roomId, string? notes, Guid changedBy)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_set_maintenance",
                new
                {
                    p_room_id = roomId,
                    p_notes = notes,
                    p_changed_by = changedBy
                }
            );
        }
        public async Task SetAvailableAsync(Guid roomId, Guid changedBy)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_set_available",
                new
                {
                    p_room_id = roomId,
                    p_changed_by = changedBy
                }
            );
        }

        // ====================================================================
        // CONSULTAS DE DISPONIBILIDAD
        // ====================================================================

        public async Task<IEnumerable<Room>> GetAvailableAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_available",
                new { p_hotel_id = hotelId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        public async Task<IEnumerable<Room>> GetAvailableByTypeAsync(Guid roomTypeId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_get_available_by_type",
                new { p_room_type_id = roomTypeId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<Room>();

            var rooms = new List<Room>();
            foreach (var item in results)
            {
                rooms.Add(MapToRoom(item));
            }

            return rooms;
        }

        // ====================================================================
        // MAPEO DE DYNAMIC A ROOM
        // ====================================================================

        private Room MapToRoom(dynamic item)
        {
            return new Room
            {
                RoomId = item.room_id,
                HotelId = item.hotel_id,
                RoomTypeId = item.room_type_id,
                RoomNumber = item.room_number,
                Floor = item.floor,
                Status = ParseRoomStatus(item.status),
                StatusChangedAt = item.status_changed_at,
                StatusChangedBy = item.status_changed_by,
                CurrentStayId = item.current_stay_id,
                Notes = item.notes,
                IsActive = item.is_active,
                CreatedAt = item.created_at,
                UpdatedAt = item.updated_at,
                CreatedBy = item.created_by
            };
        }

        private RoomStatus ParseRoomStatus(string status)
        {
            return status?.ToLower() switch
            {
                "available" => RoomStatus.Available,
                "occupied" => RoomStatus.Occupied,
                "dirty" => RoomStatus.Dirty,
                "cleaning" => RoomStatus.Cleaning,
                "maintenance" => RoomStatus.Maintenance,
                "reserved" => RoomStatus.Reserved,
                _ => RoomStatus.Available
            };
        }
    }
}