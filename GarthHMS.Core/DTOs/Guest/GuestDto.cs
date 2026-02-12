// GarthHMS.Core/DTOs/Guest/GuestDto.cs
using System;

namespace GarthHMS.Core.DTOs.Guest
{
    /// <summary>
    /// DTO completo de huésped con todos los campos
    /// </summary>
    public class GuestDto
    {
        // Identificador
        public Guid GuestId { get; set; }
        public Guid HotelId { get; set; }

        // Datos básicos (obligatorios)
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Datos básicos (opcionales)
        public string? Email { get; set; }
        public string? PhoneSecondary { get; set; }

        // Identificación (capturada en check-in)
        public string? IdType { get; set; }
        public string? IdTypeText { get; set; }
        public string? IdNumber { get; set; }
        public string? IdPhotoUrl { get; set; }
        public string? Curp { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }

        // Dirección
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressState { get; set; }
        public string? AddressZip { get; set; }
        public string? AddressCountry { get; set; }

        // Facturación
        public string? BillingRfc { get; set; }
        public string? BillingBusinessName { get; set; }
        public string? BillingEmail { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingZip { get; set; }
        public string? BillingTaxRegime { get; set; }

        // Preferencias y notas
        public string? Notes { get; set; }
        public bool IsVip { get; set; }
        public bool IsBlacklisted { get; set; }
        public string? BlacklistReason { get; set; }

        // Estadísticas
        public int TotalStays { get; set; }
        public DateTime? LastStayDate { get; set; }

        // Origen
        public string Source { get; set; } = "direct";
        public string SourceText { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
    }
}