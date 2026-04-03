// GarthHMS.Infrastructure/Repositories/PaymentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Payment;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly Procedimientos _procedimientos;

        public PaymentRepository(Procedimientos procedimientos)
        {
            _procedimientos = procedimientos;
        }

        // ────────────────────────────────────────────────────────────────────
        // LISTAR PENDIENTES DE VERIFICACIÓN
        // ────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PendingPaymentDto>> GetPendingVerificationAsync(Guid hotelId)
        {
            var rows = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_payments_get_pending_verification",
                new { p_hotel_id = hotelId });

            var list = new List<PendingPaymentDto>();
            if (rows == null) return list;

            foreach (var row in rows)
            {
                list.Add(new PendingPaymentDto
                {
                    PaymentId = (Guid)row.payment_id,
                    ReservationId = (Guid)row.reservation_id,
                    Folio = row.folio?.ToString() ?? "",
                    ReservationStatus = row.reservation_status?.ToString() ?? "",
                    CheckInDate = ((DateOnly)row.check_in_date).ToDateTime(TimeOnly.MinValue),
                    CheckOutDate = ((DateOnly)row.check_out_date).ToDateTime(TimeOnly.MinValue),
                    GuestFirstName = row.guest_first_name?.ToString() ?? "",
                    GuestLastName = row.guest_last_name?.ToString() ?? "",
                    GuestPhone = row.guest_phone?.ToString(),
                    Amount = row.amount ?? 0m,
                    PaymentMethod = row.payment_method?.ToString() ?? "",
                    PaymentType = row.payment_type?.ToString() ?? "",
                    Reference = row.reference?.ToString(),
                    PaymentDate = row.payment_date ?? DateTime.UtcNow,
                    RegisteredBy = row.registered_by != null ? (Guid?)row.registered_by : null,
                    BalancePending = row.balance_pending ?? 0m,
                    Total = row.total ?? 0m,
                });
            }

            return list;
        }

        // ────────────────────────────────────────────────────────────────────
        // VERIFICAR PAGO
        // ────────────────────────────────────────────────────────────────────

        public async Task<(bool Success, bool NewHasUnverified)> VerifyPaymentAsync(
            Guid hotelId,
            Guid paymentId,
            Guid verifiedBy)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_payment_verify",
                new
                {
                    p_hotel_id = hotelId,
                    p_payment_id = paymentId,
                    p_verified_by = verifiedBy
                });

            if (result == null)
                return (false, false);

            return (
                (bool)(result.success ?? false),
                (bool)(result.new_has_unverified ?? false)
            );
        }

        public async Task<IEnumerable<PendingPaymentDto>> GetVerifiedAsync(Guid hotelId)
        {
            var rows = await _procedimientos.EjecutarListaAsync<dynamic>(
                "sp_payments_get_verified",
                new { p_hotel_id = hotelId });

            var list = new List<PendingPaymentDto>();
            if (rows == null) return list;

            foreach (var row in rows)
            {
                list.Add(new PendingPaymentDto
                {
                    PaymentId = (Guid)row.payment_id,
                    ReservationId = (Guid)row.reservation_id,
                    Folio = row.folio?.ToString() ?? "",
                    ReservationStatus = row.reservation_status?.ToString() ?? "",
                    CheckInDate = ((DateOnly)row.check_in_date).ToDateTime(TimeOnly.MinValue),
                    CheckOutDate = ((DateOnly)row.check_out_date).ToDateTime(TimeOnly.MinValue),
                    GuestFirstName = row.guest_first_name?.ToString() ?? "",
                    GuestLastName = row.guest_last_name?.ToString() ?? "",
                    GuestPhone = row.guest_phone?.ToString(),
                    Amount = row.amount ?? 0m,
                    PaymentMethod = row.payment_method?.ToString() ?? "",
                    PaymentType = row.payment_type?.ToString() ?? "",
                    Reference = row.reference?.ToString(),
                    PaymentDate = row.payment_date ?? DateTime.UtcNow,
                    VerifiedAt = row.verified_at,
                    RegisteredBy = null,
                    BalancePending = row.balance_pending ?? 0m,
                    Total = row.total ?? 0m
                });
            }

            return list;
        }

        public async Task<(int VerifiedCount, decimal TotalAmount)> VerifyBulkAsync(
            Guid hotelId, string method, Guid verifiedBy)
        {
            var result = await _procedimientos.EjecutarUnicoAsync<dynamic>(
                "sp_payments_verify_bulk",
                new
                {
                    p_hotel_id = hotelId,
                    p_method = method,
                    p_verified_by = verifiedBy
                });

            if (result == null) return (0, 0m);

            return (
                result.verified_count != null ? (int)result.verified_count : 0,
                result.total_amount != null ? (decimal)result.total_amount : 0m
            );
        }

    }
}