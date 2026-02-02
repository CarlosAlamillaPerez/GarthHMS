// GarthHMS.Core/DTOs/Room/RoomResponseDto.cs
using System;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.DTOs.Room
{
    /// <summary>
    /// DTO para respuestas de habitación al frontend
    /// Incluye datos enriquecidos con información del tipo de habitación
    /// </summary>
    public class RoomResponseDto
    {
        public Guid RoomId { get; set; }
        public Guid HotelId { get; set; }
        public Guid RoomTypeId { get; set; }

        // Datos básicos
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }

        // Estado
        public RoomStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime StatusChangedAt { get; set; }
        public Guid? StatusChangedBy { get; set; }

        // Estancia actual
        public Guid? CurrentStayId { get; set; }
        public bool IsOccupied => CurrentStayId.HasValue;

        // Control
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        // Datos del tipo (para mostrar en tabla)
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomTypeCode { get; set; } = string.Empty;

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Propiedades calculadas para UI
        public string DisplayName => $"{RoomNumber} ({RoomTypeCode})";
        public string FloorDisplay => Floor == 0 ? "PB" : Floor.ToString();

        public string StatusBadgeClass => Status switch
        {
            RoomStatus.Available => "badge bg-success",
            RoomStatus.Occupied => "badge bg-danger",
            RoomStatus.Dirty => "badge bg-warning",
            RoomStatus.Cleaning => "badge bg-info",
            RoomStatus.Maintenance => "badge bg-secondary",
            RoomStatus.Reserved => "badge bg-primary",
            _ => "badge bg-light"
        };

        public string StatusIcon => Status switch
        {
            RoomStatus.Available => "fa-check-circle",
            RoomStatus.Occupied => "fa-user",
            RoomStatus.Dirty => "fa-broom",
            RoomStatus.Cleaning => "fa-spray-can",
            RoomStatus.Maintenance => "fa-wrench",
            RoomStatus.Reserved => "fa-calendar-check",
            _ => "fa-question-circle"
        };
    }
}