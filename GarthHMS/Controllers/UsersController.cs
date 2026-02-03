using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.User;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IRoleService roleService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
        }

        // ====================================================================
        // VISTA PRINCIPAL
        // ====================================================================

        /// <summary>
        /// Vista principal con la tabla de usuarios
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ====================================================================
        // API - OBTENER DATOS
        // ====================================================================

        /// <summary>
        /// Obtiene todos los usuarios del hotel actual en formato JSON para Bootstrap Table
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var hotelId = GetCurrentHotelId();
                var users = await _userService.GetUsersByHotelAsync(hotelId);

                return Ok(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return BadRequest(new
                {
                    success = false,
                    message = "Error al obtener los usuarios"
                });
            }
        }

        // ====================================================================
        // CREAR USUARIO
        // ====================================================================

        /// <summary>
        /// Muestra el formulario para crear un nuevo usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCreateForm()
        {
            try
            {
                var hotelId = GetCurrentHotelId();

                // Obtener roles activos del hotel para el dropdown
                var rolesResult = await _roleService.GetAllActiveAsync(hotelId);
                ViewBag.Roles = rolesResult.IsSuccess ? rolesResult.Data : Enumerable.Empty<dynamic>();

                return PartialView("_CreateModal", new CreateUserDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                return PartialView("_Error");
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new
                    {
                        success = false,
                        message = $"Datos inválidos: {errors}"
                    });
                }

                var currentUserId = GetCurrentUserId();
                var result = await _userService.CreateUserAsync(dto, currentUserId);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Usuario creado exitosamente",
                        data = new { userId = result.UserId }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al crear el usuario"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return BadRequest(new
                {
                    success = false,
                    message = "Error al crear el usuario"
                });
            }
        }

        // ====================================================================
        // EDITAR USUARIO
        // ====================================================================

        /// <summary>
        /// Muestra el formulario para editar un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEditForm(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var hotelId = GetCurrentHotelId();

                // Obtener roles activos del hotel para el dropdown
                var rolesResult = await _roleService.GetAllActiveAsync(hotelId);
                ViewBag.Roles = rolesResult.IsSuccess ? rolesResult.Data : Enumerable.Empty<dynamic>();

                var dto = new UpdateUserDto
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Username = user.Username,
                    RoleId = user.RoleId,
                    Phone = user.Phone,
                    PhotoUrl = user.PhotoUrl,
                    IsActive = user.IsActive
                };

                return PartialView("_EditModal", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de edición: {UserId}", id);
                return PartialView("_Error");
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new
                    {
                        success = false,
                        message = $"Datos inválidos: {errors}"
                    });
                }

                var result = await _userService.UpdateUserAsync(dto);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Usuario actualizado exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al actualizar el usuario"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {UserId}", dto.UserId);
                return BadRequest(new
                {
                    success = false,
                    message = "Error al actualizar el usuario"
                });
            }
        }

        // ====================================================================
        // ELIMINAR USUARIO
        // ====================================================================

        /// <summary>
        /// Muestra el modal de confirmación para eliminar un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDeleteConfirmation(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return PartialView("_DeleteConfirmation", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar confirmación de eliminación: {UserId}", id);
                return PartialView("_Error");
            }
        }

        /// <summary>
        /// Elimina un usuario (soft delete)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Validar que no se elimine a sí mismo
                if (id == currentUserId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No puedes eliminar tu propio usuario"
                    });
                }

                var result = await _userService.DeleteUserAsync(id);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Usuario eliminado exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al eliminar el usuario"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {UserId}", id);
                return BadRequest(new
                {
                    success = false,
                    message = "Error al eliminar el usuario"
                });
            }
        }

        // ====================================================================
        // VER DETALLES
        // ====================================================================

        /// <summary>
        /// Muestra los detalles completos de un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Obtener permisos efectivos del usuario
                var permissions = await _userService.GetEffectivePermissionsAsync(id);
                ViewBag.Permissions = permissions;

                return PartialView("_ViewDetailsModal", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles: {UserId}", id);
                return PartialView("_Error");
            }
        }

        // ====================================================================
        // CAMBIAR CONTRASEÑA
        // ====================================================================

        /// <summary>
        /// Muestra el formulario para cambiar la contraseña de un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChangePasswordForm(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                ViewBag.UserFullName = user.FullName;

                var dto = new ChangePasswordDto
                {
                    UserId = id
                };

                return PartialView("_ChangePasswordModal", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de cambio de contraseña: {UserId}", id);
                return PartialView("_Error");
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new
                    {
                        success = false,
                        message = $"Datos inválidos: {errors}"
                    });
                }

                var result = await _userService.ChangePasswordAsync(dto);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Contraseña cambiada exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al cambiar la contraseña"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña: {UserId}", dto.UserId);
                return BadRequest(new
                {
                    success = false,
                    message = "Error al cambiar la contraseña"
                });
            }
        }

        /// <summary>
        /// Restablece la contraseña de un usuario (solo para administradores)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(id);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Contraseña restablecida exitosamente",
                        data = new { temporaryPassword = result.NewPassword }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al restablecer la contraseña"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña: {UserId}", id);
                return BadRequest(new
                {
                    success = false,
                    message = "Error al restablecer la contraseña"
                });
            }
        }

        // ====================================================================
        // ACTIVAR/DESACTIVAR
        // ====================================================================

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Validar que no se desactive a sí mismo
                if (id == currentUserId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No puedes cambiar el estado de tu propio usuario"
                    });
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    });
                }

                var result = user.IsActive
                    ? await _userService.DeactivateUserAsync(id)
                    : await _userService.ActivateUserAsync(id);

                if (result.Success)
                {
                    var newStatus = !user.IsActive;
                    return Ok(new
                    {
                        success = true,
                        message = newStatus ? "Usuario activado exitosamente" : "Usuario desactivado exitosamente",
                        data = new { isActive = newStatus }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Error al cambiar el estado del usuario"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado: {UserId}", id);
                return BadRequest(new
                {
                    success = false,
                    message = "Error al cambiar el estado del usuario"
                });
            }
        }

        // ====================================================================
        // MÉTODOS AUXILIARES
        // ====================================================================

        private Guid GetCurrentHotelId()
        {
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;

            if (string.IsNullOrEmpty(hotelIdClaim) || !Guid.TryParse(hotelIdClaim, out var hotelId))
            {
                throw new UnauthorizedAccessException("No se pudo obtener el HotelId del usuario actual");
            }

            return hotelId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("No se pudo obtener el UserId del usuario actual");
            }

            return userId;
        }
    }
}