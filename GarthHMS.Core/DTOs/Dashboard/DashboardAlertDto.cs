namespace GarthHMS.Core.DTOs.Dashboard
{
    /// <summary>
    /// DTO para las alertas del dashboard
    /// Mapea directamente con sp_dashboard_get_alerts
    /// </summary>
    public class DashboardAlertDto
    {
        /// <summary>
        /// Identificador único de la alerta
        /// </summary>
        public string AlertId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de alerta: dirty_room, maintenance, checkout_overdue, pending_deposit
        /// </summary>
        public string AlertType { get; set; } = string.Empty;

        /// <summary>
        /// Severidad: low, medium, high, critical
        /// </summary>
        public string AlertSeverity { get; set; } = string.Empty;

        /// <summary>
        /// Título corto de la alerta
        /// </summary>
        public string AlertTitle { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje descriptivo
        /// </summary>
        public string AlertMessage { get; set; } = string.Empty;

        /// <summary>
        /// ID de la entidad relacionada (room_id, stay_id, reservation_id)
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Tipo de entidad: room, stay, reservation
        /// </summary>
        public string RelatedEntityType { get; set; } = string.Empty;

        /// <summary>
        /// Fecha/hora de creación de la alerta
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // ====================================================================
        // PROPIEDADES CALCULADAS
        // ====================================================================

        /// <summary>
        /// Icono emoji basado en el tipo de alerta
        /// </summary>
        public string AlertIcon => AlertType switch
        {
            "dirty_room" => "🧹",
            "maintenance" => "🔧",
            "checkout_overdue" => "⏰",
            "pending_deposit" => "💰",
            "checkin_today" => "🚶",
            _ => "⚠️"
        };

        /// <summary>
        /// Clase CSS basada en la severidad
        /// </summary>
        public string SeverityCssClass => AlertSeverity switch
        {
            "critical" => "danger",
            "high" => "warning",
            "medium" => "info",
            "low" => "secondary",
            _ => "secondary"
        };

        /// <summary>
        /// URL de navegación basada en el tipo de entidad
        /// </summary>
        public string NavigationUrl => RelatedEntityType switch
        {
            "room" => $"/Rooms/Details/{RelatedEntityId}",
            "stay" => $"/Stays/Details/{RelatedEntityId}",
            "reservation" => $"/Reservations/Details/{RelatedEntityId}",
            _ => "#"
        };
    }
}