// GarthHMS.Core/Interfaces/IGuestRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Guest;

namespace GarthHMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz del repositorio de huéspedes
    /// </summary>
    public interface IGuestRepository
    {
        /// <summary>
        /// Crear un nuevo huésped
        /// </summary>
        Task<Guid> CreateAsync(Guid hotelId, CreateGuestDto dto, Guid createdBy);

        /// <summary>
        /// Actualizar un huésped existente
        /// </summary>
        Task<bool> UpdateAsync(Guid hotelId, UpdateGuestDto dto);

        /// <summary>
        /// Obtener huésped por ID
        /// </summary>
        Task<GuestDto?> GetByIdAsync(Guid hotelId, Guid guestId);

        /// <summary>
        /// Obtener lista paginada de huéspedes con filtros
        /// </summary>
        /// <param name="hotelId">ID del hotel</param>
        /// <param name="search">Búsqueda por nombre, teléfono o email</param>
        /// <param name="isVip">Filtrar solo VIPs (null = todos)</param>
        /// <param name="isBlacklisted">Filtrar solo blacklist (null = todos)</param>
        /// <param name="source">Filtrar por origen (null = todos)</param>
        /// <param name="pageNumber">Número de página (1-based)</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="sortBy">Campo para ordenar</param>
        /// <param name="sortOrder">Orden: asc o desc</param>
        Task<(IEnumerable<GuestListDto> guests, int totalCount)> GetListAsync(
            Guid hotelId,
            string? search = null,
            bool? isVip = null,
            bool? isBlacklisted = null,
            string? source = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created_at",
            string sortOrder = "desc"
        );

        /// <summary>
        /// Búsqueda rápida para autocomplete
        /// </summary>
        /// <param name="hotelId">ID del hotel</param>
        /// <param name="query">Texto a buscar (nombre, teléfono, email)</param>
        /// <param name="maxResults">Número máximo de resultados</param>
        Task<IEnumerable<GuestSearchDto>> SearchAsync(Guid hotelId, string query, int maxResults = 10);

        /// <summary>
        /// Verificar si existe un huésped con el mismo teléfono o email
        /// </summary>
        /// <param name="hotelId">ID del hotel</param>
        /// <param name="phone">Teléfono a verificar</param>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeGuestId">ID del huésped a excluir (para updates)</param>
        Task<Guid?> CheckDuplicateAsync(Guid hotelId, string phone, string? email = null, Guid? excludeGuestId = null);

        /// <summary>
        /// Marcar/desmarcar huésped en blacklist
        /// </summary>
        Task<bool> ToggleBlacklistAsync(Guid hotelId, Guid guestId, bool isBlacklisted, string? reason = null);

        /// <summary>
        /// Eliminar huésped (soft delete)
        /// Solo si no tiene reservas activas
        /// </summary>
        Task<bool> DeleteAsync(Guid hotelId, Guid guestId);
    }
}