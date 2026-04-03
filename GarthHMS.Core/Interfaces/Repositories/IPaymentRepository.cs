// GarthHMS.Core/Interfaces/Repositories/IPaymentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Payment;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        /// <summary>Lista todos los pagos pendientes de verificación del hotel.</summary>
        Task<IEnumerable<PendingPaymentDto>> GetPendingVerificationAsync(Guid hotelId);

        /// <summary>Marca un pago como verificado. Retorna (success, newHasUnverified).</summary>
        Task<(bool Success, bool NewHasUnverified)> VerifyPaymentAsync(
            Guid hotelId,
            Guid paymentId,
            Guid verifiedBy);

        Task<IEnumerable<PendingPaymentDto>> GetVerifiedAsync(Guid hotelId);

        Task<(int VerifiedCount, decimal TotalAmount)> VerifyBulkAsync(
            Guid hotelId, string method, Guid verifiedBy);
    }
}