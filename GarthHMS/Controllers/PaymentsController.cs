// GarthHMS.Web/Controllers/PaymentsController.cs
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Payments")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IHotelSettingsService hotelSettingsService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _hotelSettingsService = hotelSettingsService;
            _logger = logger;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private Guid GetCurrentHotelId()
        {
            var claim = User.FindFirst("HotelId")?.Value;
            return Guid.Parse(claim ?? Guid.Empty.ToString());
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(claim ?? Guid.Empty.ToString());
        }

        private bool IsManagerOrAdmin()
        {
            // SuperAdmin del sistema siempre tiene acceso total
            if (User.FindFirst(ClaimTypes.Role)?.Value == "SuperAdmin") return true;
            // Cualquier rol marcado como is_manager_role en la BD
            return User.FindFirst("IsManagerRole")?.Value == "True";
        }

        // ====================================================================
        // VISTA PRINCIPAL
        // GET /Payments/PendingVerification
        // ====================================================================

        [HttpGet("PendingVerification")]
        public IActionResult PendingVerification()
        {
            return View();
        }

        // ====================================================================
        // API — Obtener pagos pendientes de verificación
        // GET /Payments/GetPendingVerifications
        // ====================================================================

        [HttpGet("GetPendingVerifications")]
        public async Task<IActionResult> GetPendingVerifications()
        {
            try
            {
                if (!IsManagerOrAdmin())
                    return Json(new { success = false, message = "Acceso no autorizado" });

                var hotelId = GetCurrentHotelId();
                var payments = await _paymentService.GetPendingVerificationAsync(hotelId);

                return Json(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos pendientes de verificación");
                return Json(new { success = false, message = "Error al obtener los pagos" });
            }
        }

        // ====================================================================
        // API — Verificar un pago
        // POST /Payments/Verify
        // ====================================================================

        [HttpPost("Verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request)
        {
            try
            {
                if (!IsManagerOrAdmin())
                    return Json(new { success = false, message = "Solo Gerentes y Administradores pueden verificar pagos" });

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _paymentService.VerifyPaymentAsync(
                    hotelId,
                    request.PaymentId,
                    userId,
                    isManagerOrAdmin: true);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar pago {PaymentId}", request?.PaymentId);
                return Json(new { success = false, message = "Error al verificar el pago" });
            }
        }

        // ====================================================================
        // API — Obtener pagos verificados (tab Verificados)
        // GET /Payments/GetVerified
        // ====================================================================

        [HttpGet("GetVerified")]
        public async Task<IActionResult> GetVerified()
        {
            try
            {
                if (!IsManagerOrAdmin())
                    return Json(new { success = false, message = "Acceso no autorizado" });

                var hotelId = GetCurrentHotelId();
                var payments = await _paymentService.GetVerifiedAsync(hotelId);
                return Json(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos verificados");
                return Json(new { success = false, message = "Error al obtener los pagos" });
            }
        }

        // ====================================================================
        // API — Verificación masiva por método
        // POST /Payments/VerifyBulk
        // ====================================================================

        [HttpPost("VerifyBulk")]
        public async Task<IActionResult> VerifyBulk([FromBody] VerifyBulkRequest request)
        {
            try
            {
                if (!IsManagerOrAdmin())
                    return Json(new { success = false, message = "Acceso no autorizado" });

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _paymentService.VerifyBulkAsync(
                    hotelId, request.Method, userId, isManagerOrAdmin: true);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new
                {
                    success = true,
                    message = result.Message,
                    verifiedCount = result.Data.VerifiedCount,
                    totalAmount = result.Data.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en verificación masiva");
                return Json(new { success = false, message = "Error al verificar los pagos" });
            }
        }

        // ====================================================================
        // API — Guardar configuración de auto-verificación
        // POST /Payments/UpdateAutoVerify
        // ====================================================================

        [HttpPost("UpdateAutoVerify")]
        public async Task<IActionResult> UpdateAutoVerify([FromBody] UpdateAutoVerifyRequest request)
        {
            try
            {
                if (!IsManagerOrAdmin())
                    return Json(new { success = false, message = "Acceso no autorizado" });

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _hotelSettingsService.UpdateAutoVerifyAsync(
                    hotelId, request.AutoVerifyCard, request.AutoVerifyTransfer, userId);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar auto-verificación");
                return Json(new { success = false, message = "Error al guardar la configuración" });
            }
        }

        // ====================================================================
        // API — Obtener configuración actual de auto-verificación
        // GET /Payments/GetAutoVerifySettings
        // ====================================================================

        [HttpGet("GetAutoVerifySettings")]
        public async Task<IActionResult> GetAutoVerifySettings()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var settings = await _hotelSettingsService.GetSettingsAsync(hotelId);

                if (!settings.IsSuccess)
                    return Json(new { success = false, message = settings.Message });

                return Json(new
                {
                    success = true,
                    autoVerifyCard = settings.Data?.AutoVerifyCard ?? false,
                    autoVerifyTransfer = settings.Data?.AutoVerifyTransfer ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de auto-verificación");
                return Json(new { success = false, message = "Error al cargar la configuración" });
            }
        }
    }

    // ── Request helpers ──────────────────────────────────────────────────────

    public class VerifyPaymentRequest
    {
        public Guid PaymentId { get; set; }
    }

    public class VerifyBulkRequest
    {
        public string Method { get; set; } = "";
    }

    public class UpdateAutoVerifyRequest
    {
        public bool AutoVerifyCard { get; set; }
        public bool AutoVerifyTransfer { get; set; }
    }
}