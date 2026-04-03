// GarthHMS.Core/Interfaces/Services/IPaymentService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Payment;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    public interface IPaymentService
    {
        /// <summary>Lista todos los pagos pendientes de verificación del hotel.</summary>
        Task<IEnumerable<PendingPaymentDto>> GetPendingVerificationAsync(Guid hotelId);

        /// <summary>Verifica un pago (solo Gerente/Administrador).</summary>
        Task<ServiceResult<bool>> VerifyPaymentAsync(
            Guid hotelId,
            Guid paymentId,
            Guid verifiedBy,
            bool isManagerOrAdmin);

        Task<IEnumerable<PendingPaymentDto>> GetVerifiedAsync(Guid hotelId);

        Task<ServiceResult<(int VerifiedCount, decimal TotalAmount)>> VerifyBulkAsync(
            Guid hotelId, string method, Guid verifiedBy, bool isManagerOrAdmin);
    }
}