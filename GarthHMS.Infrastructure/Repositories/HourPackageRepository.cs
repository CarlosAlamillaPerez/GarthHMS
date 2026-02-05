// GarthHMS.Infrastructure/Repositories/HourPackageRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class HourPackageRepository : IHourPackageRepository
    {
        private readonly Procedimientos _procedimientos;
        private readonly BaseDeDatos _baseDeDatos;

        public HourPackageRepository(Procedimientos procedimientos, BaseDeDatos baseDeDatos)
        {
            _procedimientos = procedimientos;
            _baseDeDatos = baseDeDatos;
        }

        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        public async Task<HourPackage?> GetByIdAsync(Guid hourPackageId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_hourpackage_get_by_id",
                new { p_hour_package_id = hourPackageId }
            );

            return result != null ? MapDynamicToEntity(result) : null;
        }

        public async Task<Guid> CreateAsync(HourPackage hourPackage)
        {
            var newId = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "sp_hourpackage_create",
                new
                {
                    p_hotel_id = hourPackage.HotelId,
                    p_room_type_id = hourPackage.RoomTypeId,
                    p_name = hourPackage.Name,
                    p_hours = hourPackage.Hours,
                    p_price = hourPackage.Price,
                    p_extra_hour_price = hourPackage.ExtraHourPrice,
                    p_allows_extensions = hourPackage.AllowsExtensions,
                    p_display_order = hourPackage.DisplayOrder,
                    p_created_by = hourPackage.CreatedBy
                }
            );

            return newId;
        }

        public async Task UpdateAsync(HourPackage hourPackage)
        {
            await _procedimientos.EjecutarAsync(
                "sp_hourpackage_update",
                new
                {
                    p_hour_package_id = hourPackage.HourPackageId,
                    p_name = hourPackage.Name,
                    p_hours = hourPackage.Hours,
                    p_price = hourPackage.Price,
                    p_extra_hour_price = hourPackage.ExtraHourPrice,
                    p_allows_extensions = hourPackage.AllowsExtensions,
                    p_display_order = hourPackage.DisplayOrder
                }
            );
        }

        public async Task DeleteAsync(Guid hourPackageId)
        {
            await _procedimientos.EjecutarAsync(
                "sp_hourpackage_delete",
                new { p_hour_package_id = hourPackageId }
            );
        }

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        public async Task<IEnumerable<HourPackage>> GetByHotelAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_hourpackage_get_by_hotel",
                new { p_hotel_id = hotelId }
            );

            return result?.Select(MapDynamicToEntity) ?? new List<HourPackage>();
        }

        public async Task<IEnumerable<HourPackage>> GetAllActiveAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_hourpackage_get_all_active",
                new { p_hotel_id = hotelId }
            );

            return result?.Select(MapDynamicToEntity) ?? new List<HourPackage>();
        }

        public async Task<IEnumerable<HourPackage>> GetByRoomTypeAsync(Guid roomTypeId)
        {
            var result = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_hourpackage_get_by_room_type",
                new { p_room_type_id = roomTypeId }
            );

            return result?.Select(MapDynamicToEntity) ?? new List<HourPackage>();
        }

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        public async Task<bool> ExistsAsync(Guid hotelId, Guid roomTypeId, int hours, Guid? excludeHourPackageId = null)
        {
            var exists = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_hourpackage_exists",
                new
                {
                    p_hotel_id = hotelId,
                    p_room_type_id = roomTypeId,
                    p_hours = hours,
                    p_exclude_hour_package_id = excludeHourPackageId
                }
            );

            return exists;
        }

        // ====================================================================
        // MAPEO
        // ====================================================================

        private HourPackage MapDynamicToEntity(dynamic d)
        {
            return new HourPackage
            {
                HourPackageId = Guid.Parse(d.hour_package_id.ToString()),
                HotelId = Guid.Parse(d.hotel_id.ToString()),
                RoomTypeId = Guid.Parse(d.room_type_id.ToString()),
                Name = d.name,
                Hours = d.hours,
                Price = d.price,
                ExtraHourPrice = d.extra_hour_price,
                AllowsExtensions = d.allows_extensions,
                DisplayOrder = d.display_order,
                IsActive = d.is_active,
                CreatedAt = d.created_at,
                UpdatedAt = d.updated_at,
                CreatedBy = d.created_by != null ? Guid.Parse(d.created_by.ToString()) : null
            };
        }
    }
}