using System;
using System.Threading.Tasks;
using Dapper;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class HotelSettingsRepository : IHotelSettingsRepository
    {
        private readonly Procedimientos _procedimientos;
        private readonly BaseDeDatos _baseDeDatos;

        public HotelSettingsRepository(Procedimientos procedimientos, BaseDeDatos baseDeDatos)
        {
            _procedimientos = procedimientos;
            _baseDeDatos = baseDeDatos;
        }

        public async Task<HotelSettings?> GetByHotelIdAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<HotelSettings>(
                "sp_hotel_settings_get_by_hotel_id",
                new { p_hotel_id = hotelId }
            );

            return result;
        }

        public async Task<Guid> CreateAsync(HotelSettings settings)
        {
            var parameters = new
            {
                p_hotel_id = settings.HotelId,

                // Información general
                p_hotel_name = settings.HotelName,
                p_address = settings.Address,
                p_city = settings.City,
                p_state = settings.State,
                p_postal_code = settings.PostalCode,
                p_country = settings.Country,
                p_phone = settings.Phone,
                p_email = settings.Email,
                p_website = settings.Website,

                // Modo de operación
                p_operation_mode = settings.OperationMode,

                // Horarios
                p_check_in_time = settings.CheckInTime,
                p_check_out_time = settings.CheckOutTime,
                p_late_checkout_time = settings.LateCheckoutTime,
                p_late_checkout_charge = settings.LateCheckoutCharge,

                // Políticas
                p_cancellation_hours = settings.CancellationHours,
                p_cancellation_policy_text = settings.CancellationPolicyText,

                // Impuestos
                p_charges_taxes = settings.ChargesTaxes,
                p_tax_iva_percent = settings.TaxIvaPercent,
                p_tax_ish_percent = settings.TaxIshPercent,

                // Anticipos
                p_min_deposit_percent = settings.MinDepositPercent,

                // Huéspedes
                p_require_companion_details = settings.RequireCompanionDetails,

                // Facturación
                p_can_invoice = settings.CanInvoice,
                p_sat_rfc = settings.SatRfc,
                p_sat_business_name = settings.SatBusinessName,
                p_sat_tax_regime = settings.SatTaxRegime,

                // Branding
                p_logo_url = settings.LogoUrl,
                p_primary_color = settings.PrimaryColor,
                p_secondary_color = settings.SecondaryColor,

                // Otros
                p_timezone = settings.Timezone,

                // Audit
                p_updated_by = settings.UpdatedBy
            };

            var result = await _procedimientos.EjecutarEscalarAsync<Guid>(
                "sp_hotel_settings_create",
                parameters
            );

            return result;
        }

        public async Task UpdateAsync(HotelSettings settings)
        {
            var parameters = new
            {
                p_hotel_id = settings.HotelId,

                // Información general
                p_hotel_name = settings.HotelName,
                p_address = settings.Address,
                p_city = settings.City,
                p_state = settings.State,
                p_postal_code = settings.PostalCode,
                p_country = settings.Country,
                p_phone = settings.Phone,
                p_email = settings.Email,
                p_website = settings.Website,

                // Modo de operación
                p_operation_mode = settings.OperationMode,

                // Horarios
                p_check_in_time = settings.CheckInTime,
                p_check_out_time = settings.CheckOutTime,
                p_late_checkout_time = settings.LateCheckoutTime,
                p_late_checkout_charge = settings.LateCheckoutCharge,

                // Políticas
                p_cancellation_hours = settings.CancellationHours,
                p_cancellation_policy_text = settings.CancellationPolicyText,

                // Impuestos
                p_charges_taxes = settings.ChargesTaxes,
                p_tax_iva_percent = settings.TaxIvaPercent,
                p_tax_ish_percent = settings.TaxIshPercent,

                // Anticipos
                p_min_deposit_percent = settings.MinDepositPercent,

                // Huéspedes
                p_require_companion_details = settings.RequireCompanionDetails,

                // Facturación
                p_can_invoice = settings.CanInvoice,
                p_sat_rfc = settings.SatRfc,
                p_sat_business_name = settings.SatBusinessName,
                p_sat_tax_regime = settings.SatTaxRegime,

                // Branding
                p_logo_url = settings.LogoUrl,
                p_primary_color = settings.PrimaryColor,
                p_secondary_color = settings.SecondaryColor,

                // Otros
                p_timezone = settings.Timezone,

                // Audit
                p_updated_by = settings.UpdatedBy
            };

            await _procedimientos.EjecutarAsync(
                "sp_hotel_settings_update",
                parameters
            );
        }

        public async Task<bool> ExistsAsync(Guid hotelId)
        {
            var result = await _procedimientos.EjecutarEscalarAsync<bool>(
                "sp_hotel_settings_exists",
                new { p_hotel_id = hotelId }
            );

            return result;
        }
    }
}