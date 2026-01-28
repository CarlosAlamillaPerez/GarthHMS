using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de tipos de habitación
    /// </summary>
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly Procedimientos _procedimientos;

        public RoomTypeRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // CRUD

        public async Task<RoomType?> GetByIdAsync(int roomTypeId)
        {
            return await _procedimientos.EjecutarUnicoAsync<RoomType>(
                "sp_roomtype_get_by_id",
                new { p_room_type_id = roomTypeId }
            );
        }

        public async Task<int> CreateAsync(RoomType roomType)
        {
            var roomTypeId = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_roomtype_create",
                new
                {
                    p_hotel_id = roomType.HotelId,
                    p_type_name = roomType.TypeName,
                    p_description = roomType.Description,
                    p_max_occupancy = roomType.MaxOccupancy,
                    p_base_price = roomType.BasePrice,
                    p_weekend_price = roomType.WeekendPrice,
                    p_amenities_json = roomType.AmenitiesJson,
                    p_is_active = roomType.IsActive,
                    p_created_by = roomType.CreatedBy
                }
            );

            return roomTypeId;
        }

        public async Task<bool> UpdateAsync(RoomType roomType)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_roomtype_update",
                new
                {
                    p_room_type_id = roomType.RoomTypeId,
                    p_type_name = roomType.TypeName,
                    p_description = roomType.Description,
                    p_max_occupancy = roomType.MaxOccupancy,
                    p_base_price = roomType.BasePrice,
                    p_weekend_price = roomType.WeekendPrice,
                    p_amenities_json = roomType.AmenitiesJson,
                    p_is_active = roomType.IsActive,
                    p_updated_by = roomType.UpdatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int roomTypeId)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_roomtype_delete",
                new { p_room_type_id = roomTypeId }
            );

            return rowsAffected > 0;
        }

        // CONSULTAS

        public async Task<List<RoomType>> GetByHotelIdAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<RoomType>(
                "sp_roomtype_get_by_hotel",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<List<RoomType>> GetAllActiveAsync(int hotelId)
        {
            return await _procedimientos.EjecutarListaAsync<RoomType>(
                "sp_roomtype_get_all_active",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<bool> TypeNameExistsAsync(int hotelId, string typeName, int? excludeRoomTypeId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_roomtype_name_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_type_name = typeName,
                    p_exclude_room_type_id = excludeRoomTypeId
                }
            );

            return result;
        }

        // ESTADÍSTICAS

        public async Task<int> GetTotalRoomsCountAsync(int roomTypeId)
        {
            var count = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_roomtype_get_total_rooms",
                new { p_room_type_id = roomTypeId }
            );

            return count;
        }

        public async Task<int> GetAvailableRoomsCountAsync(int roomTypeId)
        {
            var count = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_roomtype_get_available_rooms",
                new { p_room_type_id = roomTypeId }
            );

            return count;
        }
    }
}
