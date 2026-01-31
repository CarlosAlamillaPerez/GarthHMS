namespace GarthHMS.Core.DTOs.RoomType
{
    public class RoomTypeResponseDto
    {
        public Guid RoomTypeId { get; set; }
        public Guid HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BaseCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public decimal BasePriceNightly { get; set; }
        public decimal BasePriceHourly { get; set; }
        public decimal ExtraPersonCharge { get; set; }
        public bool AllowsPets { get; set; }
        public decimal PetCharge { get; set; }
        public decimal? SizeSqm { get; set; }
        public string? BedType { get; set; }
        public string? ViewType { get; set; }
        public List<string> Amenities { get; set; } = new();
        public List<string> PhotoUrls { get; set; } = new();
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Propiedades calculadas
        public string DisplayName => $"{Code} - {Name}";

        public string CapacityDisplay => BaseCapacity == MaxCapacity
            ? $"{BaseCapacity} persona{(BaseCapacity > 1 ? "s" : "")}"
            : $"{BaseCapacity}-{MaxCapacity} personas";

        public string StatusDisplay => IsActive ? "Activo" : "Inactivo";

        public string StatusBadgeClass => IsActive ? "success" : "secondary";
    }
}