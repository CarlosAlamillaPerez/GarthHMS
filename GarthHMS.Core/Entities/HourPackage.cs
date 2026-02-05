// GarthHMS.Core/Entities/HourPackage.cs
using System;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Entidad para paquetes de horas (modo motel)
    /// Representa bloques de tiempo con precios fijos
    /// </summary>
    public class HourPackage
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

        /// <summary>
        /// Nombre del paquete (ej: "Paquete 3 horas")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Duración en horas (ej: 3, 6, 12, 24)
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Precio del paquete completo
        /// </summary>
        public decimal Price { get; set; }

        // ====================================================================
        // CONFIGURACIÓN DE EXTRAS
        // ====================================================================

        /// <summary>
        /// Precio por cada hora extra adicional
        /// </summary>
        public decimal ExtraHourPrice { get; set; }

        /// <summary>
        /// Indica si se permiten extensiones de tiempo
        /// </summary>
        public bool AllowsExtensions { get; set; }

        // ====================================================================
        // CONTROL
        // ====================================================================

        /// <summary>
        /// Orden de visualización en listas
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Estado del paquete (soft delete)
        /// </summary>
        public bool IsActive { get; set; }

        // ====================================================================
        // TIMESTAMPS
        // ====================================================================

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}