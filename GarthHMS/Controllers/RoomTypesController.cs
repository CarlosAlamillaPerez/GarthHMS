using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.DTOs.RoomType;
using System;
using System.Threading.Tasks;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("RoomTypes")]
    public class RoomTypesController : Controller
    {
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<RoomTypesController> _logger;

        public RoomTypesController(
            IRoomTypeService roomTypeService,
            ILogger<RoomTypesController> logger)
        {
            _roomTypeService = roomTypeService;
            _logger = logger;
        }

        // GET: /RoomTypes
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================================================
        // PARTIAL VIEWS (Para mostrar en SweetAlert2)
        // ========================================================================

        // GET: /RoomTypes/GetCreateForm
        [HttpGet("GetCreateForm")]
        public IActionResult GetCreateForm()
        {
            return PartialView("_CreateModal");
        }

        // GET: /RoomTypes/GetEditForm/{id}
        [HttpGet("GetEditForm/{id}")]
        public async Task<IActionResult> GetEditForm(Guid id)
        {
            try
            {
                var roomType = await _roomTypeService.GetByIdAsync(id);

                if (roomType == null)
                    return NotFound();

                return PartialView("_EditModal", roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario de edición");
                return StatusCode(500);
            }
        }

        // GET: /RoomTypes/GetDetails/{id}
        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var roomType = await _roomTypeService.GetByIdAsync(id);

                if (roomType == null)
                    return NotFound();

                return PartialView("_ViewDetailsModal", roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles");
                return StatusCode(500);
            }
        }

        // GET: /RoomTypes/GetDeleteConfirmation/{id}
        [HttpGet("GetDeleteConfirmation/{id}")]
        public async Task<IActionResult> GetDeleteConfirmation(Guid id)
        {
            try
            {
                var roomType = await _roomTypeService.GetByIdAsync(id);

                if (roomType == null)
                    return NotFound();

                return PartialView("_DeleteConfirmation", roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener confirmación de eliminación");
                return StatusCode(500);
            }
        }

        // ========================================================================
        // API ENDPOINTS (JSON)
        // ========================================================================

        // GET: /RoomTypes/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roomTypes = await _roomTypeService.GetAllAsync();
                return Json(new { success = true, data = roomTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de habitación");
                return Json(new { success = false, message = "Error al cargar los tipos de habitación" });
            }
        }

        // POST: /RoomTypes/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateRoomTypeDto dto)
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

                var roomTypeId = await _roomTypeService.CreateAsync(dto);

                return Json(new
                {
                    success = true,
                    message = "Tipo de habitación creado exitosamente",
                    data = new { roomTypeId }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo de habitación");
                return Json(new { success = false, message = "Error al crear el tipo de habitación" });
            }
        }

        // POST: /RoomTypes/Update
        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateRoomTypeDto dto)
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

                await _roomTypeService.UpdateAsync(dto);

                return Json(new
                {
                    success = true,
                    message = "Tipo de habitación actualizado exitosamente"
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
                _logger.LogError(ex, "Error al actualizar tipo de habitación");
                return Json(new { success = false, message = "Error al actualizar el tipo de habitación" });
            }
        }

        // POST: /RoomTypes/Delete/{id}
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _roomTypeService.DeleteAsync(id);

                return Json(new
                {
                    success = true,
                    message = "Tipo de habitación eliminado exitosamente"
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo de habitación");
                return Json(new { success = false, message = "Error al eliminar el tipo de habitación" });
            }
        }
    }
}