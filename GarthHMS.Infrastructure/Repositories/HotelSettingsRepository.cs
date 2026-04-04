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
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_hotel_settings_get_by_hotel_id",
                new { p_hotel_id = hotelId }
            );

            if (result == null) return null;

            return new HotelSettings
            {
                HotelSettingsId = result.hotel_settings_id,
                HotelId = result.hotel_id,
                HotelName = result.hotel_name,
                Address = result.address,
                City = result.city,
                State = result.state,
                PostalCode = result.postal_code,
                Country = result.country,
                Phone = result.phone,
                Email = result.email,
                Website = result.website,
                OperationMode = result.operation_mode,
                CompanionsOptional = result.companions_optional,
                CheckInTime = result.check_in_time?.ToString(),
                CheckOutTime = result.check_out_time?.ToString(),
                LateCheckoutTime = result.late_checkout_time?.ToString(),
                LateCheckoutCharge = result.late_checkout_charge,
                CancellationHours = result.cancellation_hours,
                CancellationPolicyText = result.cancellation_policy_text,
                CancellationPolicyType = result.cancellation_policy_type?.ToString() ?? "window",RefundPercentOnCancel = result.refund_percent_on_cancel != null ? (int)result.refund_percent_on_cancel : 0,RefundTiers = result.refund_tiers?.ToString(),
                NoShowChargePercent = result.no_show_charge_percent != null ? (int)result.no_show_charge_percent : 100,
                ChargesTaxes = result.charges_taxes,
                TaxIvaPercent = result.tax_iva_percent,
                TaxIshPercent = result.tax_ish_percent,
                MinDepositPercent = result.min_deposit_percent,
                RequireCompanionDetails = result.require_companion_details,
                CanInvoice = result.can_invoice,
                SatRfc = result.sat_rfc,
                SatBusinessName = result.sat_business_name,
                SatTaxRegime = result.sat_tax_regime,
                LogoUrl = result.logo_url,
                PrimaryColor = result.primary_color,
                SecondaryColor = result.secondary_color,
                Timezone = result.timezone,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at,
                UpdatedBy = result.updated_by,
                AutoVerifyCard = (bool)(result.auto_verify_card ?? false),
                AutoVerifyTransfer = (bool)(result.auto_verify_transfer ?? false),
                BlockCheckinIfBalance = (bool)(result.block_checkin_if_balance ?? false)
            };
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
                p_check_in_time = TimeOnly.Parse(settings.CheckInTime),
                p_check_out_time = TimeOnly.Parse(settings.CheckOutTime),
                p_late_checkout_time = TimeOnly.Parse(settings.LateCheckoutTime),
                p_late_checkout_charge = settings.LateCheckoutCharge,

                // Políticas
                p_cancellation_hours = settings.CancellationHours,
                p_cancellation_policy_text = settings.CancellationPolicyText,
                p_cancellation_policy_type = settings.CancellationPolicyType,
                p_refund_percent_on_cancel = settings.RefundPercentOnCancel,
                p_refund_tiers = settings.RefundTiers,
                p_no_show_charge_percent = settings.NoShowChargePercent,

                // Impuestos
                p_charges_taxes = settings.ChargesTaxes,
                p_tax_iva_percent = settings.TaxIvaPercent,
                p_tax_ish_percent = settings.TaxIshPercent,

                // Anticipos
                p_min_deposit_percent = settings.MinDepositPercent,

                // Huéspedes
                p_require_companion_details = settings.RequireCompanionDetails,
                p_companions_optional = settings.CompanionsOptional,

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
                p_check_in_time = TimeOnly.Parse(settings.CheckInTime),
                p_check_out_time = TimeOnly.Parse(settings.CheckOutTime),
                p_late_checkout_time = TimeOnly.Parse(settings.LateCheckoutTime),
                p_late_checkout_charge = settings.LateCheckoutCharge,

                // Políticas
                p_cancellation_hours = settings.CancellationHours,
                p_cancellation_policy_text = settings.CancellationPolicyText,
                p_cancellation_policy_type = settings.CancellationPolicyType,
                p_refund_percent_on_cancel = settings.RefundPercentOnCancel,
                p_refund_tiers = settings.RefundTiers,
                p_no_show_charge_percent = settings.NoShowChargePercent,

                // Impuestos
                p_charges_taxes = settings.ChargesTaxes,
                p_tax_iva_percent = settings.TaxIvaPercent,
                p_tax_ish_percent = settings.TaxIshPercent,

                // Anticipos
                p_min_deposit_percent = settings.MinDepositPercent,

                // Huéspedes
                p_require_companion_details = settings.RequireCompanionDetails,
                p_companions_optional = settings.CompanionsOptional,


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

        public async Task UpdateAutoVerifyAsync(Guid hotelId, bool autoVerifyCard, bool autoVerifyTransfer, Guid updatedBy)
        {
            await _procedimientos.EjecutarAsync(
                "sp_hotel_settings_update_auto_verify",
                new
                {
                    p_hotel_id = hotelId,
                    p_auto_verify_card = autoVerifyCard,
                    p_auto_verify_transfer = autoVerifyTransfer,
                    p_updated_by = updatedBy
                });
        }
    }
}