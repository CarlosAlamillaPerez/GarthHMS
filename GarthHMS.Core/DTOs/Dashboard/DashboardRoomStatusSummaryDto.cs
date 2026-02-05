namespace GarthHMS.Core.DTOs.Dashboard
{
    /// <summary>
    /// DTO para el resumen de estados de habitaciones del dashboard
    /// Mapea directamente con sp_dashboard_get_room_status_summary
    /// </summary>
    public class DashboardRoomStatusSummaryDto
    {
        /// <summary>
        /// Total de habitaciones activas del hotel
        /// </summary>
        public int TotalRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'available'
        /// </summary>
        public int AvailableRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'occupied'
        /// </summary>
        public int OccupiedRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'dirty'
        /// </summary>
        public int DirtyRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'cleaning'
        /// </summary>
        public int CleaningRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'maintenance'
        /// </summary>
        public int MaintenanceRooms { get; set; }

        /// <summary>
        /// Habitaciones con status = 'reserved'
        /// </summary>
        public int ReservedRooms { get; set; }

        /// <summary>
        /// Porcentaje de ocupación: (OccupiedRooms / TotalRooms) * 100
        /// </summary>
        public decimal OccupancyPercent { get; set; }
    }
}