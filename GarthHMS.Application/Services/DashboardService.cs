using GarthHMS.Core.DTOs.Dashboard;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio de lógica de negocio para el Dashboard
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        // ====================================================================
        // IMPLEMENTACIÓN DE MÉTODOS
        // ====================================================================

        public async Task<ServiceResult<DashboardRoomStatusSummaryDto>> GetRoomStatusSummaryAsync(Guid hotelId)
        {
            try
            {
                var summary = await _dashboardRepository.GetRoomStatusSummaryAsync(hotelId);

                if (summary == null)
                {
                    // Si no hay datos, retornar un objeto vacío (no error)
                    return ServiceResult<DashboardRoomStatusSummaryDto>.Success(
                        new DashboardRoomStatusSummaryDto()
                    );
                }

                return ServiceResult<DashboardRoomStatusSummaryDto>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de estados de habitaciones para hotel {HotelId}", hotelId);
                return ServiceResult<DashboardRoomStatusSummaryDto>.Failure(
                    $"Error al obtener resumen de habitaciones: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<IEnumerable<DashboardRoomMapDto>>> GetRoomsMapAsync(Guid hotelId)
        {
            try
            {
                var rooms = await _dashboardRepository.GetRoomsMapAsync(hotelId);
                return ServiceResult<IEnumerable<DashboardRoomMapDto>>.Success(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mapa de habitaciones para hotel {HotelId}", hotelId);
                return ServiceResult<IEnumerable<DashboardRoomMapDto>>.Failure(
                    $"Error al obtener mapa de habitaciones: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<DashboardMetricsDto>> GetMetricsAsync(Guid hotelId, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                var metrics = await _dashboardRepository.GetMetricsAsync(hotelId, targetDate);

                if (metrics == null)
                {
                    return ServiceResult<DashboardMetricsDto>.Success(new DashboardMetricsDto());
                }

                return ServiceResult<DashboardMetricsDto>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas del dashboard para hotel {HotelId}", hotelId);
                return ServiceResult<DashboardMetricsDto>.Failure(
                    $"Error al obtener métricas: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<IEnumerable<DashboardAlertDto>>> GetAlertsAsync(Guid hotelId)
        {
            try
            {
                var alerts = await _dashboardRepository.GetAlertsAsync(hotelId);

                // Ordenar alertas por severidad (critical primero)
                var orderedAlerts = alerts
                    .OrderByDescending(a => GetSeverityOrder(a.AlertSeverity))
                    .ThenByDescending(a => a.CreatedAt);

                return ServiceResult<IEnumerable<DashboardAlertDto>>.Success(orderedAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas del dashboard para hotel {HotelId}", hotelId);
                return ServiceResult<IEnumerable<DashboardAlertDto>>.Failure(
                    $"Error al obtener alertas: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<DashboardCompleteDto>> GetDashboardCompleteAsync(Guid hotelId)
        {
            try
            {
                // Ejecutar todas las consultas en paralelo para mejor rendimiento
                var metricsTask = _dashboardRepository.GetMetricsAsync(hotelId, DateTime.Today);
                var summaryTask = _dashboardRepository.GetRoomStatusSummaryAsync(hotelId);
                var roomsMapTask = _dashboardRepository.GetRoomsMapAsync(hotelId);
                var alertsTask = _dashboardRepository.GetAlertsAsync(hotelId);

                await Task.WhenAll(metricsTask, summaryTask, roomsMapTask, alertsTask);

                var complete = new DashboardCompleteDto
                {
                    Metrics = await metricsTask ?? new DashboardMetricsDto(),
                    RoomStatusSummary = await summaryTask ?? new DashboardRoomStatusSummaryDto(),
                    RoomsMap = (await roomsMapTask).ToList(),
                    Alerts = (await alertsTask)
                        .OrderByDescending(a => GetSeverityOrder(a.AlertSeverity))
                        .ThenByDescending(a => a.CreatedAt)
                        .ToList(),
                    LastUpdated = DateTime.Now
                };

                return ServiceResult<DashboardCompleteDto>.Success(complete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard completo para hotel {HotelId}", hotelId);
                return ServiceResult<DashboardCompleteDto>.Failure(
                    $"Error al obtener datos del dashboard: {ex.Message}"
                );
            }
        }

        // ====================================================================
        // MÉTODOS AUXILIARES
        // ====================================================================

        /// <summary>
        /// Obtiene el orden numérico de severidad para ordenamiento
        /// </summary>
        private static int GetSeverityOrder(string severity)
        {
            return severity switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }
    }
}