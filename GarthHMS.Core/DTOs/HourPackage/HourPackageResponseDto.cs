using System;

namespace GarthHMS.Core.DTOs.HourPackage
{
    /// <summary>
    /// DTO para respuesta de paquete de horas con información completa
    /// </summary>
    public class HourPackageResponseDto
    {
        // ====================================================================
        // IDENTIFICADORES
        // ====================================================================

        public Guid HourPackageId { get; set; }
        public Guid HotelId { get; set; }
        public Guid RoomTypeId { get; set; }

        // ====================================================================
        // CONFIGURACIÓN DEL PAQUETE
        // ====================================================================

        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
        public decimal Price { get; set; }
        public decimal ExtraHourPrice { get; set; }
        public bool AllowsExtensions { get; set; }

        // ====================================================================
        // CONTROL
        // ====================================================================

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        // ====================================================================
        // INFORMACIÓN DEL TIPO DE HABITACIÓN (JOIN)
        // ====================================================================

        /// <summary>
        /// Nombre del tipo de habitación asociado
        /// </summary>
        public string? RoomTypeName { get; set; }

        /// <summary>
        /// Código del tipo de habitación
        /// </summary>
        public string? RoomTypeCode { get; set; }

        // ====================================================================
        // TIMESTAMPS
        // ====================================================================

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // ====================================================================
        // PROPIEDADES CALCULADAS
        // ====================================================================

        /// <summary>
        /// Precio por hora calculado
        /// </summary>
        public decimal PricePerHour => Hours > 0 ? Math.Round(Price / Hours, 2) : 0;

        /// <summary>
        /// Etiqueta completa del paquete (ej: "Suit - 3 horas - $450.00")
        /// </summary>
        public string FullLabel => $"{RoomTypeName} - {Hours}h - ${Price:N2}";
    }
}