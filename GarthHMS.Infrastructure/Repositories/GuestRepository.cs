// GarthHMS.Infrastructure/Repositories/GuestRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Guest;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio de huéspedes - Usa stored procedures exclusivamente
    /// </summary>
    public class GuestRepository : IGuestRepository
    {
        private readonly Procedimientos _procedimientos;

        public GuestRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        /// <summary>
        /// Crear un nuevo huésped
        /// </summary>
        public async Task<Guid> CreateAsync(Guid hotelId, CreateGuestDto dto, Guid createdBy)
        {
            var guestId = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "guest_create",
                new
                {
                    // Hotel ID y usuario
                    p_hotel_id = hotelId,
                    p_created_by = createdBy,

                    // Datos básicos (obligatorios)
                    p_first_name = dto.FirstName,
                    p_last_name = dto.LastName,
                    p_phone = dto.Phone,

                    // Datos básicos (opcionales)
                    p_email = dto.Email,
                    p_phone_secondary = dto.PhoneSecondary,

                    // Identificación
                    p_id_type = dto.IdType,
                    p_id_number = dto.IdNumber,
                    p_id_photo_url = dto.IdPhotoUrl,
                    p_curp = dto.Curp,
                    p_birth_date = dto.BirthDate,

                    // Dirección
                    p_address_street = dto.AddressStreet,
                    p_address_city = dto.AddressCity,
                    p_address_state = dto.AddressState,
                    p_address_zip = dto.AddressZip,
                    p_address_country = dto.AddressCountry,

                    // Facturación
                    p_billing_rfc = dto.BillingRfc,
                    p_billing_business_name = dto.BillingBusinessName,
                    p_billing_email = dto.BillingEmail,
                    p_billing_address = dto.BillingAddress,
                    p_billing_zip = dto.BillingZip,
                    p_billing_tax_regime = dto.BillingTaxRegime,

                    // Preferencias
                    p_notes = dto.Notes,
                    p_is_vip = dto.IsVip,
                    p_is_blacklisted = dto.IsBlacklisted,
                    p_blacklist_reason = dto.BlacklistReason,

                    // Origen
                    p_source = dto.Source
                }
            );

            return guestId;
        }

        /// <summary>
        /// Actualizar un huésped existente
        /// </summary>
        public async Task<bool> UpdateAsync(Guid hotelId, UpdateGuestDto dto)
        {
            var filasAfectadas = await _procedimientos.EjecutarAsync(
                "guest_update",
                new
                {
                    // Identificadores
                    p_hotel_id = hotelId,
                    p_guest_id = dto.GuestId,

                    // Datos básicos (obligatorios)
                    p_first_name = dto.FirstName,
                    p_last_name = dto.LastName,
                    p_phone = dto.Phone,

                    // Datos básicos (opcionales)
                    p_email = dto.Email,
                    p_phone_secondary = dto.PhoneSecondary,

                    // Identificación
                    p_id_type = dto.IdType,
                    p_id_number = dto.IdNumber,
                    p_id_photo_url = dto.IdPhotoUrl,
                    p_curp = dto.Curp,
                    p_birth_date = dto.BirthDate,

                    // Dirección
                    p_address_street = dto.AddressStreet,
                    p_address_city = dto.AddressCity,
                    p_address_state = dto.AddressState,
                    p_address_zip = dto.AddressZip,
                    p_address_country = dto.AddressCountry,

                    // Facturación
                    p_billing_rfc = dto.BillingRfc,
                    p_billing_business_name = dto.BillingBusinessName,
                    p_billing_email = dto.BillingEmail,
                    p_billing_address = dto.BillingAddress,
                    p_billing_zip = dto.BillingZip,
                    p_billing_tax_regime = dto.BillingTaxRegime,

                    // Preferencias
                    p_notes = dto.Notes,
                    p_is_vip = dto.IsVip,
                    p_is_blacklisted = dto.IsBlacklisted,
                    p_blacklist_reason = dto.BlacklistReason,

                    // Origen
                    p_source = dto.Source
                }
            );

            return filasAfectadas > 0;
        }

        /// <summary>
        /// Obtener huésped por ID
        /// </summary>
        public async Task<GuestDto?> GetByIdAsync(Guid hotelId, Guid guestId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<GuestDto>(
                "guest_get_by_id",
                new
                {
                    p_hotel_id = hotelId,
                    p_guest_id = guestId
                }
            );

            return result;
        }

        /// <summary>
        /// Obtener lista paginada de huéspedes con filtros
        /// </summary>
        public async Task<(IEnumerable<GuestListDto> guests, int totalCount)> GetListAsync(
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
            var guests = await _procedimientos.EjecutarListaAsync<GuestListDto>(
                "guest_get_list",
                new
                {
                    p_hotel_id = hotelId,
                    p_search = search,
                    p_is_vip = isVip,
                    p_is_blacklisted = isBlacklisted,
                    p_source = source,
                    p_page_number = pageNumber,
                    p_page_size = pageSize,
                    p_sort_by = sortBy,
                    p_sort_order = sortOrder
                }
            );

            var totalCount = guests.Any() ? guests.First().TotalCount : 0;

            return (guests, totalCount);
        }

        /// <summary>
        /// Búsqueda rápida para autocomplete
        /// </summary>
        public async Task<IEnumerable<GuestSearchDto>> SearchAsync(Guid hotelId, string query, int maxResults = 10)
        {
            return await _procedimientos.EjecutarListaAsync<GuestSearchDto>(
                "guest_search",
                new
                {
                    p_hotel_id = hotelId,
                    p_query = query,
                    p_max_results = maxResults
                }
            );
        }

        /// <summary>
        /// Verificar si existe un huésped con el mismo teléfono o email
        /// </summary>
        public async Task<Guid?> CheckDuplicateAsync(Guid hotelId, string phone, string? email = null, Guid? excludeGuestId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<Guid?>(
                "guest_check_duplicate",
                new
                {
                    p_hotel_id = hotelId,
                    p_phone = phone,
                    p_email = email,
                    p_exclude_guest_id = excludeGuestId
                }
            );

            return result == Guid.Empty ? null : result;
        }

        /// <summary>
        /// Marcar/desmarcar huésped en blacklist
        /// </summary>
        public async Task<bool> ToggleBlacklistAsync(Guid hotelId, Guid guestId, bool isBlacklisted, string? reason = null)
        {
            var filasAfectadas = await _procedimientos.EjecutarAsync(
                "guest_toggle_blacklist",
                new
                {
                    p_hotel_id = hotelId,
                    p_guest_id = guestId,
                    p_is_blacklisted = isBlacklisted,
                    p_blacklist_reason = reason
                }
            );

            return filasAfectadas > 0;
        }

        /// <summary>
        /// Eliminar huésped (soft delete)
        /// Solo si no tiene reservas activas
        /// </summary>
        public async Task<bool> DeleteAsync(Guid hotelId, Guid guestId)
        {
            var filasAfectadas = await _procedimientos.EjecutarAsync(
                "guest_delete",
                new
                {
                    p_hotel_id = hotelId,
                    p_guest_id = guestId
                }
            );

            return filasAfectadas > 0;
        }
    }
}