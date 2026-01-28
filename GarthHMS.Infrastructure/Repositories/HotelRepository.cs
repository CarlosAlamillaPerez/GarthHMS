using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de hoteles
    /// </summary>
    public class HotelRepository : IHotelRepository
    {
        private readonly Procedimientos _procedimientos;

        public HotelRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // CRUD

        public async Task<Hotel?> GetByIdAsync(int hotelId)
        {
            return await _procedimientos.EjecutarUnicoAsync<Hotel>(
                "sp_hotel_get_by_id",
                new { p_hotel_id = hotelId }
            );
        }

        public async Task<int> CreateAsync(Hotel hotel)
        {
            var hotelId = await _procedimientos.EjecutarEscalarAsync<int>(
                "sp_hotel_create",
                new
                {
                    p_hotel_name = hotel.HotelName,
                    p_legal_name = hotel.LegalName,
                    p_rfc = hotel.RFC,
                    p_tax_regime = hotel.TaxRegime,
                    p_email = hotel.Email,
                    p_phone = hotel.Phone,
                    p_whatsapp = hotel.WhatsApp,
                    p_website = hotel.Website,
                    p_street = hotel.Street,
                    p_exterior_number = hotel.ExteriorNumber,
                    p_interior_number = hotel.InteriorNumber,
                    p_neighborhood = hotel.Neighborhood,
                    p_city = hotel.City,
                    p_state = hotel.State,
                    p_postal_code = hotel.PostalCode,
                    p_country = hotel.Country,
                    p_time_zone = hotel.TimeZone,
                    p_currency = hotel.Currency,
                    p_is_motel = hotel.IsMotel,
                    p_default_checkin_time = hotel.DefaultCheckInTime,
                    p_default_checkout_time = hotel.DefaultCheckOutTime,
                    p_max_pets_allowed = hotel.MaxPetsAllowed,
                    p_max_guests_per_room = hotel.MaxGuestsPerRoom,
                    p_requires_tax_invoice = hotel.RequiresTaxInvoice,
                    p_subscription_plan = hotel.SubscriptionPlan,
                    p_is_active = hotel.IsActive,
                    p_created_by = hotel.CreatedBy
                }
            );

            return hotelId;
        }

        public async Task<bool> UpdateAsync(Hotel hotel)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_hotel_update",
                new
                {
                    p_hotel_id = hotel.HotelId,
                    p_hotel_name = hotel.HotelName,
                    p_legal_name = hotel.LegalName,
                    p_rfc = hotel.RFC,
                    p_tax_regime = hotel.TaxRegime,
                    p_email = hotel.Email,
                    p_phone = hotel.Phone,
                    p_whatsapp = hotel.WhatsApp,
                    p_website = hotel.Website,
                    p_street = hotel.Street,
                    p_exterior_number = hotel.ExteriorNumber,
                    p_interior_number = hotel.InteriorNumber,
                    p_neighborhood = hotel.Neighborhood,
                    p_city = hotel.City,
                    p_state = hotel.State,
                    p_postal_code = hotel.PostalCode,
                    p_country = hotel.Country,
                    p_time_zone = hotel.TimeZone,
                    p_currency = hotel.Currency,
                    p_default_checkin_time = hotel.DefaultCheckInTime,
                    p_default_checkout_time = hotel.DefaultCheckOutTime,
                    p_max_pets_allowed = hotel.MaxPetsAllowed,
                    p_max_guests_per_room = hotel.MaxGuestsPerRoom,
                    p_requires_tax_invoice = hotel.RequiresTaxInvoice,
                    p_is_active = hotel.IsActive,
                    p_updated_by = hotel.UpdatedBy
                }
            );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int hotelId)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_hotel_delete",
                new { p_hotel_id = hotelId }
            );

            return rowsAffected > 0;
        }

        // CONSULTAS

        public async Task<List<Hotel>> GetAllAsync()
        {
            return await _procedimientos.EjecutarListaAsync<Hotel>(
                "sp_hotel_get_all",
                null
            );
        }

        public async Task<List<Hotel>> GetAllActiveAsync()
        {
            return await _procedimientos.EjecutarListaAsync<Hotel>(
                "sp_hotel_get_all_active",
                null
            );
        }

        public async Task<Hotel?> GetByRFCAsync(string rfc)
        {
            return await _procedimientos.EjecutarUnicoAsync<Hotel>(
                "sp_hotel_get_by_rfc",
                new { p_rfc = rfc }
            );
        }

        public async Task<bool> RFCExistsAsync(string rfc, int? excludeHotelId = null)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_hotel_rfc_exists",
                new
                {
                    p_rfc = rfc,
                    p_exclude_hotel_id = excludeHotelId
                }
            );

            return result;
        }

        // CONFIGURACIÓN

        public async Task<bool> UpdateSettingsAsync(int hotelId, string settingsJson)
        {
            var rowsAffected = await _procedimientos.EjecutarAsync(
                "sp_hotel_update_settings",
                new
                {
                    p_hotel_id = hotelId,
                    p_settings_json = settingsJson
                }
            );

            return rowsAffected > 0;
        }

        public async Task<string?> GetSettingsAsync(int hotelId)
        {
            return await _procedimientos.EjecutarEscalarAsync<string>(
                "sp_hotel_get_settings",
                new { p_hotel_id = hotelId }
            );
        }
    }
}
