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
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
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
            // DEBUG TEMPORAL — quitar después
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            _logger.LogWarning(">>> ROL DEL USUARIO ACTUAL: '{Role}'", role);

            if (!IsManagerOrAdmin())
                return Content($"DEBUG — Rol detectado: '{role}' — IsManagerOrAdmin: false");

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
    }

    // ── Request helpers ──────────────────────────────────────────────────────

    public class VerifyPaymentRequest
    {
        public Guid PaymentId { get; set; }
    }
}