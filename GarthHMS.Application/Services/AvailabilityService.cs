// GarthHMS.Application/Services/AvailabilityService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.AvailabilityEngine;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio del Motor de Disponibilidad
    /// Implementa la lógica de negocio para el calendario y verificación de disponibilidad
    /// </summary>
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(
            IAvailabilityRepository availabilityRepository,
            ILogger<AvailabilityService> logger)
        {
            _availabilityRepository = availabilityRepository;
            _logger = logger;
        }

        // ====================================================================
        // CALENDARIO
        // ====================================================================

        public async Task<IEnumerable<CalendarDayDto>> GetMonthlyCalendarAsync(
            Guid hotelId, int year, int month)
        {
            if (year < 2020 || year > 2099)
                throw new ArgumentException("El año no es válido.", nameof(year));

            if (month < 1 || month > 12)
                throw new ArgumentException("El mes debe estar entre 1 y 12.", nameof(month));

            _logger.LogInformation(
                "Cargando calendario {Year}/{Month} para hotel {HotelId}", year, month, hotelId);

            return await _availabilityRepository.GetMonthlyCalendarAsync(hotelId, year, month);
        }

        public async Task<DaySummaryDto?> GetDaySummaryAsync(Guid hotelId, DateTime date)
        {
            return await _availabilityRepository.GetDaySummaryAsync(hotelId, date);
        }

        // ====================================================================
        // LISTAS DE RESERVAS
        // ====================================================================

        public async Task<IEnumerable<ReservationListItemDto>> GetReservationsByDateAsync(
            Guid hotelId, DateTime date)
        {
            _logger.LogInformation(
                "Cargando reservas para {Date} en hotel {HotelId}", date.Date, hotelId);

            return await _availabilityRepository.GetReservationsByDateAsync(hotelId, date);
        }

        public async Task<IEnumerable<ReservationListItemDto>> GetTodayReservationsAsync(Guid hotelId)
        {
            return await _availabilityRepository.GetTodayReservationsAsync(hotelId);
        }

        public async Task<IEnumerable<ReservationListItemDto>> GetUpcomingReservationsAsync(Guid hotelId)
        {
            return await _availabilityRepository.GetUpcomingReservationsAsync(hotelId, days: 7);
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
            ValidateDates(checkInDate, checkOutDate);

            return await _availabilityRepository.CheckRoomAvailabilityAsync(
                roomId, checkInDate, checkOutDate, excludeReservationId);
        }

        public async Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(AvailabilityQueryDto query, bool? requiresPets = null)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            ValidateDates(query.CheckInDate, query.CheckOutDate);

            return await _availabilityRepository.GetAvailableRoomsAsync(
                query.HotelId,
                query.CheckInDate,
                query.CheckOutDate,
                query.RoomTypeId,
                p_requires_pets = requiresPets);
        }

        // ====================================================================
        // PRIVADOS
        // ====================================================================

        private static void ValidateDates(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut)
                throw new InvalidOperationException(
                    "La fecha de check-in debe ser anterior al check-out.");

            // Permitimos hasta 2 días atrás como margen — el frontend controla la regla de 5am
            if (checkIn.Date < DateTime.UtcNow.Date.AddDays(-2))
                throw new InvalidOperationException(
                    "La fecha de check-in no puede ser en el pasado.");

            if ((checkOut.Date - checkIn.Date).TotalDays > 365)
                throw new InvalidOperationException(
                    "La estancia no puede exceder 365 días.");
        }

        // ====================================================================
        // BÚSQUEDA GLOBAL
        // ====================================================================

        public async Task<IEnumerable<ReservationListItemDto>> SearchReservationsAsync(
            Guid hotelId, string query, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                throw new ArgumentException("La búsqueda debe tener al menos 2 caracteres.");

            _logger.LogInformation(
                "Búsqueda global '{Query}' en hotel {HotelId}", query, hotelId);

            return await _availabilityRepository.SearchReservationsAsync(
                hotelId, query.Trim(), limit);
        }
    }
}