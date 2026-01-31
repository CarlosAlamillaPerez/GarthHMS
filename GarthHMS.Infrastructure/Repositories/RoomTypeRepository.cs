using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly Procedimientos _procedimientos;

        public RoomTypeRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        public async Task<RoomType?> GetByIdAsync(Guid roomTypeId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_room_type_get_by_id",
                new { p_room_type_id = roomTypeId }
            );

            return result != null ? MapToRoomType(result) : null;
        }

        public async Task<IEnumerable<RoomType>> GetByHotelAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_type_get_by_hotel",
                new { p_hotel_id = hotelId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<RoomType>();

            // Conversión explícita elemento por elemento
            var roomTypes = new List<RoomType>();
            foreach (var item in results)
            {
                roomTypes.Add(MapToRoomType(item));
            }

            return roomTypes;
        }

        public async Task<IEnumerable<RoomType>> GetAllActiveAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_room_type_get_all_active",
                new { p_hotel_id = hotelId }
            );

            if (results == null || !results.Any())
                return Enumerable.Empty<RoomType>();

            // Conversión explícita elemento por elemento
            var roomTypes = new List<RoomType>();
            foreach (var item in results)
            {
                roomTypes.Add(MapToRoomType(item));
            }

            return roomTypes;
        }

        public async Task<RoomType?> GetByCodeAsync(Guid hotelId, string code)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_room_type_get_by_code",
                new { p_hotel_id = hotelId, p_code = code }
            );

            return result != null ? MapToRoomType(result) : null;
        }

        public async Task<Guid> CreateAsync(RoomType roomType)
        {
            var roomTypeId = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "sp_room_type_create",
                new
                {
                    p_hotel_id = roomType.HotelId,
                    p_name = roomType.Name,
                    p_code = roomType.Code,
                    p_description = roomType.Description,
                    p_base_capacity = roomType.BaseCapacity,
                    p_max_capacity = roomType.MaxCapacity,
                    p_base_price_nightly = roomType.BasePriceNightly,
                    p_base_price_hourly = roomType.BasePriceHourly,
                    p_extra_person_charge = roomType.ExtraPersonCharge,
                    p_allows_pets = roomType.AllowsPets,
                    p_pet_charge = roomType.PetCharge,
                    p_size_sqm = roomType.SizeSqm,
                    p_bed_type = roomType.BedType,
                    p_view_type = roomType.ViewType,
                    p_amenities = roomType.AmenitiesJson,
                    p_photo_urls = roomType.PhotoUrlsJson,
                    p_display_order = roomType.DisplayOrder,
                    p_created_by = roomType.CreatedBy
                }
            );

            return roomTypeId;
        }

        public async Task UpdateAsync(RoomType roomType)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_type_update",
                new
                {
                    p_room_type_id = roomType.RoomTypeId,
                    p_name = roomType.Name,
                    p_code = roomType.Code,
                    p_description = roomType.Description,
                    p_base_capacity = roomType.BaseCapacity,
                    p_max_capacity = roomType.MaxCapacity,
                    p_base_price_nightly = roomType.BasePriceNightly,
                    p_base_price_hourly = roomType.BasePriceHourly,
                    p_extra_person_charge = roomType.ExtraPersonCharge,
                    p_allows_pets = roomType.AllowsPets,
                    p_pet_charge = roomType.PetCharge,
                    p_size_sqm = roomType.SizeSqm,
                    p_bed_type = roomType.BedType,
                    p_view_type = roomType.ViewType,
                    p_amenities = roomType.AmenitiesJson,
                    p_photo_urls = roomType.PhotoUrlsJson,
                    p_display_order = roomType.DisplayOrder
                }
            );
        }

        public async Task DeleteAsync(Guid roomTypeId)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_type_delete",
                new { p_room_type_id = roomTypeId }
            );
        }

        public async Task<bool> CodeExistsAsync(Guid hotelId, string code, Guid? excludeRoomTypeId = null)
        {
            var exists = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_room_type_code_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_code = code,
                    p_exclude_room_type_id = excludeRoomTypeId
                }
            );

            return exists;
        }

        public async Task<bool> NameExistsAsync(Guid hotelId, string name, Guid? excludeRoomTypeId = null)
        {
            var exists = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_room_type_name_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_name = name,
                    p_exclude_room_type_id = excludeRoomTypeId
                }
            );

            return exists;
        }

        public async Task UpdateDisplayOrderAsync(Guid roomTypeId, int newOrder)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_type_update_order",
                new
                {
                    p_room_type_id = roomTypeId,
                    p_new_order = newOrder
                }
            );
        }

        public async Task UpdatePricesAsync(Guid roomTypeId, decimal basePriceNightly, decimal basePriceHourly)
        {
            await _procedimientos.EjecutarAsync(
                "sp_room_type_update_prices",
                new
                {
                    p_room_type_id = roomTypeId,
                    p_base_price_nightly = basePriceNightly,
                    p_base_price_hourly = basePriceHourly
                }
            );
        }

        private RoomType MapToRoomType(dynamic data)
        {
            return new RoomType
            {
                RoomTypeId = data.room_type_id,
                HotelId = data.hotel_id,
                Name = data.name,
                Code = data.code,
                Description = data.description,
                BaseCapacity = data.base_capacity,
                MaxCapacity = data.max_capacity,
                BasePriceNightly = data.base_price_nightly,
                BasePriceHourly = data.base_price_hourly,
                ExtraPersonCharge = data.extra_person_charge,
                AllowsPets = data.allows_pets,
                PetCharge = data.pet_charge,
                SizeSqm = data.size_sqm,
                BedType = data.bed_type,
                ViewType = data.view_type,
                AmenitiesJson = data.amenities?.ToString() ?? "[]",
                PhotoUrlsJson = data.photo_urls?.ToString() ?? "[]",
                DisplayOrder = data.display_order,
                IsActive = data.is_active,
                CreatedAt = data.created_at,
                UpdatedAt = data.updated_at,
                CreatedBy = data.created_by
            };
        }
    }
}