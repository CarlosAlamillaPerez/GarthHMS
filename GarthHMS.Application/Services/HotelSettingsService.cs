using System;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.HotelSettings;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    public class HotelSettingsService : IHotelSettingsService
    {
        private readonly IHotelSettingsRepository _repository;
        private readonly ILogger<HotelSettingsService> _logger;

        public HotelSettingsService(
            IHotelSettingsRepository repository,
            ILogger<HotelSettingsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ServiceResult<HotelSettingsDto>> GetSettingsAsync(Guid hotelId)
        {
            try
            {
                var settings = await _repository.GetByHotelIdAsync(hotelId);

                if (settings == null)
                {
                    return ServiceResult<HotelSettingsDto>.Failure("No se encontró la configuración del hotel");
                }

                var dto = MapToDto(settings);
                return ServiceResult<HotelSettingsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración del hotel {HotelId}", hotelId);
                return ServiceResult<HotelSettingsDto>.Failure($"Error al obtener la configuración: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateSettingsAsync(Guid hotelId, UpdateHotelSettingsDto dto, Guid userId)
        {
            try
            {
                // Validar que exista la configuración
                var exists = await _repository.ExistsAsync(hotelId);
                if (!exists)
                {
                    return ServiceResult<bool>.Failure("No existe configuración para este hotel");
                }

                // Validaciones de negocio
                var validationResult = ValidateSettings(dto);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<bool>.Failure(validationResult.ErrorMessage);
                }

                // Mapear DTO a Entity
                var settings = MapToEntity(hotelId, dto, userId);

                // Actualizar en BD
                await _repository.UpdateAsync(settings);

                _logger.LogInformation("Configuración actualizada para hotel {HotelId} por usuario {UserId}", hotelId, userId);
                return ServiceResult<bool>.Success(true, "Configuración actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración del hotel {HotelId}", hotelId);
                return ServiceResult<bool>.Failure($"Error al actualizar la configuración: {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(Guid hotelId)
        {
            return await _repository.ExistsAsync(hotelId);
        }

        // ============================================================================
        // VALIDACIONES DE NEGOCIO
        // ============================================================================

        private (bool IsValid, string ErrorMessage) ValidateSettings(UpdateHotelSettingsDto dto)
        {
            // 1. Validar horarios
            if (dto.CheckInTime >= dto.CheckOutTime)
            {
                return (false, "La hora de check-in debe ser anterior a la hora de check-out");
            }

            if (dto.LateCheckoutTime <= dto.CheckOutTime)
            {
                return (false, "La hora de check-out tardío debe ser posterior a la hora de check-out normal");
            }

            // 2. Validar impuestos
            if (dto.ChargesTaxes)
            {
                if (dto.TaxIvaPercent < 0 || dto.TaxIvaPercent > 100)
                {
                    return (false, "El IVA debe estar entre 0 y 100%");
                }

                if (dto.TaxIshPercent < 0 || dto.TaxIshPercent > 100)
                {
                    return (false, "El ISH debe estar entre 0 y 100%");
                }
            }

            // 3. Validar facturación
            if (dto.CanInvoice)
            {
                if (string.IsNullOrWhiteSpace(dto.SatRfc))
                {
                    return (false, "El RFC es obligatorio si el hotel puede facturar");
                }

                if (string.IsNullOrWhiteSpace(dto.SatBusinessName))
                {
                    return (false, "La razón social es obligatoria si el hotel puede facturar");
                }

                if (string.IsNullOrWhiteSpace(dto.SatTaxRegime))
                {
                    return (false, "El régimen fiscal es obligatorio si el hotel puede facturar");
                }

                // Si puede facturar, DEBE cobrar impuestos
                if (!dto.ChargesTaxes)
                {
                    return (false, "Si el hotel puede facturar, debe cobrar impuestos");
                }
            }

            // 4. Validar colores (formato hexadecimal)
            if (!IsValidHexColor(dto.PrimaryColor))
            {
                return (false, "El color primario debe estar en formato hexadecimal (ej: #2BA49A)");
            }

            if (!IsValidHexColor(dto.SecondaryColor))
            {
                return (false, "El color secundario debe estar en formato hexadecimal (ej: #D9C9B6)");
            }

            return (true, string.Empty);
        }

        private bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
        }

        // ============================================================================
        // MAPPERS
        // ============================================================================

        private HotelSettingsDto MapToDto(HotelSettings settings)
        {
            return new HotelSettingsDto
            {
                HotelSettingsId = settings.HotelSettingsId,
                HotelId = settings.HotelId,

                // Información general
                HotelName = settings.HotelName,
                Address = settings.Address,
                City = settings.City,
                State = settings.State,
                PostalCode = settings.PostalCode,
                Country = settings.Country,
                Phone = settings.Phone,
                Email = settings.Email,
                Website = settings.Website,

                // Modo de operación
                OperationMode = settings.OperationMode,

                // Horarios
                CheckInTime = settings.CheckInTime,
                CheckOutTime = settings.CheckOutTime,
                LateCheckoutTime = settings.LateCheckoutTime,
                LateCheckoutCharge = settings.LateCheckoutCharge,

                // Políticas
                CancellationHours = settings.CancellationHours,
                CancellationPolicyText = settings.CancellationPolicyText,

                // Impuestos
                ChargesTaxes = settings.ChargesTaxes,
                TaxIvaPercent = settings.TaxIvaPercent,
                TaxIshPercent = settings.TaxIshPercent,

                // Anticipos
                MinDepositPercent = settings.MinDepositPercent,

                // Huéspedes
                RequireCompanionDetails = settings.RequireCompanionDetails,

                // Facturación
                CanInvoice = settings.CanInvoice,
                SatRfc = settings.SatRfc,
                SatBusinessName = settings.SatBusinessName,
                SatTaxRegime = settings.SatTaxRegime,

                // Branding
                LogoUrl = settings.LogoUrl,
                PrimaryColor = settings.PrimaryColor,
                SecondaryColor = settings.SecondaryColor,

                // Otros
                Timezone = settings.Timezone,

                // Audit
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt,
                UpdatedBy = settings.UpdatedBy
            };
        }

        private HotelSettings MapToEntity(Guid hotelId, UpdateHotelSettingsDto dto, Guid userId)
        {
            return new HotelSettings
            {
                HotelId = hotelId,

                // Información general
                HotelName = dto.HotelName.Trim(),
                Address = dto.Address?.Trim(),
                City = dto.City?.Trim(),
                State = dto.State?.Trim(),
                PostalCode = dto.PostalCode?.Trim(),
                Country = dto.Country?.Trim() ?? "México",
                Phone = dto.Phone?.Trim(),
                Email = dto.Email?.Trim(),
                Website = dto.Website?.Trim(),

                // Modo de operación
                OperationMode = dto.OperationMode,

                // Horarios
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                LateCheckoutTime = dto.LateCheckoutTime,
                LateCheckoutCharge = dto.LateCheckoutCharge,

                // Políticas
                CancellationHours = dto.CancellationHours,
                CancellationPolicyText = dto.CancellationPolicyText?.Trim(),

                // Impuestos
                ChargesTaxes = dto.ChargesTaxes,
                TaxIvaPercent = dto.TaxIvaPercent,
                TaxIshPercent = dto.TaxIshPercent,

                // Anticipos
                MinDepositPercent = dto.MinDepositPercent,

                // Huéspedes
                RequireCompanionDetails = dto.RequireCompanionDetails,

                // Facturación
                CanInvoice = dto.CanInvoice,
                SatRfc = dto.SatRfc?.Trim()?.ToUpper(),
                SatBusinessName = dto.SatBusinessName?.Trim(),
                SatTaxRegime = dto.SatTaxRegime?.Trim(),

                // Branding
                LogoUrl = dto.LogoUrl?.Trim(),
                PrimaryColor = dto.PrimaryColor.ToUpper(),
                SecondaryColor = dto.SecondaryColor.ToUpper(),

                // Otros
                Timezone = dto.Timezone,

                // Audit
                UpdatedBy = userId
            };
        }
    }
}