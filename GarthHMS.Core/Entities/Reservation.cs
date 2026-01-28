using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Reservación maestra (puede incluir múltiples habitaciones)
    /// </summary>
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int HotelId { get; set; }
        public int GuestId { get; set; }

        // FOLIO
        public string FolioNumber { get; set; } = string.Empty;  // AUTO: "RSV-2025-00001"

        // FECHAS
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalNights { get; set; }  // Calculado

        // HABITACIONES
        public int TotalRooms { get; set; } = 1;
        public int TotalGuests { get; set; } = 1;

        // FINANCIERO
        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; } = 0;
        public decimal PendingAmount { get; set; }

        // DESCUENTO
        public decimal? DiscountPercent { get; set; }
        public string? DiscountReason { get; set; }
        public int? ApprovedBy { get; set; }

        // ESTADO
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // ORIGEN
        public string ReservationSource { get; set; } = "Direct";  // "Direct", "Phone", "WhatsApp", "Booking.com"

        // EXTRAS
        public bool HasParking { get; set; } = false;
        public int PetsCount { get; set; } = 0;

        // NOTAS
        public string? SpecialRequests { get; set; }
        public string? InternalNotes { get; set; }

        // CANCELACIÓN
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public int? CancelledBy { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
