namespace GarthHMS.Core.DTOs.Dashboard
{
    /// <summary>
    /// DTO que agrupa todos los datos del dashboard
    /// Útil para cargar todo en una sola llamada API
    /// </summary>
    public class DashboardCompleteDto
    {
        /// <summary>
        /// Métricas principales (KPIs)
        /// </summary>
        public DashboardMetricsDto Metrics { get; set; } = new();

        /// <summary>
        /// Resumen de estados de habitaciones
        /// </summary>
        public DashboardRoomStatusSummaryDto RoomStatusSummary { get; set; } = new();

        /// <summary>
        /// Lista de habitaciones para el mapa
        /// </summary>
        public List<DashboardRoomMapDto> RoomsMap { get; set; } = new();

        /// <summary>
        /// Lista de alertas activas
        /// </summary>
        public List<DashboardAlertDto> Alerts { get; set; } = new();

        /// <summary>
        /// Fecha/hora de la última actualización
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}