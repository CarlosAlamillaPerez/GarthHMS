using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Habitación física específica (Ej: Habitación 101)
    /// </summary>
    public class Room
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public int RoomTypeId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;  // "101", "102A", etc.
        public string? Floor { get; set; }  // "1", "2", "PB"
        public string? Location { get; set; }  // "Ala Norte", "Torre B"

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        // CARACTERÍSTICAS ESPECÍFICAS
        public bool IsSmoking { get; set; } = false;
        public bool IsAccessible { get; set; } = false;
        public bool AllowsPets { get; set; } = false;

        // BLOQUEO
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }
        public DateTime? BlockedUntil { get; set; }

        // NOTAS
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
