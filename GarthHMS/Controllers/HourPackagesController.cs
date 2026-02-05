// GarthHMS.Web/Controllers/HourPackagesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.DTOs.HourPackage;
using GarthHMS.Core.Interfaces.Services;
using System.Security.Claims;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("HourPackages")]
    public class HourPackagesController : Controller
    {
        private readonly IHourPackageService _hourPackageService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<HourPackagesController> _logger;

        public HourPackagesController(
            IHourPackageService hourPackageService,
            IRoomTypeService roomTypeService,
            ILogger<HourPackagesController> logger)
        {
            _hourPackageService = hourPackageService;
            _roomTypeService = roomTypeService;
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

        [HttpGet("GetCreateForm")]
        public async Task<IActionResult> GetCreateForm()
        {
            try
            {
                // Obtener tipos de habitación activos para el dropdown
                var roomTypes = await _roomTypeService.GetAllActiveAsync();
                ViewBag.RoomTypes = roomTypes;

                return PartialView("_CreateModal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario de creación");
                return StatusCode(500);
            }
        }

        [HttpGet("GetEditForm/{id}")]
        public async Task<IActionResult> GetEditForm(Guid id)
        {
            try
            {
                var result = await _hourPackageService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                // Obtener tipos de habitación para el dropdown
                var roomTypes = await _roomTypeService.GetAllActiveAsync();
                ViewBag.RoomTypes = roomTypes;

                // Convertir a UpdateDto
                var dto = new UpdateHourPackageDto
                {
                    HourPackageId = result.Data.HourPackageId,
                    Name = result.Data.Name,
                    Hours = result.Data.Hours,
                    Price = result.Data.Price,
                    ExtraHourPrice = result.Data.ExtraHourPrice,
                    AllowsExtensions = result.Data.AllowsExtensions,
                    DisplayOrder = result.Data.DisplayOrder
                };

                ViewBag.CurrentRoomTypeId = result.Data.RoomTypeId;
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
                var result = await _hourPackageService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                return PartialView("_ViewDetailsModal", result.Data);
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
                var result = await _hourPackageService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                return PartialView("_DeleteConfirmation", result.Data);
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var result = await _hourPackageService.GetAllByHotelAsync(hotelId);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes de horas");
                return Json(new { success = false, message = "Error al cargar los paquetes de horas" });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateHourPackageDto dto)
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

                var result = await _hourPackageService.CreateAsync(dto, hotelId, userId);

                if (result.IsSuccess)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        data = new { hourPackageId = result.Data }
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear paquete de horas");
                return Json(new { success = false, message = "Error al crear el paquete de horas" });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateHourPackageDto dto)
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
                var result = await _hourPackageService.UpdateAsync(dto, hotelId);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar paquete de horas");
                return Json(new { success = false, message = "Error al actualizar el paquete de horas" });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var result = await _hourPackageService.DeleteAsync(id, hotelId);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paquete de horas");
                return Json(new { success = false, message = "Error al eliminar el paquete de horas" });
            }
        }
    }
}