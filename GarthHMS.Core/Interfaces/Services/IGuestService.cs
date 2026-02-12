// GarthHMS.Core/Interfaces/IGuestService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Guest;

namespace GarthHMS.Core.Interfaces.Services
{
    /// <summary>
    /// Interfaz del servicio de huéspedes
    /// </summary>
    public interface IGuestService
    {
        /// <summary>
        /// Crear un nuevo huésped
        /// </summary>
        Task<(bool success, string message, Guid? guestId)> CreateGuestAsync(Guid hotelId, CreateGuestDto dto, Guid createdBy);

        /// <summary>
        /// Actualizar un huésped existente
        /// </summary>
        Task<(bool success, string message)> UpdateGuestAsync(Guid hotelId, UpdateGuestDto dto);

        /// <summary>
        /// Obtener un huésped por ID
        /// </summary>
        Task<GuestDto?> GetGuestByIdAsync(Guid hotelId, Guid guestId);

        /// <summary>
        /// Obtener lista paginada de huéspedes
        /// </summary>
        Task<(IEnumerable<GuestListDto> guests, int totalCount)> GetGuestsAsync(
            Guid hotelId,
            string? search = null,
            bool? isVip = null,
            bool? isBlacklisted = null,
            string? source = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created_at",
            string sortOrder = "desc");

        /// <summary>
        /// Búsqueda rápida de huéspedes (autocomplete)
        /// </summary>
        Task<IEnumerable<GuestSearchDto>> SearchGuestsAsync(Guid hotelId, string query, int maxResults = 10);

        /// <summary>
        /// Marcar/desmarcar huésped en blacklist
        /// </summary>
        Task<(bool success, string message)> ToggleBlacklistAsync(Guid hotelId, Guid guestId, bool isBlacklisted, string? reason = null);

        /// <summary>
        /// Eliminar un huésped (soft delete)
        /// </summary>
        Task<(bool success, string message)> DeleteGuestAsync(Guid hotelId, Guid guestId);
    }
}