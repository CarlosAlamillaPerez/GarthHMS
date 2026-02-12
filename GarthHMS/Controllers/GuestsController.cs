// GarthHMS.Web/Controllers/GuestsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.DTOs.Guest;
using System.Security.Claims;
using GarthHMS.Core.Interfaces.Services;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Guests")]
    public class GuestsController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly ILogger<GuestsController> _logger;

        public GuestsController(
            IGuestService guestService,
            ILogger<GuestsController> _logger)
        {
            _guestService = guestService;
            this._logger = _logger;
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

        [HttpGet("GetCreateForm")]
        public IActionResult GetCreateForm()
        {
            return PartialView("_CreateModal");
        }

        [HttpGet("GetEditForm/{id}")]
        public async Task<IActionResult> GetEditForm(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var guest = await _guestService.GetGuestByIdAsync(hotelId, id);

                if (guest == null)
                {
                    return NotFound();
                }

                // Convertir a UpdateDto
                var dto = new UpdateGuestDto
                {
                    GuestId = guest.GuestId,
                    FirstName = guest.FirstName,
                    LastName = guest.LastName,
                    Phone = guest.Phone,
                    Email = guest.Email,
                    PhoneSecondary = guest.PhoneSecondary,
                    IdType = guest.IdType,
                    IdNumber = guest.IdNumber,
                    IdPhotoUrl = guest.IdPhotoUrl,
                    Curp = guest.Curp,
                    BirthDate = guest.BirthDate,
                    AddressStreet = guest.AddressStreet,
                    AddressCity = guest.AddressCity,
                    AddressState = guest.AddressState,
                    AddressZip = guest.AddressZip,
                    AddressCountry = guest.AddressCountry,
                    BillingRfc = guest.BillingRfc,
                    BillingBusinessName = guest.BillingBusinessName,
                    BillingEmail = guest.BillingEmail,
                    BillingAddress = guest.BillingAddress,
                    BillingZip = guest.BillingZip,
                    BillingTaxRegime = guest.BillingTaxRegime,
                    Notes = guest.Notes,
                    IsVip = guest.IsVip,
                    IsBlacklisted = guest.IsBlacklisted,
                    BlacklistReason = guest.BlacklistReason,
                    Source = guest.Source
                };

                return PartialView("_EditModal", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario de edición");
                return StatusCode(500);
            }
        }

        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var guest = await _guestService.GetGuestByIdAsync(hotelId, id);

                if (guest == null)
                {
                    return NotFound();
                }

                return PartialView("_ViewDetailsModal", guest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles");
                return StatusCode(500);
            }
        }

        [HttpGet("GetDeleteConfirmation/{id}")]
        public async Task<IActionResult> GetDeleteConfirmation(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var guest = await _guestService.GetGuestByIdAsync(hotelId, id);

                if (guest == null)
                {
                    return NotFound();
                }

                return PartialView("_DeleteConfirmation", guest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener confirmación");
                return StatusCode(500);
            }
        }

        // ====================================================================
        // API ENDPOINTS (JSON)
        // ====================================================================

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isVip = null,
            [FromQuery] bool? isBlacklisted = null,
            [FromQuery] string? source = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "created_at",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var (guests, totalCount) = await _guestService.GetGuestsAsync(
                    hotelId,
                    search,
                    isVip,
                    isBlacklisted,
                    source,
                    pageNumber,
                    pageSize,
                    sortBy,
                    sortOrder
                );

                return Json(new
                {
                    success = true,
                    data = guests,
                    total = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener huéspedes");
                return Json(new { success = false, message = "Error al cargar los datos" });
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int maxResults = 10)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var results = await _guestService.SearchGuestsAsync(hotelId, query, maxResults);

                return Json(new
                {
                    success = true,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de huéspedes");
                return Json(new { success = false, message = "Error al buscar" });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateGuestDto dto)
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

                var (success, message, guestId) = await _guestService.CreateGuestAsync(hotelId, dto, userId);

                return Json(new
                {
                    success,
                    message,
                    data = new { guestId }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear huésped");
                return Json(new { success = false, message = "Error al crear el huésped" });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateGuestDto dto)
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
                var (success, message) = await _guestService.UpdateGuestAsync(hotelId, dto);

                return Json(new
                {
                    success,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar huésped");
                return Json(new { success = false, message = "Error al actualizar el huésped" });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var (success, message) = await _guestService.DeleteGuestAsync(hotelId, id);

                return Json(new
                {
                    success,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar huésped");
                return Json(new { success = false, message = "Error al eliminar el huésped" });
            }
        }

        [HttpPost("ToggleBlacklist")]
        public async Task<IActionResult> ToggleBlacklist([FromBody] ToggleBlacklistRequest request)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var (success, message) = await _guestService.ToggleBlacklistAsync(
                    hotelId,
                    request.GuestId,
                    request.IsBlacklisted,
                    request.Reason
                );

                return Json(new
                {
                    success,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de blacklist");
                return Json(new { success = false, message = "Error al actualizar el estado" });
            }
        }
    }

    // DTO para el request de toggle blacklist
    public class ToggleBlacklistRequest
    {
        public Guid GuestId { get; set; }
        public bool IsBlacklisted { get; set; }
        public string? Reason { get; set; }
    }
}