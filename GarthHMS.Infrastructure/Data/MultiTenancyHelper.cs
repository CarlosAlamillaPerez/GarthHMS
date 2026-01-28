using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.AspNetCore.Http;

namespace GarthHMS.Infrastructure.Data
{
    /// <summary>
    /// Helper para manejar multi-tenancy con Row Level Security (RLS) en PostgreSQL
    /// Establece el hotel_id en el contexto de la sesión de PostgreSQL
    /// </summary>
    public class MultiTenancyHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BaseDeDatos _baseDeDatos;

        public MultiTenancyHelper(IHttpContextAccessor httpContextAccessor, BaseDeDatos baseDeDatos)
        {
            _httpContextAccessor = httpContextAccessor;
            _baseDeDatos = baseDeDatos;
        }

        /// <summary>
        /// Obtiene el HotelId del usuario actual desde la sesión/claims
        /// </summary>
        public int? GetCurrentHotelId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            var hotelIdClaim = httpContext.User.FindFirst("HotelId")?.Value;

            if (int.TryParse(hotelIdClaim, out var hotelId))
                return hotelId;

            return null;
        }

        /// <summary>
        /// Obtiene el UserId del usuario actual desde la sesión/claims
        /// </summary>
        public int? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = httpContext.User.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        /// <summary>
        /// Establece el hotel_id en el contexto de PostgreSQL para RLS
        /// Debe llamarse al inicio de cada request
        /// </summary>
        public async Task SetHotelContextAsync(NpgsqlConnection connection, int hotelId)
        {
            await using var command = new NpgsqlCommand(
                $"SET LOCAL app.current_hotel_id = {hotelId};",
                connection
            );

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Establece el user_id en el contexto de PostgreSQL para auditoría
        /// </summary>
        public async Task SetUserContextAsync(NpgsqlConnection connection, int userId)
        {
            await using var command = new NpgsqlCommand(
                $"SET LOCAL app.current_user_id = {userId};",
                connection
            );

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Establece tanto hotel_id como user_id en una sola llamada
        /// </summary>
        public async Task SetContextAsync(NpgsqlConnection connection, int hotelId, int userId)
        {
            await using var command = new NpgsqlCommand(
                $@"SET LOCAL app.current_hotel_id = {hotelId};
                   SET LOCAL app.current_user_id = {userId};",
                connection
            );

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Verifica si el usuario actual es SuperAdmin
        /// </summary>
        public bool IsSuperAdmin()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            var roleClaim = httpContext.User.FindFirst("Role")?.Value;
            return roleClaim == "SuperAdmin";
        }

        /// <summary>
        /// Obtiene el rol del usuario actual
        /// </summary>
        public string? GetCurrentUserRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            return httpContext.User.FindFirst("Role")?.Value;
        }

        /// <summary>
        /// Verifica si el usuario tiene un permiso específico
        /// </summary>
        public bool HasPermission(string permission)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            // SuperAdmin tiene todos los permisos
            if (IsSuperAdmin())
                return true;

            var permissionsClaim = httpContext.User.FindFirst("Permissions")?.Value;
            if (string.IsNullOrEmpty(permissionsClaim))
                return false;

            return permissionsClaim.Contains(permission, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el límite de descuento del usuario actual
        /// </summary>
        public decimal GetMaxDiscountPercent()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return 0;

            var discountClaim = httpContext.User.FindFirst("MaxDiscount")?.Value;

            if (decimal.TryParse(discountClaim, out var maxDiscount))
                return maxDiscount;

            return 0;
        }
    }
}
