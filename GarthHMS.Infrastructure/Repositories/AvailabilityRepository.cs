// GarthHMS.Infrastructure/Repositories/AvailabilityRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.AvailabilityEngine;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio del Motor de Disponibilidad usando Stored Procedures
    /// </summary>
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly Procedimientos _procedimientos;

        public AvailabilityRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // ====================================================================
        // CALENDARIO
        // ====================================================================

        public async Task<IEnumerable<CalendarDayDto>> GetMonthlyCalendarAsync(
            Guid hotelId, int year, int month)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_get_monthly_calendar",
                new
                {
                    p_hotel_id = hotelId,
                    p_year = year,
                    p_month = month
                }
            );

            var list = new List<CalendarDayDto>();
            if (results == null) return list;

            foreach (var row in results)
            {
                list.Add(new CalendarDayDto
                {
                    DayDate = ((DateOnly)row.day_date).ToDateTime(TimeOnly.MinValue),
                    TotalRooms = (long)row.total_rooms,
                    OccupiedRooms = (long)row.occupied_rooms,
                    ReservationCount = (long)row.reservation_count,
                    AvailableRooms = (long)row.available_rooms,
                    OccupancyPercent = (decimal)row.occupancy_percent
                });
            }
            return list;
        }

        public async Task<DaySummaryDto?> GetDaySummaryAsync(Guid hotelId, DateTime date)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_availability_get_day_summary",
                new
                {
                    p_hotel_id = hotelId,
                    p_date = DateOnly.FromDateTime(date)
                }
            );

            if (result == null) return null;

            return new DaySummaryDto
            {
                TotalRooms = (long)result.total_rooms,
                OccupiedRooms = (long)result.occupied_rooms,
                AvailableRooms = (long)result.available_rooms,
                ReservedRooms = (long)result.reserved_rooms,
                MaintenanceRooms = (long)result.maintenance_rooms,
                OccupancyPercent = (decimal)result.occupancy_percent
            };
        }

        // ====================================================================
        // LISTAS DE RESERVAS
        // ====================================================================

        public async Task<IEnumerable<ReservationListItemDto>> GetReservationsByDateAsync(
            Guid hotelId, DateTime date)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_get_reservations_by_date",
                new
                {
                    p_hotel_id = hotelId,
                    p_date = DateOnly.FromDateTime(date)
                }
            );

            return MapToReservationList(results);
        }

        public async Task<IEnumerable<ReservationListItemDto>> GetTodayReservationsAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_get_today_reservations",
                new { p_hotel_id = hotelId }
            );

            return MapToReservationList(results);
        }

        public async Task<IEnumerable<ReservationListItemDto>> GetUpcomingReservationsAsync(
            Guid hotelId, int days = 7)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_get_upcoming_reservations",
                new
                {
                    p_hotel_id = hotelId,
                    p_days = days
                }
            );

            return MapToReservationList(results);
        }

        // ====================================================================
        // VERIFICACIÓN DE DISPONIBILIDAD
        // ====================================================================

        public async Task<bool> CheckRoomAvailabilityAsync(
            Guid roomId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_availability_check_room_dates",
                new
                {
                    p_room_id = roomId,
                    p_check_in_date = DateOnly.FromDateTime(checkInDate),
                    p_check_out_date = DateOnly.FromDateTime(checkOutDate),
                    p_exclude_reservation_id = excludeReservationId
                }
            );

            return result;
        }

        public async Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(
            Guid hotelId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? roomTypeId = null)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_get_available_rooms",
                new
                {
                    p_hotel_id = hotelId,
                    p_check_in_date = DateOnly.FromDateTime(checkInDate),
                    p_check_out_date = DateOnly.FromDateTime(checkOutDate),
                    p_room_type_id = roomTypeId
                }
            );

            var list = new List<AvailableRoomDto>();
            if (results == null) return list;

            foreach (var row in results)
            {
                list.Add(new AvailableRoomDto
                {
                    RoomId = (Guid)row.room_id,
                    RoomNumber = (string)row.room_number,
                    Floor = (int)row.floor,
                    RoomTypeId = (Guid)row.room_type_id,
                    RoomTypeName = (string)row.room_type_name,
                    RoomTypeCode = (string)row.room_type_code,
                    BaseCapacity = (int)row.base_capacity,
                    MaxCapacity = (int)row.max_capacity,
                    BasePriceNightly = (decimal)row.base_price_nightly,
                    ExtraPersonCharge = (decimal)row.extra_person_charge,
                    AllowsPets = (bool)row.allows_pets,
                    PetCharge = (decimal)row.pet_charge,
                    BedType = row.bed_type != null ? (string)row.bed_type : null,
                    ViewType = row.view_type != null ? (string)row.view_type : null,
                    SizeSqm = row.size_sqm != null ? (decimal?)row.size_sqm : null,
                    Amenities = row.amenities != null ? row.amenities.ToString() : null
                });
            }
            return list;
        }

        // ====================================================================
        // MAPPER PRIVADO
        // ====================================================================

        private static IEnumerable<ReservationListItemDto> MapToReservationList(
            IEnumerable<dynamic> results)
        {
            var list = new List<ReservationListItemDto>();
            if (results == null) return list;

            foreach (var row in results)
            {
                list.Add(new ReservationListItemDto
                {
                    ReservationId = (Guid)row.reservation_id,
                    Folio = (string)row.folio,
                    Status = row.status?.ToString() ?? string.Empty,
                    ReservationType = row.reservation_type?.ToString() ?? string.Empty,
                    Source = row.source?.ToString() ?? string.Empty,
                    CheckInDate = ((DateOnly)row.check_in_date).ToDateTime(TimeOnly.MinValue),
                    CheckOutDate = ((DateOnly)row.check_out_date).ToDateTime(TimeOnly.MinValue),
                    NumNights = row.num_nights != null ? (int?)row.num_nights : null,
                    Total = (decimal)row.total,
                    DepositAmount = row.deposit_amount != null ? (decimal)row.deposit_amount : 0,
                    DepositPaidAt = row.deposit_paid_at != null ? (DateTime?)row.deposit_paid_at : null,
                    BalancePending = row.balance_pending != null ? (decimal)row.balance_pending : 0,
                    HasUnverifiedPayments = row.has_unverified_payments ?? false,
                    GuestId = (Guid)row.guest_id,
                    GuestFirstName = (string)row.guest_first_name,
                    GuestLastName = (string)row.guest_last_name,
                    GuestPhone = (string)row.guest_phone,
                    IsVip = (bool)row.is_vip,
                    RoomsSummary = row.rooms_summary != null ? (string)row.rooms_summary : string.Empty,
                    RoomCount = (long)row.room_count
                });
            }
            return list;
        }

        // ====================================================================
        // BÚSQUEDA GLOBAL
        // ====================================================================

        public async Task<IEnumerable<ReservationListItemDto>> SearchReservationsAsync(
            Guid hotelId, string query, int limit = 20)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_availability_search_reservations",
                new
                {
                    p_hotel_id = hotelId,
                    p_query = query,
                    p_limit = limit
                }
            );

            var list = new List<ReservationListItemDto>();
            if (results == null) return list;

            foreach (var row in results)
            {
                list.Add(new ReservationListItemDto
                {
                    ReservationId = row.reservation_id,
                    Folio = row.folio?.ToString() ?? "",
                    Status = row.status?.ToString() ?? "",
                    ReservationType = row.reservation_type?.ToString() ?? "nightly",
                    Source = row.source?.ToString() ?? "",
                    CheckInDate = ((DateOnly)row.check_in_date).ToDateTime(TimeOnly.MinValue),
                    CheckOutDate = ((DateOnly)row.check_out_date).ToDateTime(TimeOnly.MinValue),
                    NumNights = row.num_nights,
                    Total = row.total,
                    DepositAmount = row.deposit_amount,
                    DepositPaidAt = row.deposit_paid_at,
                    BalancePending = row.balance_pending,
                    HasUnverifiedPayments = row.has_unverified_payments ?? false,
                    GuestId = row.guest_id,
                    GuestFirstName = row.guest_first_name?.ToString() ?? "",
                    GuestLastName = row.guest_last_name?.ToString() ?? "",
                    GuestPhone = row.guest_phone?.ToString() ?? "",
                    IsVip = row.is_vip,
                    RoomsSummary = row.rooms_summary?.ToString() ?? "",
                    RoomCount = row.room_count
                });
            }

            return list;
        }

    }
}