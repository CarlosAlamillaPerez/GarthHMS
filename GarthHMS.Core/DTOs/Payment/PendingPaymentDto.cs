// GarthHMS.Core/DTOs/Payment/PendingPaymentDto.cs
using System;

namespace GarthHMS.Core.DTOs.Payment
{
    public class PendingPaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid ReservationId { get; set; }
        public string Folio { get; set; } = "";
        public string ReservationStatus { get; set; } = "";
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        // Huésped
        public string GuestFirstName { get; set; } = "";
        public string GuestLastName { get; set; } = "";
        public string? GuestPhone { get; set; }
        public string GuestFullName => $"{GuestFirstName} {GuestLastName}".Trim();

        // Pago
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentType { get; set; } = "";
        public string? Reference { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid RegisteredBy { get; set; }

        // Reserva financiero
        public decimal BalancePending { get; set; }
        public decimal Total { get; set; }

        // Labels para la vista
        public string MethodLabel => PaymentMethod switch
        {
            "transfer" => "Transferencia",
            "card" => "Tarjeta",
            "cash" => "Efectivo",
            _ => PaymentMethod
        };

        public string PaymentTypeLabel => PaymentType switch
        {
            "deposit" => "Anticipo",
            "balance" => "Abono",
            "full" => "Pago completo",
            _ => PaymentType
        };
    }
}