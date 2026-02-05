using GarthHMS.Core.DTOs.Dashboard;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de datos del Dashboard
    /// Implementa IDashboardRepository usando Stored Procedures
    /// </summary>
    public class DashboardRepository : IDashboardRepository
    {
        private readonly Procedimientos _procedimientos;

        public DashboardRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // ====================================================================
        // IMPLEMENTACIÓN DE MÉTODOS
        // ====================================================================

        public async Task<DashboardRoomStatusSummaryDto?> GetRoomStatusSummaryAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_dashboard_get_room_status_summary",
                new { p_hotel_id = hotelId }
            );

            return result != null ? MapToRoomStatusSummary(result) : null;
        }

        public async Task<IEnumerable<DashboardRoomMapDto>> GetRoomsMapAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_dashboard_get_rooms_map",
                new { p_hotel_id = hotelId }
            );

            return results.Select(MapToRoomMap);
        }

        public async Task<DashboardMetricsDto?> GetMetricsAsync(Guid hotelId, DateTime date)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_dashboard_get_metrics",
                new { p_hotel_id = hotelId, p_date = date }
            );

            return result != null ? MapToMetrics(result) : null;
        }

        public async Task<IEnumerable<DashboardAlertDto>> GetAlertsAsync(Guid hotelId)
        {
            var results = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_dashboard_get_alerts",
                new { p_hotel_id = hotelId }
            );

            return results.Select(MapToAlert);
        }

        // ====================================================================
        // MÉTODOS DE MAPEO (dynamic → DTO)
        // ====================================================================

        private static DashboardRoomStatusSummaryDto MapToRoomStatusSummary(dynamic result)
        {
            return new DashboardRoomStatusSummaryDto
            {
                TotalRooms = (int)(result.total_rooms ?? 0),
                AvailableRooms = (int)(result.available_rooms ?? 0),
                OccupiedRooms = (int)(result.occupied_rooms ?? 0),
                DirtyRooms = (int)(result.dirty_rooms ?? 0),
                CleaningRooms = (int)(result.cleaning_rooms ?? 0),
                MaintenanceRooms = (int)(result.maintenance_rooms ?? 0),
                ReservedRooms = (int)(result.reserved_rooms ?? 0),
                OccupancyPercent = (decimal)(result.occupancy_percent ?? 0m)
            };
        }

        private static DashboardRoomMapDto MapToRoomMap(dynamic result)
        {
            return new DashboardRoomMapDto
            {
                RoomId = result.room_id,
                RoomNumber = result.room_number ?? string.Empty,
                Floor = (int)(result.floor ?? 1),
                Status = result.status?.ToString() ?? "available",
                StatusText = result.status_text ?? "Disponible",
                RoomTypeId = result.room_type_id,
                RoomTypeName = result.room_type_name ?? string.Empty,
                RoomTypeCode = result.room_type_code ?? string.Empty,
                CurrentStayId = result.current_stay_id
            };
        }

        private static DashboardMetricsDto MapToMetrics(dynamic result)
        {
            return new DashboardMetricsDto
            {
                TotalRooms = (int)(result.total_rooms ?? 0),
                OccupiedRooms = (int)(result.occupied_rooms ?? 0),
                AvailableRooms = (int)(result.available_rooms ?? 0),
                DirtyRooms = (int)(result.dirty_rooms ?? 0),
                OccupancyPercent = (decimal)(result.occupancy_percent ?? 0m),
                ReservationsToday = (int)(result.reservations_today ?? 0),
                CheckoutsToday = (int)(result.checkouts_today ?? 0),
                CheckinsToday = (int)(result.checkins_today ?? 0),
                GuestsActive = (int)(result.guests_active ?? 0),
                PendingDeposits = (int)(result.pending_deposits ?? 0)
            };
        }

        private static DashboardAlertDto MapToAlert(dynamic result)
        {
            return new DashboardAlertDto
            {
                AlertId = result.alert_id ?? string.Empty,
                AlertType = result.alert_type ?? string.Empty,
                AlertSeverity = result.alert_severity ?? "low",
                AlertTitle = result.alert_title ?? string.Empty,
                AlertMessage = result.alert_message ?? string.Empty,
                RelatedEntityId = result.related_entity_id,
                RelatedEntityType = result.related_entity_type ?? string.Empty,
                CreatedAt = result.created_at ?? DateTime.Now
            };
        }
    }
}