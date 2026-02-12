// GarthHMS.Application/Services/GuestService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Guest;
using GarthHMS.Core.Interfaces;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio de huéspedes - Lógica de negocio
    /// </summary>
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _guestRepository;

        public GuestService(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
        }

        /// <summary>
        /// Crear un nuevo huésped
        /// </summary>
        public async Task<(bool success, string message, Guid? guestId)> CreateGuestAsync(
            Guid hotelId,
            CreateGuestDto dto,
            Guid createdBy)
        {
            try
            {
                // Validar duplicados
                var duplicateId = await _guestRepository.CheckDuplicateAsync(
                    hotelId,
                    dto.Phone,
                    dto.Email
                );

                if (duplicateId.HasValue)
                {
                    return (false, "Ya existe un huésped con el mismo teléfono o email", null);
                }

                // Normalizar datos
                dto.FirstName = NormalizeText(dto.FirstName);
                dto.LastName = NormalizeText(dto.LastName);
                dto.Phone = NormalizePhone(dto.Phone);

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    dto.Email = dto.Email.Trim().ToLowerInvariant();
                }

                if (!string.IsNullOrWhiteSpace(dto.Curp))
                {
                    dto.Curp = dto.Curp.ToUpperInvariant();
                }

                if (!string.IsNullOrWhiteSpace(dto.BillingRfc))
                {
                    dto.BillingRfc = dto.BillingRfc.ToUpperInvariant();
                }

                // Crear huésped
                var guestId = await _guestRepository.CreateAsync(hotelId, dto, createdBy);

                return (true, "Huésped creado exitosamente", guestId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear huésped: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Actualizar un huésped existente
        /// </summary>
        public async Task<(bool success, string message)> UpdateGuestAsync(
            Guid hotelId,
            UpdateGuestDto dto)
        {
            try
            {
                // Verificar que existe
                var existing = await _guestRepository.GetByIdAsync(hotelId, dto.GuestId);
                if (existing == null)
                {
                    return (false, "Huésped no encontrado");
                }

                // Validar duplicados (excluyendo este mismo huésped)
                var duplicateId = await _guestRepository.CheckDuplicateAsync(
                    hotelId,
                    dto.Phone,
                    dto.Email,
                    dto.GuestId
                );

                if (duplicateId.HasValue)
                {
                    return (false, "Ya existe otro huésped con el mismo teléfono o email");
                }

                // Normalizar datos
                dto.FirstName = NormalizeText(dto.FirstName);
                dto.LastName = NormalizeText(dto.LastName);
                dto.Phone = NormalizePhone(dto.Phone);

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    dto.Email = dto.Email.Trim().ToLowerInvariant();
                }

                if (!string.IsNullOrWhiteSpace(dto.Curp))
                {
                    dto.Curp = dto.Curp.ToUpperInvariant();
                }

                if (!string.IsNullOrWhiteSpace(dto.BillingRfc))
                {
                    dto.BillingRfc = dto.BillingRfc.ToUpperInvariant();
                }

                // Actualizar
                var success = await _guestRepository.UpdateAsync(hotelId, dto);

                return success
                    ? (true, "Huésped actualizado exitosamente")
                    : (false, "No se pudo actualizar el huésped");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar huésped: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtener huésped por ID
        /// </summary>
        public async Task<GuestDto?> GetGuestByIdAsync(Guid hotelId, Guid guestId)
        {
            return await _guestRepository.GetByIdAsync(hotelId, guestId);
        }

        /// <summary>
        /// Obtener lista paginada de huéspedes
        /// </summary>
        public async Task<(IEnumerable<GuestListDto> guests, int totalCount)> GetGuestsAsync(
            Guid hotelId,
            string? search = null,
            bool? isVip = null,
            bool? isBlacklisted = null,
            string? source = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created_at",
            string sortOrder = "desc")
        {
            return await _guestRepository.GetListAsync(
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
        }

        /// <summary>
        /// Búsqueda rápida para autocomplete
        /// </summary>
        public async Task<IEnumerable<GuestSearchDto>> SearchGuestsAsync(
            Guid hotelId,
            string query,
            int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<GuestSearchDto>();
            }

            return await _guestRepository.SearchAsync(hotelId, query.Trim(), maxResults);
        }

        /// <summary>
        /// Marcar/desmarcar huésped en blacklist
        /// </summary>
        public async Task<(bool success, string message)> ToggleBlacklistAsync(
            Guid hotelId,
            Guid guestId,
            bool isBlacklisted,
            string? reason = null)
        {
            try
            {
                // Validar que existe
                var guest = await _guestRepository.GetByIdAsync(hotelId, guestId);
                if (guest == null)
                {
                    return (false, "Huésped no encontrado");
                }

                // Si se está marcando como blacklist, la razón es obligatoria
                if (isBlacklisted && string.IsNullOrWhiteSpace(reason))
                {
                    return (false, "Debe proporcionar una razón para agregar a la lista negra");
                }

                var success = await _guestRepository.ToggleBlacklistAsync(
                    hotelId,
                    guestId,
                    isBlacklisted,
                    reason
                );

                if (success)
                {
                    var message = isBlacklisted
                        ? "Huésped agregado a lista negra"
                        : "Huésped removido de lista negra";
                    return (true, message);
                }

                return (false, "No se pudo actualizar el estado del huésped");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar blacklist: {ex.Message}");
            }
        }

        /// <summary>
        /// Eliminar huésped (soft delete)
        /// </summary>
        public async Task<(bool success, string message)> DeleteGuestAsync(
            Guid hotelId,
            Guid guestId)
        {
            try
            {
                // Validar que existe
                var guest = await _guestRepository.GetByIdAsync(hotelId, guestId);
                if (guest == null)
                {
                    return (false, "Huésped no encontrado");
                }

                // El stored procedure validará que no tenga reservas activas
                var success = await _guestRepository.DeleteAsync(hotelId, guestId);

                return success
                    ? (true, "Huésped eliminado exitosamente")
                    : (false, "No se puede eliminar el huésped porque tiene reservas activas");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar huésped: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODOS AUXILIARES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Normalizar texto: trim, capitalizar primera letra de cada palabra
        /// </summary>
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Trim();

            // Capitalizar primera letra de cada palabra
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0]) +
                               (words[i].Length > 1 ? words[i].Substring(1).ToLowerInvariant() : "");
                }
            }

            return string.Join(" ", words);
        }

        /// <summary>
        /// Normalizar teléfono: remover espacios, guiones, paréntesis
        /// </summary>
        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return phone.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");
        }
    }
}