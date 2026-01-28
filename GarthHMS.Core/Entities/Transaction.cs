using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Transacción financiera (ingreso o egreso)
    /// </summary>
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int HotelId { get; set; }

        // RELACIONES
        public int? ReservationId { get; set; }
        public int? StayId { get; set; }
        public int? GuestId { get; set; }

        // TIPO Y MÉTODO
        public TransactionType TransactionType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // MONTO
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "MXN";

        // DESCRIPCIÓN
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }  // Número de referencia bancaria

        // COMPROBANTE
        public string? ReceiptPath { get; set; }
        public string? InvoiceUUID { get; set; }  // UUID del SAT si se facturó

        // ESTADO
        public bool IsVoided { get; set; } = false;
        public DateTime? VoidedAt { get; set; }
        public string? VoidReason { get; set; }
        public int? VoidedBy { get; set; }

        // AUDITORÍA
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
    }
}
