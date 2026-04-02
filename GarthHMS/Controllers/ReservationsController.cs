// GarthHMS.Web/Controllers/ReservationsController.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Reservation;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Reservations")]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
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

        private string GetOperationMode()
        {
            return User.FindFirst("OperationMode")?.Value ?? "hotel";
        }

        // ====================================================================
        // VISTA PRINCIPAL — redirige al calendario de disponibilidad
        // ====================================================================

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Availability");
        }

        // ====================================================================
        // PÁGINA CREAR RESERVA (Full Page — no modal)
        // ====================================================================

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var operationMode = GetOperationMode();
                var config = await _reservationService.GetFormConfigAsync(hotelId);

                ViewBag.OperationMode = operationMode;
                ViewBag.Config = config ?? new ReservationFormConfigDto();

                return View("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir formulario de reserva");
                TempData["ErrorMessage"] = "Error al cargar el formulario de reserva";
                return RedirectToAction("Index", "Availability");
            }
        }

        // ====================================================================
        // PÁGINA EDITAR RESERVA (Full Page — reutiliza vista Create)
        // ====================================================================

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var operationMode = GetOperationMode();
                var config = await _reservationService.GetFormConfigAsync(hotelId);
                var reservation = await _reservationService.GetByIdAsync(hotelId, id);

                if (reservation == null)
                    return NotFound();

                // Solo se pueden editar estos estados
                if (reservation.Status is "checked_in" or "checked_out" or "cancelled" or "no_show")
                {
                    TempData["ErrorMessage"] = $"No se puede editar una reserva con estado '{reservation.StatusLabel}'";
                    return RedirectToAction("Index", "Availability");
                }

                ViewBag.IsEdit = true;
                ViewBag.OperationMode = operationMode;
                ViewBag.Config = config ?? new ReservationFormConfigDto();
                ViewBag.Reservation = reservation;

                return View("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar edición de reserva {ReservationId}", id);
                return RedirectToAction("Index", "Availability");
            }
        }

        // ====================================================================
        // API — ACTUALIZAR RESERVA
        // ====================================================================

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateReservationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Datos inválidos" });

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();
                var result = await _reservationService.UpdateNightlyAsync(hotelId, dto, userId);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva");
                return Json(new { success = false, message = "Error al actualizar la reserva" });
            }
        }

        // ====================================================================
        // API — CREAR RESERVA (POST JSON)
        // ====================================================================

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _reservationService.CreateNightlyAsync(hotelId, dto, userId);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new
                {
                    success = true,
                    message = dto.Status == "draft"
                                        ? "Borrador guardado correctamente"
                                        : "Reserva creada exitosamente",
                    data = new
                    {
                        reservationId = result.Data.ReservationId,
                        folio = result.Data.Folio,
                        status = dto.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                return Json(new { success = false, message = "Error al crear la reserva" });
            }
        }

        // ====================================================================
        // API — LISTAR RESERVAS (GET JSON para Bootstrap Table)
        // ====================================================================

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? source = null,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var hotelId = GetCurrentHotelId();

                DateTime? parsedFrom = DateTime.TryParse(dateFrom, out var df) ? df : null;
                DateTime? parsedTo = DateTime.TryParse(dateTo, out var dt) ? dt : null;

                var (items, total) = await _reservationService.GetListAsync(
                    hotelId, search, status, source, parsedFrom, parsedTo, pageNumber, pageSize);

                return Json(new { success = true, data = items, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas");
                return Json(new { success = false, message = "Error al cargar las reservas" });
            }
        }

        // ====================================================================
        // API — DETALLE DE UNA RESERVA (GET JSON)
        // ====================================================================

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var reservation = await _reservationService.GetByIdAsync(hotelId, id);

                if (reservation == null)
                    return Json(new { success = false, message = "Reserva no encontrada" });

                return Json(new { success = true, data = reservation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva {ReservationId}", id);
                return Json(new { success = false, message = "Error al obtener la reserva" });
            }
        }

        // ====================================================================
        // PARTIAL VIEW — Modal detalles de reserva
        // ====================================================================

        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var reservation = await _reservationService.GetByIdAsync(hotelId, id);

                if (reservation == null)
                    return NotFound();

                return PartialView("_ReservationDetailsModal", reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de reserva {ReservationId}", id);
                return StatusCode(500);
            }
        }

        // ====================================================================
        // API — CANCELAR RESERVA
        // ====================================================================

        [HttpPost("Cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReservationRequest? request = null)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();

                var result = await _reservationService.CancelAsync(
                    hotelId, id, userId, request?.Reason);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = "Reserva cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva {ReservationId}", id);
                return Json(new { success = false, message = "Error al cancelar la reserva" });
            }
        }

        // ====================================================================
        // API — CONFIGURACIÓN DEL FORMULARIO
        // ====================================================================

        [HttpGet("GetFormConfig")]
        public async Task<IActionResult> GetFormConfig()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var config = await _reservationService.GetFormConfigAsync(hotelId);

                return Json(new { success = true, data = config ?? new ReservationFormConfigDto() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración del formulario");
                return Json(new { success = false, message = "Error al obtener la configuración" });
            }
        }

        // ====================================================================
        // PARTIAL VIEW — Modal gestión de pagos (Componente 5)
        // GET /Reservations/GetPaymentModal/{id}
        // ====================================================================

        [HttpGet("GetPaymentModal/{id}")]
        public async Task<IActionResult> GetPaymentModal(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var reservation = await _reservationService.GetByIdAsync(hotelId, id);

                if (reservation == null)
                    return NotFound();

                // Pasar si el usuario puede hacer devoluciones
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                ViewBag.CanRefund = userRole is "Administrador" or "Gerente";

                return PartialView("_PaymentManagementModal", reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de pagos {ReservationId}", id);
                return StatusCode(500);
            }
        }

        // ====================================================================
        // API — Registrar pago (Componente 5)
        // POST /Reservations/AddPayment
        // ====================================================================

        [HttpPost("AddPayment")]
        public async Task<IActionResult> AddPayment([FromBody] AddPaymentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Datos inválidos" });

                var hotelId = GetCurrentHotelId();
                var userId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var isManagerOrAdmin = userRole is "Administrador" or "Gerente";

                var result = await _reservationService.AddPaymentAsync(
                    hotelId,
                    dto.ReservationId,
                    dto.Amount,
                    dto.PaymentMethod,
                    dto.PaymentType,
                    dto.Reference,
                    userId,
                    isManagerOrAdmin);

                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Message });

                return Json(new
                {
                    success = true,
                    message = "Pago registrado correctamente",
                    paymentId = result.Data.PaymentId,
                    newBalance = result.Data.NewBalance,
                    newStatus = result.Data.NewStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago");
                return Json(new { success = false, message = "Error al registrar el pago" });
            }
        }

        // ====================================================================
        // API — Obtener pagos de una reserva (Componente 5)
        // GET /Reservations/GetPayments/{id}
        // ====================================================================

        [HttpGet("GetPayments/{id}")]
        public async Task<IActionResult> GetPayments(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var payments = await _reservationService.GetPaymentsAsync(hotelId, id);

                return Json(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos {ReservationId}", id);
                return Json(new { success = false, message = "Error al obtener los pagos" });
            }
        }
    }

    // ── Request helpers ──────────────────────────────────────────────────────

    public class CancelReservationRequest
    {
        public string? Reason { get; set; }
    }
}
