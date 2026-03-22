// GarthHMS.Application/Services/ReservationService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Reservation;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<ReservationService> _logger;

        private static readonly string[] ValidStatuses = { "draft", "pending", "confirmed" };
        private static readonly string[] ValidSources = { "direct", "phone", "whatsapp", "walk_in", "ota_booking", "ota_airbnb", "ota_expedia" };
        private static readonly string[] ValidMethods = { "cash", "transfer", "card" };

        public ReservationService(
            IReservationRepository reservationRepository,
            ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        // ────────────────────────────────────────────────────────────────────
        // CREAR RESERVA NIGHTLY
        // ────────────────────────────────────────────────────────────────────

        public async Task<ServiceResult<(Guid ReservationId, string Folio)>> CreateNightlyAsync(
            Guid hotelId,
            CreateReservationDto dto,
            Guid createdBy)
        {
            try
            {
                if (hotelId == Guid.Empty)
                    return ServiceResult<(Guid, string)>.Failure("Hotel no identificado");

                if (dto.GuestId == Guid.Empty)
                    return ServiceResult<(Guid, string)>.Failure("Selecciona un huésped para la reserva");

                if (!Array.Exists(ValidStatuses, s => s == dto.Status))
                    return ServiceResult<(Guid, string)>.Failure("Estado de reserva inválido");

                if (!Array.Exists(ValidSources, s => s == dto.Source))
                    return ServiceResult<(Guid, string)>.Failure("Canal de reserva inválido");

                if (dto.CheckInDate.Date >= dto.CheckOutDate.Date)
                    return ServiceResult<(Guid, string)>.Failure("La fecha de check-out debe ser posterior al check-in");

                if (dto.CheckInDate.Date < DateTime.Today && dto.Status != "draft")
                    return ServiceResult<(Guid, string)>.Failure("La fecha de check-in no puede ser en el pasado");

                var expectedNights = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
                if (dto.NumNights != expectedNights)
                    dto.NumNights = expectedNights;

                if (dto.Rooms == null || dto.Rooms.Count == 0)
                    return ServiceResult<(Guid, string)>.Failure("La reserva debe tener al menos una habitación");

                foreach (var room in dto.Rooms)
                {
                    if (room.RoomId == Guid.Empty)
                        return ServiceResult<(Guid, string)>.Failure("Una de las habitaciones no tiene ID válido");

                    if (room.PricePerNight <= 0)
                        return ServiceResult<(Guid, string)>.Failure($"La habitación {room.RoomNumber ?? room.RoomId.ToString()} debe tener precio mayor a 0");
                }

                if (dto.Total <= 0)
                    return ServiceResult<(Guid, string)>.Failure("El total de la reserva debe ser mayor a 0");

                if (dto.RequiresDeposit && dto.DepositAmount > dto.Total)
                    return ServiceResult<(Guid, string)>.Failure("El anticipo no puede ser mayor al total de la reserva");

                if (dto.RequiresDeposit && dto.DepositAmount > 0 &&
                    dto.DepositPaymentMethod != null &&
                    !Array.Exists(ValidMethods, m => m == dto.DepositPaymentMethod))
                    return ServiceResult<(Guid, string)>.Failure("Método de pago inválido");

                var (reservationId, folio) = await _reservationRepository.CreateNightlyAsync(
                    hotelId, dto, createdBy);

                _logger.LogInformation(
                    "Reserva creada: {Folio} | Hotel: {HotelId} | Huésped: {GuestId} | Estado: {Status}",
                    folio, hotelId, dto.GuestId, dto.Status);

                return ServiceResult<(Guid, string)>.Success((reservationId, folio));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva | Hotel: {HotelId} | Huésped: {GuestId}", hotelId, dto.GuestId);
                return ServiceResult<(Guid, string)>.Failure("Ocurrió un error al crear la reserva. Intenta de nuevo.");
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // OBTENER POR ID
        // ────────────────────────────────────────────────────────────────────

        public async Task<ReservationDetailDto?> GetByIdAsync(Guid hotelId, Guid reservationId)
        {
            try
            {
                return await _reservationRepository.GetByIdAsync(hotelId, reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva {ReservationId}", reservationId);
                return null;
            }
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
            try
            {
                return await _reservationRepository.GetListAsync(
                    hotelId, search, status, source, dateFrom, dateTo, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de reservas | Hotel: {HotelId}", hotelId);
                return (Array.Empty<ReservationListDto>(), 0);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // CANCELAR
        // ────────────────────────────────────────────────────────────────────

        public async Task<ServiceResult<bool>> CancelAsync(
            Guid hotelId, Guid reservationId, Guid cancelledBy, string? reason = null)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(hotelId, reservationId);
                if (reservation == null)
                    return ServiceResult<bool>.Failure("Reserva no encontrada");

                if (reservation.Status is "checked_out" or "cancelled")
                    return ServiceResult<bool>.Failure($"No se puede cancelar una reserva en estado '{reservation.StatusLabel}'");

                await _reservationRepository.CancelAsync(hotelId, reservationId, cancelledBy, reason);
                _logger.LogInformation("Reserva cancelada: {ReservationId} por {UserId}", reservationId, cancelledBy);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva {ReservationId}", reservationId);
                return ServiceResult<bool>.Failure("Error al cancelar la reserva");
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // CONFIGURACIÓN DEL FORMULARIO
        // ────────────────────────────────────────────────────────────────────

        public async Task<ReservationFormConfigDto?> GetFormConfigAsync(Guid hotelId)
        {
            try
            {
                return await _reservationRepository.GetFormConfigAsync(hotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de formulario | Hotel: {HotelId}", hotelId);
                return null;
            }
        }
    }
}