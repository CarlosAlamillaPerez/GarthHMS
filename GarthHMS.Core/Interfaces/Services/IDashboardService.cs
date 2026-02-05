using GarthHMS.Core.DTOs.Dashboard;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Contrato para la lógica de negocio del Dashboard
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Obtiene el resumen de estados de habitaciones
        /// </summary>
        Task<ServiceResult<DashboardRoomStatusSummaryDto>> GetRoomStatusSummaryAsync(Guid hotelId);

        /// <summary>
        /// Obtiene la lista de habitaciones para el mapa visual
        /// </summary>
        Task<ServiceResult<IEnumerable<DashboardRoomMapDto>>> GetRoomsMapAsync(Guid hotelId);

        /// <summary>
        /// Obtiene las métricas principales del dashboard
        /// </summary>
        Task<ServiceResult<DashboardMetricsDto>> GetMetricsAsync(Guid hotelId, DateTime? date = null);

        /// <summary>
        /// Obtiene las alertas activas del dashboard
        /// </summary>
        Task<ServiceResult<IEnumerable<DashboardAlertDto>>> GetAlertsAsync(Guid hotelId);

        /// <summary>
        /// Obtiene todos los datos del dashboard en una sola llamada
        /// Útil para la carga inicial y el auto-refresh
        /// </summary>
        Task<ServiceResult<DashboardCompleteDto>> GetDashboardCompleteAsync(Guid hotelId);
    }
}