namespace GarthHMS.Core.DTOs.Dashboard
{
    /// <summary>
    /// DTO para las métricas principales del dashboard
    /// Mapea directamente con sp_dashboard_get_metrics
    /// </summary>
    public class DashboardMetricsDto
    {
        // ====================================================================
        // MÉTRICAS DE HABITACIONES (funcionan ahora)
        // ====================================================================

        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int DirtyRooms { get; set; }
        public decimal OccupancyPercent { get; set; }

        // ====================================================================
        // MÉTRICAS DE RESERVAS (retornan 0 hasta FASE 3)
        // ====================================================================

        /// <summary>
        /// Reservas con llegada programada para hoy
        /// NOTA: Retorna 0 hasta implementar módulo de Reservas
        /// </summary>
        public int ReservationsToday { get; set; }

        /// <summary>
        /// Check-outs programados para hoy
        /// NOTA: Retorna 0 hasta implementar módulo de Stays
        /// </summary>
        public int CheckoutsToday { get; set; }

        /// <summary>
        /// Check-ins programados para hoy
        /// NOTA: Retorna 0 hasta implementar módulo de Reservas
        /// </summary>
        public int CheckinsToday { get; set; }

        /// <summary>
        /// Huéspedes actualmente hospedados
        /// NOTA: Retorna 0 hasta implementar módulo de Stays
        /// </summary>
        public int GuestsActive { get; set; }

        /// <summary>
        /// Reservas pendientes de anticipo
        /// NOTA: Retorna 0 hasta implementar módulo de Reservas
        /// </summary>
        public int PendingDeposits { get; set; }

        // ====================================================================
        // PROPIEDADES CALCULADAS
        // ====================================================================

        /// <summary>
        /// Texto formateado para la ocupación (ej: "75%")
        /// </summary>
        public string OccupancyPercentText => $"{OccupancyPercent:F0}%";

        /// <summary>
        /// Clase CSS para el color de ocupación
        /// </summary>
        public string OccupancyCssClass => OccupancyPercent switch
        {
            >= 90 => "danger",
            >= 70 => "warning",
            >= 50 => "success",
            _ => "info"
        };
    }
}