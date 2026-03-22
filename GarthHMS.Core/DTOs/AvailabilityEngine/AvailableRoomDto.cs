// GarthHMS.Core/DTOs/AvailabilityEngine/AvailableRoomDto.cs
namespace GarthHMS.Core.DTOs.AvailabilityEngine
{
    /// <summary>
    /// Habitación disponible para un rango de fechas
    /// SP: sp_availability_get_available_rooms
    /// </summary>
    public class AvailableRoomDto
    {
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }

        // Tipo de habitación
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomTypeCode { get; set; } = string.Empty;
        public int BaseCapacity { get; set; }
        public int MaxCapacity { get; set; }

        // Precios
        public decimal BasePriceNightly { get; set; }
        public decimal ExtraPersonCharge { get; set; }
        public bool AllowsPets { get; set; }
        public decimal PetCharge { get; set; }

        // Descripción
        public string? BedType { get; set; }
        public string? ViewType { get; set; }
        public decimal? SizeSqm { get; set; }
        public string? Amenities { get; set; }
    }

    // ---------------------------------------------------------------------------

    /// <summary>
    /// Resumen de ocupación para el panel inferior del calendario
    /// SP: sp_availability_get_day_summary
    /// </summary>
    public class DaySummaryDto
    {
        public long TotalRooms { get; set; }
        public long OccupiedRooms { get; set; }
        public long AvailableRooms { get; set; }
        public long ReservedRooms { get; set; }
        public long MaintenanceRooms { get; set; }
        public decimal OccupancyPercent { get; set; }
    }

    // ---------------------------------------------------------------------------

    /// <summary>
    /// Query para verificar disponibilidad
    /// Usado como parámetro en AvailabilityService.GetAvailableRoomsAsync
    /// </summary>
    public class AvailabilityQueryDto
    {
        public Guid HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public Guid? RoomTypeId { get; set; }
    }
}