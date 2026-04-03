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

        public async Task<IEnumerable<PendingPaymentDto>> GetVerifiedAsync(Guid hotelId)
        {
            try
            {
                if (hotelId == Guid.Empty) return Array.Empty<PendingPaymentDto>();
                return await _paymentRepository.GetVerifiedAsync(hotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos verificados | Hotel: {HotelId}", hotelId);
                return Array.Empty<PendingPaymentDto>();
            }
        }

        public async Task<ServiceResult<(int VerifiedCount, decimal TotalAmount)>> VerifyBulkAsync(
            Guid hotelId, string method, Guid verifiedBy, bool isManagerOrAdmin)
        {
            try
            {
                if (!isManagerOrAdmin)
                    return ServiceResult<(int, decimal)>.Failure("Solo Gerentes y Administradores pueden verificar pagos");

                if (hotelId == Guid.Empty)
                    return ServiceResult<(int, decimal)>.Failure("Hotel no identificado");

                var validMethods = new[] { "transfer", "card" };
                if (!validMethods.Contains(method))
                    return ServiceResult<(int, decimal)>.Failure("Método de pago inválido");

                var result = await _paymentRepository.VerifyBulkAsync(hotelId, method, verifiedBy);
                return ServiceResult<(int, decimal)>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en verificación masiva | Hotel: {HotelId} | Método: {Method}", hotelId, method);
                return ServiceResult<(int, decimal)>.Failure("Error al verificar los pagos");
            }
        }
    }
}