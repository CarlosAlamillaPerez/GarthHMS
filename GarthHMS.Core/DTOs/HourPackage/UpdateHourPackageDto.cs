using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.HourPackage
{
    /// <summary>
    /// DTO para actualizar un paquete de horas existente
    /// </summary>
    public class UpdateHourPackageDto
    {
        [Required(ErrorMessage = "El ID del paquete es requerido")]
        public Guid HourPackageId { get; set; }

        [Required(ErrorMessage = "El nombre del paquete es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las horas son requeridas")]
        [Range(1, 48, ErrorMessage = "Las horas deben estar entre 1 y 48")]
        public int Hours { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El precio por hora extra no puede ser negativo")]
        public decimal ExtraHourPrice { get; set; }

        public bool AllowsExtensions { get; set; }

        [Range(0, 999, ErrorMessage = "El orden de visualización debe estar entre 0 y 999")]
        public int DisplayOrder { get; set; }
    }
}