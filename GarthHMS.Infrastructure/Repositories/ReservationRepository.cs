// GarthHMS.Infrastructure/Repositories/ReservationRepository.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Reservation;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly Procedimientos _procedimientos;

        public ReservationRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // ────────────────────────────────────────────────────────────────────
        // CREAR RESERVA NIGHTLY
        // ────────────────────────────────────────────────────────────────────

        public async Task<(Guid ReservationId, string Folio)> CreateNightlyAsync(
            Guid hotelId,
            CreateReservationDto dto,
            Guid createdBy)
        {
            // Serializar habitaciones a JSON para el SP
            var roomsJson = SerializeRooms(dto.Rooms);

            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_reservation_create_nightly",
                new
                {
                    p_hotel_id = hotelId,
                    p_guest_id = dto.GuestId,
                    p_created_by = createdBy,
                    p_source = dto.Source,
                    p_status = dto.Status,
                    p_check_in_date = dto.CheckInDate.Date,
                    p_check_out_date = dto.CheckOutDate.Date,
                    p_num_nights = dto.NumNights,
                    p_total_adults = dto.TotalAdults,
                    p_total_children = dto.TotalChildren,
                    p_total_babies = dto.TotalBabies,
                    p_travel_reason = dto.TravelReason,
                    p_subtotal = dto.Subtotal,
                    p_discount_amount = dto.DiscountAmount,
                    p_discount_percent = dto.DiscountPercent,
                    p_discount_reason = dto.DiscountReason,
                    p_taxes_amount = dto.TaxesAmount,
                    p_total = dto.Total,
                    p_requires_deposit = dto.RequiresDeposit,
                    p_deposit_amount = dto.DepositAmount,
                    p_deposit_payment_method = dto.DepositPaymentMethod,
                    p_deposit_reference = dto.DepositReference,
                    p_deposit_proof_url = dto.DepositProofUrl,
                    p_deposit_due_date = dto.DepositDueDate,
                    p_guest_notes = dto.GuestNotes,
                    p_internal_notes = dto.InternalNotes,
                    p_rooms = roomsJson
                }
            );

            if (result == null)
                throw new InvalidOperationException("El stored procedure no retornó resultado");

            Guid reservationId = result.out_reservation_id is Guid g ? g : Guid.Parse(result.out_reservation_id.ToString());
            string folio = result.out_folio?.ToString() ?? string.Empty;

            return (reservationId, folio);
        }

        // ────────────────────────────────────────────────────────────────────
        // OBTENER POR ID
        // ────────────────────────────────────────────────────────────────────

        public async Task<ReservationDetailDto?> GetByIdAsync(Guid hotelId, Guid reservationId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_reservation_get_by_id",
                new { p_hotel_id = hotelId, p_reservation_id = reservationId }
            );

            if (result == null) return null;

            return MapToDetail(result);
        }

        // ────────────────────────────────────────────────────────────────────
        // LISTAR
        // ────────────────────────────────────────────────────────────────────

        public async Task<(IEnumerable<ReservationListDto> Items, long TotalCount)> GetListAsync(
            Guid hotelId,
            string? search = null,
            string? status = null,
            string? source = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var rows = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_reservation_get_list",
                new
                {
                    p_hotel_id = hotelId,
                    p_search = search,
                    p_status = status,
                    p_source = source,
                    p_date_from = dateFrom?.Date,
                    p_date_to = dateTo?.Date,
                    p_page_number = pageNumber,
                    p_page_size = pageSize
                }
            );

            var items = new List<ReservationListDto>();
            long totalCount = 0;

            foreach (var row in rows)
            {
                totalCount = row.total_count is long l ? l : (long)(row.total_count ?? 0);
                items.Add(MapToListItem(row));
            }

            return (items, totalCount);
        }

        // ────────────────────────────────────────────────────────────────────
        // CANCELAR
        // ────────────────────────────────────────────────────────────────────

        public async Task<bool> CancelAsync(
            Guid hotelId, Guid reservationId, Guid cancelledBy, string? reason = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_reservation_cancel",
                new
                {
                    p_hotel_id = hotelId,
                    p_reservation_id = reservationId,
                    p_cancelled_by = cancelledBy,
                    p_cancel_reason = reason
                }
            );
            return result;
        }

        // ────────────────────────────────────────────────────────────────────
        // ACTUALIZAR ESTADO
        // ────────────────────────────────────────────────────────────────────

        public async Task<bool> UpdateStatusAsync(
            Guid hotelId, Guid reservationId, string newStatus, Guid changedBy)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_reservation_update_status",
                new
                {
                    p_hotel_id = hotelId,
                    p_reservation_id = reservationId,
                    p_new_status = newStatus,
                    p_changed_by = changedBy
                }
            );
            return result;
        }

        // ────────────────────────────────────────────────────────────────────
        // CONFIGURACIÓN DEL FORMULARIO
        // ────────────────────────────────────────────────────────────────────

        public async Task<ReservationFormConfigDto?> GetFormConfigAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<ReservationFormConfigDto>(
                "sp_reservation_get_form_config",
                new { p_hotel_id = hotelId }
            );
            return result;
        }

        // ────────────────────────────────────────────────────────────────────
        // MAPPERS PRIVADOS
        // ────────────────────────────────────────────────────────────────────

        private static ReservationDetailDto MapToDetail(dynamic row)
        {
            var dto = new ReservationDetailDto
            {
                ReservationId = row.reservation_id,
                Folio = row.folio?.ToString() ?? "",
                Status = row.status?.ToString() ?? "",
                ReservationType = row.reservation_type?.ToString() ?? "nightly",
                Source = row.source?.ToString() ?? "",
                GuestId = row.guest_id,
                GuestFirstName = row.guest_first_name?.ToString() ?? "",
                GuestLastName = row.guest_last_name?.ToString() ?? "",
                GuestPhone = row.guest_phone?.ToString(),
                GuestEmail = row.guest_email?.ToString(),
                GuestIsVip = row.guest_is_vip ?? false,
                CheckInDate = row.check_in_date,
                CheckOutDate = row.check_out_date,
                NumNights = row.num_nights ?? 0,
                TotalAdults = row.total_adults ?? 1,
                TotalChildren = row.total_children ?? 0,
                TotalBabies = row.total_babies ?? 0,
                TravelReason = row.travel_reason?.ToString(),
                Subtotal = row.subtotal ?? 0m,
                DiscountAmount = row.discount_amount ?? 0m,
                DiscountPercent = row.discount_percent ?? 0m,
                DiscountReason = row.discount_reason?.ToString(),
                TaxesAmount = row.taxes_amount ?? 0m,
                Total = row.total ?? 0m,
                DepositAmount = row.deposit_amount ?? 0m,
                DepositPaidAt = row.deposit_paid_at,
                DepositPaymentMethod = row.deposit_payment_method?.ToString(),
                DepositReference = row.deposit_reference?.ToString(),
                DepositProofUrl = row.deposit_proof_url?.ToString(),
                DepositDueDate = row.deposit_due_date,
                DepositValidatedBy = row.deposit_validated_by,
                BalancePending = row.balance_pending ?? 0m,
                GuestNotes = row.guest_notes?.ToString(),
                InternalNotes = row.internal_notes?.ToString(),
                CreatedAt = row.created_at ?? DateTime.UtcNow,
                UpdatedAt = row.updated_at ?? DateTime.UtcNow,
                CreatedBy = row.created_by
            };

            // Deserializar habitaciones del JSON
            var roomsJson = row.rooms_json?.ToString();
            if (!string.IsNullOrEmpty(roomsJson) && roomsJson != "[]")
            {
                try
                {
                    var rooms = JsonSerializer.Deserialize<List<ReservationRoomDetailDto>>(
                        roomsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    dto.Rooms = rooms ?? new List<ReservationRoomDetailDto>();
                }
                catch { dto.Rooms = new List<ReservationRoomDetailDto>(); }
            }

            return dto;
        }

        private static ReservationListDto MapToListItem(dynamic row) => new()
        {
            ReservationId = row.reservation_id,
            Folio = row.folio?.ToString() ?? "",
            Status = row.status?.ToString() ?? "",
            Source = row.source?.ToString() ?? "",
            CheckInDate = row.check_in_date,
            CheckOutDate = row.check_out_date,
            NumNights = row.num_nights ?? 0,
            TotalAdults = row.total_adults ?? 1,
            TotalChildren = row.total_children ?? 0,
            TotalBabies = row.total_babies ?? 0,
            GuestId = row.guest_id,
            GuestFullName = row.guest_full_name?.ToString() ?? "",
            GuestPhone = row.guest_phone?.ToString(),
            GuestIsVip = row.guest_is_vip ?? false,
            RoomsSummary = row.rooms_summary?.ToString() ?? "",
            RoomCount = row.room_count is long lc ? lc : (long)(row.room_count ?? 0),
            Total = row.total ?? 0m,
            DepositAmount = row.deposit_amount ?? 0m,
            BalancePending = row.balance_pending ?? 0m,
            DepositPaidAt = row.deposit_paid_at,
            CreatedAt = row.created_at ?? DateTime.UtcNow,
            TotalCount = row.total_count is long tc ? tc : (long)(row.total_count ?? 0)
        };

        private static string SerializeRooms(List<CreateReservationRoomDto> rooms)
        {
            var roomObjects = rooms.Select(r => new
            {
                room_id = r.RoomId,
                room_type_id = r.RoomTypeId,
                num_adults = r.NumAdults,
                num_children = r.NumChildren,
                num_babies = r.NumBabies,
                has_pets = r.HasPets,
                num_pets = r.NumPets,
                pet_charge_applied = r.PetChargeApplied,
                vehicle_plate = r.VehiclePlate,
                vehicle_description = r.VehicleDescription,
                price_per_night = r.PricePerNight,
                extra_person_charge = r.ExtraPersonCharge,
                subtotal = r.Subtotal
            });

            return JsonSerializer.Serialize(roomObjects);
        }
    }
}
