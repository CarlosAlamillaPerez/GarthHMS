using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.RoomType
{
    public class CreateRoomTypeDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La capacidad base es requerida")]
        [Range(1, 20, ErrorMessage = "La capacidad base debe estar entre 1 y 20")]
        public int BaseCapacity { get; set; }

        [Required(ErrorMessage = "La capacidad máxima es requerida")]
        [Range(1, 20, ErrorMessage = "La capacidad máxima debe estar entre 1 y 20")]
        public int MaxCapacity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio por noche no puede ser negativo")]
        public decimal BasePriceNightly { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio por hora no puede ser negativo")]
        public decimal BasePriceHourly { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El cargo por persona extra no puede ser negativo")]
        public decimal ExtraPersonCharge { get; set; }

        public bool AllowsPets { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El cargo por mascota no puede ser negativo")]
        public decimal PetCharge { get; set; }

        [Range(0, 500, ErrorMessage = "El tamaño debe estar entre 0 y 500 m²")]
        public decimal? SizeSqm { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de cama no puede exceder 50 caracteres")]
        public string? BedType { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de vista no puede exceder 50 caracteres")]
        public string? ViewType { get; set; }

        public List<string> Amenities { get; set; } = new();
        public List<string> PhotoUrls { get; set; } = new();
        public int DisplayOrder { get; set; }
    }
}