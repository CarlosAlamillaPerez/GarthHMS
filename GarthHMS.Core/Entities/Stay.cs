using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Estancia activa (cuando ya hicieron check-in)
    /// Es la materialización de una reserva
    /// </summary>
    public class Stay
    {
        public int StayId { get; set; }
        public int HotelId { get; set; }
        public int ReservationId { get; set; }
        public int RoomId { get; set; }
        public int GuestId { get; set; }

        // FOLIO
        public string StayFolio { get; set; } = string.Empty;  // "STY-2025-00001"

        // FECHAS REALES
        public DateTime ActualCheckInDate { get; set; }
        public DateTime? ActualCheckOutDate { get; set; }
        public DateTime ExpectedCheckOutDate { get; set; }

        // HUÉSPEDES
        public int TotalGuests { get; set; } = 1;
        public int PetsCount { get; set; } = 0;
        public bool HasParking { get; set; } = false;

        // ESTADO
        public bool IsActive { get; set; } = true;
        public bool IsLateCheckout { get; set; } = false;

        // CARGOS EXTRAS
        public decimal ExtraChargesAmount { get; set; } = 0;

        // NOTAS
        public string? Notes { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}