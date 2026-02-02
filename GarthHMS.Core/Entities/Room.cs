// GarthHMS.Core/Entities/Room.cs
using System;
using GarthHMS.Core.Enums;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Habitación física específica (Ej: Habitación 101)
    /// Tabla: room
    /// </summary>
    public class Room
    {
        #region Identificación

        public Guid RoomId { get; set; }
        public Guid HotelId { get; set; }
        public Guid RoomTypeId { get; set; }

        #endregion

        #region Datos de la Habitación

        public string RoomNumber { get; set; } = string.Empty;  // "101", "102", "201"
        public int Floor { get; set; } = 1;  // Piso (1, 2, 3, etc.)

        #endregion

        #region Estados

        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public DateTime StatusChangedAt { get; set; }
        public Guid? StatusChangedBy { get; set; }

        #endregion

        #region Conexión con Estancia

        public Guid? CurrentStayId { get; set; }  // FK a stay cuando está ocupada

        #endregion

        #region Control

        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        #endregion

        #region Auditoría

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        #endregion

        #region Constructor

        public Room()
        {
            RoomId = Guid.NewGuid();
            Status = RoomStatus.Available;
            StatusChangedAt = DateTime.UtcNow;
            IsActive = true;
            Floor = 1;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}