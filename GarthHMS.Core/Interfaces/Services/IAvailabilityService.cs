// GarthHMS.Core/Interfaces/Services/IAvailabilityService.cs
using GarthHMS.Core.DTOs.AvailabilityEngine;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para la lógica de negocio del Motor de Disponibilidad
    /// </summary>
    public interface IAvailabilityService
    {
        // ====================================================================
        // CALENDARIO
        // ====================================================================

        /// <summary>
        /// Obtiene los datos de ocupación del mes para el calendario
        /// </summary>
        Task<IEnumerable<CalendarDayDto>> GetMonthlyCalendarAsync(Guid hotelId, int year, int month);

        /// <summary>
        /// Obtiene el resumen de ocupación para una fecha específica
        /// </summary>
        Task<DaySummaryDto?> GetDaySummaryAsync(Guid hotelId, DateTime date);

        // ====================================================================
        // LISTAS DE RESERVAS
        // ====================================================================

        /// <summary>
        /// Obtiene reservas activas para una fecha (panel derecho del calendario)
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetReservationsByDateAsync(Guid hotelId, DateTime date);

        /// <summary>
        /// Obtiene reservas con check-in hoy
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetTodayReservationsAsync(Guid hotelId);

        /// <summary>
        /// Obtiene reservas de los próximos 7 días
        /// </summary>
        Task<IEnumerable<ReservationListItemDto>> GetUpcomingReservationsAsync(Guid hotelId);

        // ====================================================================
        // VERIFICACIÓN DE DISPONIBILIDAD
        // ====================================================================

        /// <summary>
        /// Verifica si una habitación está disponible para las fechas dadas
        /// TRUE = disponible, FALSE = conflicto
        /// </summary>
        Task<bool> CheckRoomAvailabilityAsync(
            Guid roomId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null);

        /// <summary>
        /// Retorna habitaciones disponibles para un rango de fechas
        /// Lanza InvalidOperationException si las fechas son inválidas
        /// </summary>
        Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(AvailabilityQueryDto query);
    }
}