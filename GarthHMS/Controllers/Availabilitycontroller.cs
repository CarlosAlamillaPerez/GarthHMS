// GarthHMS.Web/Controllers/AvailabilityController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.DTOs.AvailabilityEngine;
using GarthHMS.Core.Interfaces.Services;
using System.Security.Claims;
using GarthHMS.Application.Services;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Availability")]
    public class AvailabilityController : Controller
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(
            IAvailabilityService availabilityService,
            IReservationService reservationService,  
            ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _reservationService = reservationService;  
            _logger = logger;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private Guid GetCurrentHotelId()
        {
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            return Guid.Parse(hotelIdClaim ?? Guid.Empty.ToString());
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        // ====================================================================
        // VISTA PRINCIPAL
        // ====================================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ====================================================================
        // PARTIAL VIEWS (Para SweetAlert2)
        // ====================================================================

        /// <summary>
        /// Panel de detalle de una reserva (modal de lectura)
        /// GET /Availability/GetReservationDetails/{id}
        /// </summary>
        [HttpGet("GetReservationDetails/{id}")]
        public async Task<IActionResult> GetReservationDetails(Guid id)
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
                _logger.LogError(ex, "Error al cargar detalle de reserva {ReservationId}", id);
                return StatusCode(500);
            }
        }

        // ====================================================================
        // API ENDPOINTS (JSON) — CALENDARIO
        // ====================================================================

        /// <summary>
        /// Datos de ocupación por día para el calendario mensual
        /// GET /Availability/GetCalendarData?year=2026&month=3
        /// </summary>
        [HttpGet("GetCalendarData")]
        public async Task<IActionResult> GetCalendarData(int? year, int? month)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var targetYear = year ?? DateTime.Today.Year;
                var targetMonth = month ?? DateTime.Today.Month;

                var days = await _availabilityService.GetMonthlyCalendarAsync(
                    hotelId, targetYear, targetMonth);

                return Ok(new
                {
                    success = true,
                    data = days.Select(d => new
                    {
                        date = d.DayDate.ToString("yyyy-MM-dd"),
                        totalRooms = d.TotalRooms,
                        occupiedRooms = d.OccupiedRooms,
                        reservationCount = d.ReservationCount,
                        availableRooms = d.AvailableRooms,
                        occupancyPercent = d.OccupancyPercent,
                        occupancyLevel = d.OccupancyLevel
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos del calendario");
                return BadRequest(new { success = false, message = "Error al cargar el calendario" });
            }
        }

        /// <summary>
        /// Resumen de ocupación del día seleccionado (panel inferior del calendario)
        /// GET /Availability/GetDaySummary?date=2026-03-07
        /// </summary>
        [HttpGet("GetDaySummary")]
        public async Task<IActionResult> GetDaySummary(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { success = false, message = "Fecha inválida" });

                var hotelId = GetCurrentHotelId();
                var summary = await _availabilityService.GetDaySummaryAsync(hotelId, parsedDate);

                if (summary == null)
                    return Ok(new { success = false, message = "Sin datos para esa fecha" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalRooms = summary.TotalRooms,
                        occupiedRooms = summary.OccupiedRooms,
                        availableRooms = summary.AvailableRooms,
                        reservedRooms = summary.ReservedRooms,
                        maintenanceRooms = summary.MaintenanceRooms,
                        occupancyPercent = summary.OccupancyPercent
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar resumen del día");
                return BadRequest(new { success = false, message = "Error al cargar el resumen" });
            }
        }

        // ====================================================================
        // API ENDPOINTS (JSON) — LISTAS DE RESERVAS
        // ====================================================================

        /// <summary>
        /// Reservas con check-in HOY
        /// GET /Availability/GetTodayReservations
        /// </summary>
        [HttpGet("GetTodayReservations")]
        public async Task<IActionResult> GetTodayReservations()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var reservations = await _availabilityService.GetTodayReservationsAsync(hotelId);

                return Ok(new { success = true, data = MapReservationList(reservations) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reservas de hoy");
                return BadRequest(new { success = false, message = "Error al cargar reservas de hoy" });
            }
        }

        /// <summary>
        /// Reservas de los próximos 7 días
        /// GET /Availability/GetUpcomingReservations
        /// </summary>
        [HttpGet("GetUpcomingReservations")]
        public async Task<IActionResult> GetUpcomingReservations()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var reservations = await _availabilityService.GetUpcomingReservationsAsync(hotelId);

                return Ok(new { success = true, data = MapReservationList(reservations) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar próximas reservas");
                return BadRequest(new { success = false, message = "Error al cargar próximas reservas" });
            }
        }

        /// <summary>
        /// Búsqueda global por folio, nombre de huésped o teléfono
        /// GET /Availability/SearchReservations?q=R-2026&limit=20
        /// </summary>
        [HttpGet("SearchReservations")]
        public async Task<IActionResult> SearchReservations(string? q, int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                    return Ok(new { success = true, data = Array.Empty<object>() });

                var hotelId = GetCurrentHotelId();
                var reservations = await _availabilityService.SearchReservationsAsync(
                    hotelId, q.Trim(), Math.Min(limit, 50));

                return Ok(new { success = true, data = MapReservationList(reservations) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda global de reservas");
                return BadRequest(new { success = false, message = "Error al buscar reservas" });
            }
        }

        /// <summary>
        /// Reservas activas para la fecha seleccionada en el calendario
        /// GET /Availability/GetReservationsByDate?date=2026-03-07
        /// </summary>
        [HttpGet("GetReservationsByDate")]
        public async Task<IActionResult> GetReservationsByDate(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { success = false, message = "Fecha inválida" });

                var hotelId = GetCurrentHotelId();
                var reservations = await _availabilityService.GetReservationsByDateAsync(hotelId, parsedDate);

                return Ok(new { success = true, data = MapReservationList(reservations) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reservas por fecha");
                return BadRequest(new { success = false, message = "Error al cargar reservas" });
            }
        }

        // ====================================================================
        // API ENDPOINTS (JSON) — DISPONIBILIDAD (usado por Componente 3)
        // ====================================================================

        /// <summary>
        /// Habitaciones disponibles para un rango de fechas
        /// GET /Availability/GetAvailableRooms?checkIn=2026-03-07&checkOut=2026-03-09
        /// </summary>
        [HttpGet("GetAvailableRooms")]
        public async Task<IActionResult> GetAvailableRooms(
            string checkIn,
            string checkOut,
            Guid? roomTypeId = null,
            bool? requiresPets = null)
        {
            try
            {
                if (!DateTime.TryParse(checkIn, out var checkInDate))
                    return BadRequest(new { success = false, message = "Fecha de check-in inválida" });

                if (!DateTime.TryParse(checkOut, out var checkOutDate))
                    return BadRequest(new { success = false, message = "Fecha de check-out inválida" });

                var hotelId = GetCurrentHotelId();

                var query = new AvailabilityQueryDto
                {
                    HotelId = hotelId,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    RoomTypeId = roomTypeId
                };

                var rooms = await _availabilityService.GetAvailableRoomsAsync(query, requiresPets);

                return Ok(new
                {
                    success = true,
                    data = rooms.Select(r => new
                    {
                        roomId = r.RoomId,
                        roomNumber = r.RoomNumber,
                        floor = r.Floor,
                        roomTypeId = r.RoomTypeId,
                        roomTypeName = r.RoomTypeName,
                        roomTypeCode = r.RoomTypeCode,
                        baseCapacity = r.BaseCapacity,
                        maxCapacity = r.MaxCapacity,
                        basePriceNightly = r.BasePriceNightly,
                        extraPersonCharge = r.ExtraPersonCharge,
                        allowsPets = r.AllowsPets,
                        petCharge = r.PetCharge,
                        bedType = r.BedType,
                        viewType = r.ViewType,
                        sizeSqm = r.SizeSqm
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad");
                return BadRequest(new { success = false, message = "Error al verificar disponibilidad" });
            }
        }

        // ====================================================================
        // MAPPER PRIVADO
        // ====================================================================

        private static object MapReservationList(
            IEnumerable<ReservationListItemDto> reservations)
        {
            return reservations.Select(r => new
            {
                reservationId = r.ReservationId,
                folio = r.Folio,
                status = r.Status,
                statusLabel = r.StatusLabel,
                source = r.Source,
                sourceLabel = r.SourceLabel,
                checkInDate = r.CheckInDate.ToString("yyyy-MM-dd"),
                checkOutDate = r.CheckOutDate.ToString("yyyy-MM-dd"),
                numNights = r.NumNights,
                total = r.Total,
                depositAmount = r.DepositAmount,
                depositPaidAt = r.DepositPaidAt?.ToString("yyyy-MM-dd"),
                balancePending = r.BalancePending,
                hasDeposit = r.HasDeposit,
                hasUnverifiedPayments = r.HasUnverifiedPayments,
                requiresInvoice = r.RequiresInvoice,
                isCheckInToday = r.IsCheckInToday,
                guestId = r.GuestId,
                guestFullName = r.GuestFullName,
                guestPhone = r.GuestPhone,
                isVip = r.IsVip,
                roomsSummary = r.RoomsSummary,
                roomCount = r.RoomCount
            });
        }

        
    }
}