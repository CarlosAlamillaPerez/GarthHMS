// GarthHMS.Core/DTOs/Room/CreateRoomDto.cs
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.Room
{
    /// <summary>
    /// DTO para crear una nueva habitación
    /// </summary>
    public class CreateRoomDto
    {
        [Required(ErrorMessage = "El tipo de habitación es requerido")]
        public Guid RoomTypeId { get; set; }

        [Required(ErrorMessage = "El número de habitación es requerido")]
        [StringLength(10, ErrorMessage = "El número de habitación no puede exceder 10 caracteres")]
        public string RoomNumber { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "El piso debe estar entre 0 y 100")]
        public int Floor { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notes { get; set; }
    }
}