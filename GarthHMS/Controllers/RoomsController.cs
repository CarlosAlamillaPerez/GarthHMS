// GarthHMS.Web/Controllers/RoomsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.DTOs.Room;
using GarthHMS.Core.Enums;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Rooms")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(
            IRoomService roomService,
            IRoomTypeService roomTypeService,
            ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _logger = logger;
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
                var room = await _roomService.GetByIdAsync(id);
                if (room == null)
                    return NotFound();

                // Obtener tipos de habitación para el dropdown
                var roomTypes = await _roomTypeService.GetAllActiveAsync();
                ViewBag.RoomTypes = roomTypes;

                // Convertir a UpdateRoomDto
                var dto = new UpdateRoomDto
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    Floor = room.Floor,
                    Notes = room.Notes
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
                var room = await _roomService.GetByIdAsync(id);
                if (room == null)
                    return NotFound();

                return PartialView("_ViewDetailsModal", room);
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
                var room = await _roomService.GetByIdAsync(id);
                if (room == null)
                    return NotFound();

                return PartialView("_DeleteConfirmation", room);
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
                var rooms = await _roomService.GetAllAsync();
                return Json(new { success = true, data = rooms });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener habitaciones");
                return Json(new { success = false, message = "Error al cargar las habitaciones" });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
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

                var roomId = await _roomService.CreateAsync(dto);

                return Json(new
                {
                    success = true,
                    message = "Habitación creada exitosamente",
                    data = new { roomId }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear habitación");
                return Json(new { success = false, message = "Error al crear la habitación" });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateRoomDto dto)
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

                await _roomService.UpdateAsync(dto);

                return Json(new
                {
                    success = true,
                    message = "Habitación actualizada exitosamente"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar habitación");
                return Json(new { success = false, message = "Error al actualizar la habitación" });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _roomService.DeleteAsync(id);

                return Json(new
                {
                    success = true,
                    message = "Habitación eliminada exitosamente"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar habitación");
                return Json(new { success = false, message = "Error al eliminar la habitación" });
            }
        }

        // ====================================================================
        // ENDPOINTS ADICIONALES (Cambio de estado)
        // ====================================================================

        [HttpPost("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                // Validar que el request no sea null
                if (request == null)
                {
                    return Json(new { success = false, message = "Request inválido" });
                }

                // Validar que StatusString no sea null o vacío
                if (string.IsNullOrWhiteSpace(request.StatusString))
                {
                    return Json(new { success = false, message = "El estado es requerido" });
                }

                // Log para debug
                _logger.LogInformation("Intentando cambiar estado de habitación {RoomId} a {Status}",
                    id, request.StatusString);

                // Convertir string a enum
                if (!Enum.TryParse<RoomStatus>(request.StatusString, true, out var status))
                {
                    return Json(new { success = false, message = $"Estado inválido: {request.StatusString}" });
                }

                await _roomService.UpdateStatusAsync(id, status);

                return Json(new
                {
                    success = true,
                    message = $"Estado actualizado a {GetStatusText(status)}"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de habitación {RoomId}", id);
                return Json(new { success = false, message = "Error al actualizar el estado" });
            }
        }

        // Helper para obtener texto en español
        private string GetStatusText(RoomStatus status)
        {
            return status switch
            {
                RoomStatus.Available => "Disponible",
                RoomStatus.Occupied => "Ocupada",
                RoomStatus.Dirty => "Sucia",
                RoomStatus.Cleaning => "En Limpieza",
                RoomStatus.Maintenance => "Mantenimiento",
                RoomStatus.Reserved => "Reservada",
                _ => status.ToString()
            };
        }

        [HttpPost("SetMaintenance/{id}")]
        public async Task<IActionResult> SetMaintenance(Guid id, [FromBody] MaintenanceRequest request)
        {
            try
            {
                await _roomService.SetMaintenanceAsync(id, request.Notes);

                return Json(new
                {
                    success = true,
                    message = "Habitación puesta en mantenimiento"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al poner en mantenimiento");
                return Json(new { success = false, message = "Error al poner en mantenimiento" });
            }
        }

        [HttpPost("SetAvailable/{id}")]
        public async Task<IActionResult> SetAvailable(Guid id)
        {
            try
            {
                await _roomService.SetAvailableAsync(id);

                return Json(new
                {
                    success = true,
                    message = "Habitación marcada como disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como disponible");
                return Json(new { success = false, message = "Error al marcar como disponible" });
            }
        }
    }


    // ====================================================================
    // REQUEST MODELS
    // ====================================================================

    public class UpdateStatusRequest
    {
        public string? StatusString { get; set; }
    }

    public class MaintenanceRequest
    {
        public string? Notes { get; set; }
    }

}