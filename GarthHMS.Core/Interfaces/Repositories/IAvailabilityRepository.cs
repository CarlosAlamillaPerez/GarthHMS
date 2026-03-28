// GarthHMS.Core/Interfaces/Repositories/IAvailabilityRepository.cs
using GarthHMS.Core.DTOs.AvailabilityEngine;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de disponibilidad y calendario
    /// </summary>
    public interface IAvailabilityRepository
    {
        // ====================================================================
        // CALENDARIO
        // ====================================================================

        /// <summary>
        /// Retorna datos de ocupación por día para el calendario mensual
        /// SP: sp_availability_get_monthly_calendar
        /// </summary>
        Task<IEnumerable<CalendarDayDto>> GetMonthlyCalendarAsync(Guid hotelId, int year, int month);

        /// <summary>
        /// Retorna resumen de ocupación para el día seleccionado
        /// SP: sp_availability_get_day_summary
        /// </summary>
        Task<DaySummaryDto?> GetDaySummaryAsync(Guid hotelId, DateTime date);

        // ====================================================================
        // LISTAS DE RESERVAS
        // ====================================================================

        /// <summary>
        /// Retorna reservas activas para una fecha (panel derecho del calendario)
        /// SP: sp_availability_get_reservations_by_date
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetReservationsByDateAsync(Guid hotelId, DateTime date);

        /// <summary>
        /// Retorna reservas con check-in hoy
        /// SP: sp_availability_get_today_reservations
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetTodayReservationsAsync(Guid hotelId);

        /// <summary>
        /// Retorna reservas de los próximos N días
        /// SP: sp_availability_get_upcoming_reservations
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetUpcomingReservationsAsync(Guid hotelId, int days = 7);

        // ====================================================================
        // VERIFICACIÓN DE DISPONIBILIDAD
        // ====================================================================

        /// <summary>
        /// Verifica si una habitación está disponible para un rango de fechas
        /// SP: sp_availability_check_room_dates
        /// Retorna TRUE si está disponible, FALSE si hay conflicto
        /// </summary>
        Task<bool> CheckRoomAvailabilityAsync(
            Guid roomId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null);

        /// <summary>
        /// Retorna habitaciones disponibles para un rango de fechas
        /// SP: sp_availability_get_available_rooms
        /// </summary>
        Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(
            Guid hotelId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? roomTypeId = null);

        /// <summary>
        /// Búsqueda global de reservas por folio, nombre de huésped o teléfono
        /// SP: sp_availability_search_reservations
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> SearchReservationsAsync(
            Guid hotelId, string query, int limit = 20);
    }
}