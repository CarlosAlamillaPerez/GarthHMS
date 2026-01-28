using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Habitación específica dentro de una reservación
    /// Una reserva puede tener múltiples habitaciones
    /// </summary>
    public class ReservationRoom
    {
        public int ReservationRoomId { get; set; }
        public int ReservationId { get; set; }
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }

        // HUÉSPEDES ASIGNADOS
        public int GuestsCount { get; set; } = 1;
        public string? GuestNames { get; set; }  // "Juan Pérez, María López"

        // PRECIO
        public decimal RoomPrice { get; set; }  // Precio por noche de esta habitación
        public decimal TotalRoomAmount { get; set; }  // Precio total (noches * precio)

        // NOTAS
        public string? Notes { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}