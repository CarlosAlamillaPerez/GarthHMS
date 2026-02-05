using GarthHMS.Core.DTOs.Dashboard;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones de datos del Dashboard
    /// NOTA: Este repositorio es de SOLO LECTURA (no hay operaciones de escritura)
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Obtiene el resumen de estados de habitaciones
        /// SP: sp_dashboard_get_room_status_summary
        /// </summary>
        Task<DashboardRoomStatusSummaryDto?> GetRoomStatusSummaryAsync(Guid hotelId);

        /// <summary>
        /// Obtiene la lista de habitaciones para el mapa visual
        /// SP: sp_dashboard_get_rooms_map
        /// </summary>
        Task<IEnumerable<DashboardRoomMapDto>> GetRoomsMapAsync(Guid hotelId);

        /// <summary>
        /// Obtiene las métricas principales del dashboard
        /// SP: sp_dashboard_get_metrics
        /// </summary>
        Task<DashboardMetricsDto?> GetMetricsAsync(Guid hotelId, DateTime date);

        /// <summary>
        /// Obtiene las alertas activas del dashboard
        /// SP: sp_dashboard_get_alerts
        /// </summary>
        Task<IEnumerable<DashboardAlertDto>> GetAlertsAsync(Guid hotelId);
    }
}