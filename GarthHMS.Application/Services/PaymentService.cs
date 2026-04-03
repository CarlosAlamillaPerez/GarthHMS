// GarthHMS.Application/Services/PaymentService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Payment;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        // ────────────────────────────────────────────────────────────────────
        // LISTAR PENDIENTES DE VERIFICACIÓN
        // ────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PendingPaymentDto>> GetPendingVerificationAsync(Guid hotelId)
        {
            try
            {
                if (hotelId == Guid.Empty)
                    return Array.Empty<PendingPaymentDto>();

                return await _paymentRepository.GetPendingVerificationAsync(hotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos pendientes de verificación | Hotel: {HotelId}", hotelId);
                return Array.Empty<PendingPaymentDto>();
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // VERIFICAR PAGO
        // ────────────────────────────────────────────────────────────────────

        public async Task<ServiceResult<bool>> VerifyPaymentAsync(
            Guid hotelId,
            Guid paymentId,
            Guid verifiedBy,
            bool isManagerOrAdmin)
        {
            try
            {
                if (hotelId == Guid.Empty)
                    return ServiceResult<bool>.Failure("Hotel no identificado");

                if (paymentId == Guid.Empty)
                    return ServiceResult<bool>.Failure("Pago no identificado");

                if (!isManagerOrAdmin)
                    return ServiceResult<bool>.Failure("Solo Gerentes y Administradores pueden verificar pagos");

                var (success, _) = await _paymentRepository.VerifyPaymentAsync(hotelId, paymentId, verifiedBy);

                if (!success)
                    return ServiceResult<bool>.Failure("No se encontró el pago o ya fue verificado anteriormente");

                return ServiceResult<bool>.Success(true, "Pago verificado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar pago {PaymentId}", paymentId);
                return ServiceResult<bool>.Failure("Error al verificar el pago");
            }
        }
    }
}