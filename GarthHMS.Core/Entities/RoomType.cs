using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GarthHMS.Core.Entities
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

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        #endregion

        #region Capacidad

        public int BaseCapacity { get; set; }
        public int MaxCapacity { get; set; }

        #endregion

        #region Precios Base

        public decimal BasePriceNightly { get; set; }
        public decimal BasePriceHourly { get; set; }

        #endregion

        #region Cargos Adicionales

        public decimal ExtraPersonCharge { get; set; }
        public bool AllowsPets { get; set; }
        public decimal PetCharge { get; set; }

        #endregion

        #region Características Físicas

        public decimal? SizeSqm { get; set; }
        public string? BedType { get; set; }
        public string? ViewType { get; set; }

        #endregion

        #region Amenidades y Fotos (JSONB)

        public string AmenitiesJson { get; set; } = "[]";
        public string PhotoUrlsJson { get; set; } = "[]";

        #endregion

        #region Control

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        #endregion

        #region Auditoría

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        #endregion

        #region Propiedades Calculadas

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
    }
}