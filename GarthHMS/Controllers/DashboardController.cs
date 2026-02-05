using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.Interfaces.Services;
using System.Security.Claims;

namespace GarthHMS.Web.Controllers
{
    /// <summary>
    /// Controlador para el Dashboard principal del sistema
    /// FASE 2 - Solo lectura, sin operaciones CRUD
    /// </summary>
    [Authorize]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        // ====================================================================
        // VISTA PRINCIPAL
        // ====================================================================

        /// <summary>
        /// Vista principal del dashboard
        /// Carga datos iniciales del hotel del usuario logueado
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var hotelId = GetHotelId();

                // Cargar datos iniciales para renderizado server-side
                var result = await _dashboardService.GetDashboardCompleteAsync(hotelId);

                if (!result.IsSuccess || result.Data == null)
                {
                    _logger.LogWarning("Error al cargar dashboard inicial: {Message}", result.Message);
                    // Aún así mostramos la vista, el JS cargará los datos
                    return View();
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar vista del dashboard");
                return View(); // Vista vacía, el JS intentará cargar
            }
        }

        // ====================================================================
        // API ENDPOINTS (JSON para AJAX)
        // ====================================================================

        /// <summary>
        /// Obtiene todos los datos del dashboard en una sola llamada
        /// Usado para el auto-refresh cada 30 segundos
        /// </summary>
        [HttpGet("GetDashboardComplete")]
        public async Task<IActionResult> GetDashboardComplete()
        {
            try
            {
                var hotelId = GetHotelId();
                var result = await _dashboardService.GetDashboardCompleteAsync(hotelId);

                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard completo");
                return Json(new { success = false, message = "Error al cargar los datos del dashboard" });
            }
        }

        /// <summary>
        /// Obtiene solo las métricas (KPIs)
        /// Endpoint alternativo si solo se necesitan los números
        /// </summary>
        [HttpGet("GetMetrics")]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                var hotelId = GetHotelId();
                var result = await _dashboardService.GetMetricsAsync(hotelId);

                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas");
                return Json(new { success = false, message = "Error al cargar las métricas" });
            }
        }

        /// <summary>
        /// Obtiene solo el mapa de habitaciones
        /// </summary>
        [HttpGet("GetRoomsMap")]
        public async Task<IActionResult> GetRoomsMap()
        {
            try
            {
                var hotelId = GetHotelId();
                var result = await _dashboardService.GetRoomsMapAsync(hotelId);

                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mapa de habitaciones");
                return Json(new { success = false, message = "Error al cargar el mapa de habitaciones" });
            }
        }

        /// <summary>
        /// Obtiene solo las alertas activas
        /// </summary>
        [HttpGet("GetAlerts")]
        public async Task<IActionResult> GetAlerts()
        {
            try
            {
                var hotelId = GetHotelId();
                var result = await _dashboardService.GetAlertsAsync(hotelId);

                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return Json(new { success = false, message = "Error al cargar las alertas" });
            }
        }

        /// <summary>
        /// Obtiene el resumen de estados de habitaciones
        /// </summary>
        [HttpGet("GetRoomStatusSummary")]
        public async Task<IActionResult> GetRoomStatusSummary()
        {
            try
            {
                var hotelId = GetHotelId();
                var result = await _dashboardService.GetRoomStatusSummaryAsync(hotelId);

                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de estados");
                return Json(new { success = false, message = "Error al cargar el resumen" });
            }
        }

        // ====================================================================
        // MÉTODOS AUXILIARES
        // ====================================================================

        /// <summary>
        /// Obtiene el HotelId del usuario logueado desde los Claims
        /// </summary>
        private Guid GetHotelId()
        {
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;

            if (string.IsNullOrEmpty(hotelIdClaim))
            {
                _logger.LogError("HotelId no encontrado en los claims del usuario {UserId}", User.Identity?.Name);
                throw new InvalidOperationException("No se pudo identificar el hotel del usuario");
            }

            if (!Guid.TryParse(hotelIdClaim, out var hotelId))
            {
                _logger.LogError("HotelId inválido en claims: {HotelIdClaim}", hotelIdClaim);
                throw new InvalidOperationException("ID de hotel inválido");
            }

            return hotelId;
        }

        /// <summary>
        /// Obtiene el UserId del usuario logueado desde los Claims
        /// (Para uso futuro si se necesita rastrear quién consulta el dashboard)
        /// </summary>
        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}