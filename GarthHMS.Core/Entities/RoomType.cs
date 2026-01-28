using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Tipo de habitación (Ej: Individual, Doble, Suite)
    /// </summary>
    public class RoomType
    {
        public int RoomTypeId { get; set; }
        public int HotelId { get; set; }

        public string TypeName { get; set; } = string.Empty;  // "Suite", "Doble", etc.
        public string? Description { get; set; }
        public int MaxOccupancy { get; set; } = 2;
        public int TotalRooms { get; set; } = 0;  // Cantidad de habitaciones de este tipo

        // PRECIOS BASE
        public decimal BasePrice { get; set; }  // Precio por noche/hora base
        public decimal? WeekendPrice { get; set; }  // Precio fin de semana

        // AMENIDADES (JSON array)
        public string? AmenitiesJson { get; set; } = "[]";  // ["WiFi", "TV", "A/C"]

        public bool IsActive { get; set; } = true;

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
