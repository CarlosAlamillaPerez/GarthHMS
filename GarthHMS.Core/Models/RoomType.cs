using System;
using System.Text.Json;

namespace GarthHMS.Core.Models
{
    /// <summary>
    /// Representa un tipo de habitación (Sencilla, Doble, Suite, etc.)
    /// Tabla: room_type
    /// </summary>
    public class RoomType
    {
        #region Identificación

        public Guid RoomTypeId { get; set; }
        public Guid HotelId { get; set; }

        #endregion

        #region Información Básica

        /// <summary>
        /// Nombre del tipo (ej: "Suite Presidencial")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Código corto único (ej: "SUI", "DOB")
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Descripción detallada del tipo de habitación
        /// </summary>
        public string Description { get; set; }

        #endregion

        #region Capacidad

        /// <summary>
        /// Capacidad estándar (número de personas incluidas en el precio base)
        /// </summary>
        public int BaseCapacity { get; set; }

        /// <summary>
        /// Capacidad máxima permitida (incluye personas extras con cargo)
        /// </summary>
        public int MaxCapacity { get; set; }

        #endregion

        #region Precios Base

        /// <summary>
        /// Precio base por noche (modo HOTEL)
        /// Requerido si operation_mode = 'hotel' o 'hybrid'
        /// </summary>
        public decimal BasePriceNightly { get; set; }

        /// <summary>
        /// Precio base por hora (modo MOTEL)
        /// Requerido si operation_mode = 'motel' o 'hybrid'
        /// </summary>
        public decimal BasePriceHourly { get; set; }

        #endregion

        #region Cargos Adicionales

        /// <summary>
        /// Cargo por persona extra (después de base_capacity)
        /// </summary>
        public decimal ExtraPersonCharge { get; set; }

        /// <summary>
        /// Indica si permite mascotas
        /// </summary>
        public bool AllowsPets { get; set; }

        /// <summary>
        /// Cargo por mascota (si allows_pets = true)
        /// </summary>
        public decimal PetCharge { get; set; }

        #endregion

        #region Características Físicas

        /// <summary>
        /// Tamaño en metros cuadrados
        /// </summary>
        public decimal? SizeSqm { get; set; }

        /// <summary>
        /// Tipo de cama (ej: "1 King", "2 Queen", "1 Matrimonial")
        /// </summary>
        public string BedType { get; set; }

        /// <summary>
        /// Tipo de vista (ej: "Al mar", "A la ciudad", "Interior")
        /// </summary>
        public string ViewType { get; set; }

        #endregion

        #region Amenidades y Fotos (JSONB)

        /// <summary>
        /// Amenidades en formato JSON
        /// Ejemplo: ["WiFi", "TV", "Aire acondicionado", "Minibar"]
        /// Almacenado como JSONB en PostgreSQL
        /// </summary>
        public string AmenitiesJson { get; set; }

        /// <summary>
        /// URLs de fotos en formato JSON
        /// Ejemplo: ["https://...", "https://..."]
        /// Almacenado como JSONB en PostgreSQL
        /// </summary>
        public string PhotoUrlsJson { get; set; }

        #endregion

        #region Control

        /// <summary>
        /// Orden de visualización (para ordenar en listados)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indica si el tipo está activo
        /// </summary>
        public bool IsActive { get; set; }

        #endregion

        #region Auditoría

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        #endregion

        #region Propiedades Calculadas (No en BD)

        /// <summary>
        /// Lista de amenidades deserializada (para facilitar el uso en C#)
        /// </summary>
        public List<string> Amenities
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AmenitiesJson))
                    return new List<string>();

                try
                {
                    return JsonSerializer.Deserialize<List<string>>(AmenitiesJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                AmenitiesJson = value != null && value.Any()
                    ? JsonSerializer.Serialize(value)
                    : "[]";
            }
        }

        /// <summary>
        /// Lista de URLs de fotos deserializada
        /// </summary>
        public List<string> PhotoUrls
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PhotoUrlsJson))
                    return new List<string>();

                try
                {
                    return JsonSerializer.Deserialize<List<string>>(PhotoUrlsJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                PhotoUrlsJson = value != null && value.Any()
                    ? JsonSerializer.Serialize(value)
                    : "[]";
            }
        }

        /// <summary>
        /// Nombre completo para mostrar (código + nombre)
        /// </summary>
        public string DisplayName => $"{Code} - {Name}";

        #endregion

        #region Constructor

        public RoomType()
        {
            RoomTypeId = Guid.NewGuid();
            IsActive = true;
            DisplayOrder = 0;
            ExtraPersonCharge = 0;
            AllowsPets = false;
            PetCharge = 0;
            BasePriceHourly = 0;
            AmenitiesJson = "[]";
            PhotoUrlsJson = "[]";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Métodos de Validación

        /// <summary>
        /// Valida que la capacidad máxima sea mayor o igual a la base
        /// </summary>
        public bool IsCapacityValid()
        {
            return MaxCapacity >= BaseCapacity && BaseCapacity > 0;
        }

        /// <summary>
        /// Valida que los precios sean válidos según el modo de operación
        /// </summary>
        public bool IsPriceValid(string operationMode)
        {
            switch (operationMode?.ToLower())
            {
                case "hotel":
                    return BasePriceNightly > 0;

                case "motel":
                    return BasePriceHourly > 0;

                case "hybrid":
                    return BasePriceNightly > 0 && BasePriceHourly > 0;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Valida que si permite mascotas, el cargo sea >= 0
        /// </summary>
        public bool IsPetChargeValid()
        {
            if (!AllowsPets)
                return true; // Si no permite mascotas, el cargo no importa

            return PetCharge >= 0;
        }

        #endregion
    }
}