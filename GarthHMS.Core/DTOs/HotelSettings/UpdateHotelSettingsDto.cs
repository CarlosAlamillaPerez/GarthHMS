using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.HotelSettings
{
    /// <summary>
    /// DTO para actualizar configuración del hotel
    /// </summary>
    public class UpdateHotelSettingsDto
    {
        // ===== INFORMACIÓN GENERAL =====
        [Required(ErrorMessage = "El nombre del hotel es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string HotelName { get; set; } = null!;

        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "El estado no puede exceder 100 caracteres")]
        public string? State { get; set; }

        [StringLength(10, ErrorMessage = "El código postal no puede exceder 10 caracteres")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        public string? Country { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }

        [Url(ErrorMessage = "Formato de URL inválido")]
        [StringLength(200, ErrorMessage = "El sitio web no puede exceder 200 caracteres")]
        public string? Website { get; set; }

        // ===== MODO DE OPERACIÓN =====
        [Required(ErrorMessage = "El modo de operación es requerido")]
        [RegularExpression("^(hotel|motel|hybrid)$", ErrorMessage = "Modo inválido. Valores permitidos: hotel, motel, hybrid")]
        public string OperationMode { get; set; } = "hotel";

        // ===== HORARIOS =====
        [Required(ErrorMessage = "La hora de check-in es requerida")]
        public TimeSpan CheckInTime { get; set; } = new TimeSpan(15, 0, 0);

        [Required(ErrorMessage = "La hora de check-out es requerida")]
        public TimeSpan CheckOutTime { get; set; } = new TimeSpan(12, 0, 0);

        [Required(ErrorMessage = "La hora de check-out tardío es requerida")]
        public TimeSpan LateCheckoutTime { get; set; } = new TimeSpan(14, 0, 0);

        [Range(0, 9999.99, ErrorMessage = "El cargo por check-out tardío debe estar entre 0 y 9999.99")]
        public decimal LateCheckoutCharge { get; set; } = 0;

        // ===== POLÍTICAS =====
        [Range(0, 168, ErrorMessage = "Las horas de cancelación deben estar entre 0 y 168 (7 días)")]
        public int CancellationHours { get; set; } = 24;

        public string? CancellationPolicyText { get; set; }

        // ===== IMPUESTOS =====
        public bool ChargesTaxes { get; set; } = true;

        [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100%")]
        public decimal TaxIvaPercent { get; set; } = 16.00m;

        [Range(0, 100, ErrorMessage = "El ISH debe estar entre 0 y 100%")]
        public decimal TaxIshPercent { get; set; } = 3.00m;

        // ===== ANTICIPOS =====
        [Range(0, 100, ErrorMessage = "El anticipo mínimo debe estar entre 0 y 100%")]
        public int MinDepositPercent { get; set; } = 50;

        // ===== HUÉSPEDES =====
        public bool RequireCompanionDetails { get; set; } = false;

        // ===== FACTURACIÓN =====
        public bool CanInvoice { get; set; } = false;

        [StringLength(13, ErrorMessage = "El RFC no puede exceder 13 caracteres")]
        public string? SatRfc { get; set; }

        [StringLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
        public string? SatBusinessName { get; set; }

        [StringLength(100, ErrorMessage = "El régimen fiscal no puede exceder 100 caracteres")]
        public string? SatTaxRegime { get; set; }

        // ===== BRANDING =====
        [StringLength(500, ErrorMessage = "La URL del logo no puede exceder 500 caracteres")]
        public string? LogoUrl { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Debe ser hexadecimal (ej: #2BA49A)")]
        public string PrimaryColor { get; set; } = "#2BA49A";

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Debe ser hexadecimal (ej: #D9C9B6)")]
        public string SecondaryColor { get; set; } = "#D9C9B6";

        // ===== OTROS =====
        [Required(ErrorMessage = "La zona horaria es requerida")]
        [StringLength(50, ErrorMessage = "La zona horaria no puede exceder 50 caracteres")]
        public string Timezone { get; set; } = "America/Mexico_City";
    }
}