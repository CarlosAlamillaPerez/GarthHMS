using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GarthHMS.Core.DTOs.Role;
using System.Security.Claims;
using GarthHMS.Core.Interfaces.Services;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    [Route("Roles")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleService roleService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService;
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
        public IActionResult GetCreateForm()
        {
            return PartialView("_CreateModal");
        }

        [HttpGet("GetEditForm/{id}")]
        public async Task<IActionResult> GetEditForm(Guid id)
        {
            try
            {
                var result = await _roleService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                // Convertir a UpdateRoleDto
                var dto = new UpdateRoleDto
                {
                    RoleId = result.Data.RoleId,
                    Name = result.Data.Name,
                    Description = result.Data.Description,
                    MaxDiscountPercent = result.Data.MaxDiscountPercent
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
                var result = await _roleService.GetByIdAsync(id);
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
                var result = await _roleService.GetByIdAsync(id);
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

        [HttpGet("GetPermissionsForm/{id}")]
        public async Task<IActionResult> GetPermissionsForm(Guid id)
        {
            try
            {
                var result = await _roleService.GetRolePermissionsAsync(id);
                if (!result.IsSuccess)
                {
                    return BadRequest(result.Message);
                }

                ViewBag.RoleId = id;
                return PartialView("_AssignPermissionsModal", result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario de permisos");
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
                var result = await _roleService.GetByHotelAsync(hotelId);

                return Json(new
                {
                    success = result.IsSuccess,
                    data = result.Data,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return Json(new { success = false, message = "Error al cargar los datos" });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
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
                dto.CreatedBy = userId;

                var result = await _roleService.CreateAsync(hotelId, dto);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return Json(new { success = false, message = "Error al crear el rol" });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
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

                var result = await _roleService.UpdateAsync(dto);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol");
                return Json(new { success = false, message = "Error al actualizar el rol" });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol");
                return Json(new { success = false, message = "Error al eliminar el rol" });
            }
        }

        [HttpPost("AssignPermissions")]
        public async Task<IActionResult> AssignPermissions([FromBody] RolePermissionDto dto)
        {
            try
            {
                var result = await _roleService.AssignPermissionsAsync(dto);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar permisos");
                return Json(new { success = false, message = "Error al asignar permisos" });
            }
        }
    }
}