using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.HotelSettings;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("HotelSettings")]  // ← ESTA ES LA LÍNEA CRÍTICA
    public class HotelSettingsController : Controller
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ILogger<HotelSettingsController> _logger;

        public HotelSettingsController(
            IHotelSettingsService hotelSettingsService,
            ILogger<HotelSettingsController> logger)
        {
            _hotelSettingsService = hotelSettingsService;
            _logger = logger;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private Guid GetCurrentHotelId()
        {
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            return Guid.TryParse(hotelIdClaim, out var hotelId) ? hotelId : Guid.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // ====================================================================
        // VISTA PRINCIPAL
        // ====================================================================

        // GET: /HotelSettings
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ====================================================================
        // API ENDPOINTS
        // ====================================================================

        // GET: /HotelSettings/GetSettings
        [HttpGet("GetSettings")]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var result = await _hotelSettingsService.GetSettingsAsync(hotelId);

                return Json(new
                {
                    success = result.IsSuccess,
                    data = result.Data,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración del hotel");
                return Json(new { success = false, message = "Error al cargar la configuración" });
            }
        }

        // POST: /HotelSettings/Update
        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateHotelSettingsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _hotelSettingsService.UpdateSettingsAsync(hotelId, dto, userId);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración");
                return Json(new { success = false, message = "Error al actualizar la configuración" });
            }
        }
    }
}