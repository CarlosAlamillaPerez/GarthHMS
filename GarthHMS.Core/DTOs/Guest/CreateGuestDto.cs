// GarthHMS.Core/DTOs/Guest/CreateGuestDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.Guest
{
    /// <summary>
    /// DTO para crear un nuevo huésped
    /// </summary>
    public class CreateGuestDto
    {
        // ═══════════════════════════════════════════════════════════
        // DATOS BÁSICOS (Obligatorios)
        // ═══════════════════════════════════════════════════════════

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El teléfono debe tener entre 10 y 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string Phone { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════
        // DATOS BÁSICOS (Opcionales)
        // ═══════════════════════════════════════════════════════════

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono secundario no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono secundario no es válido")]
        public string? PhoneSecondary { get; set; }

        // ═══════════════════════════════════════════════════════════
        // IDENTIFICACIÓN (Opcional - se completa en check-in)
        // ═══════════════════════════════════════════════════════════

        public string? IdType { get; set; }

        [StringLength(30, ErrorMessage = "El número de identificación no puede exceder 30 caracteres")]
        public string? IdNumber { get; set; }

        [StringLength(500, ErrorMessage = "La URL de la foto no puede exceder 500 caracteres")]
        public string? IdPhotoUrl { get; set; }

        [StringLength(18, MinimumLength = 18, ErrorMessage = "El CURP debe tener exactamente 18 caracteres")]
        [RegularExpression(@"^[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d$", ErrorMessage = "El formato del CURP no es válido")]
        public string? Curp { get; set; }

        public DateTime? BirthDate { get; set; }

        // ═══════════════════════════════════════════════════════════
        // DIRECCIÓN (Todo opcional)
        // ═══════════════════════════════════════════════════════════

        [StringLength(200, ErrorMessage = "La calle no puede exceder 200 caracteres")]
        public string? AddressStreet { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string? AddressCity { get; set; }

        [StringLength(100, ErrorMessage = "El estado no puede exceder 100 caracteres")]
        public string? AddressState { get; set; }

        [StringLength(10, ErrorMessage = "El código postal no puede exceder 10 caracteres")]
        public string? AddressZip { get; set; }

        [StringLength(50, ErrorMessage = "El país no puede exceder 50 caracteres")]
        public string? AddressCountry { get; set; } = "México";

        // ═══════════════════════════════════════════════════════════
        // FACTURACIÓN (Todo opcional)
        // ═══════════════════════════════════════════════════════════

        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener 12 o 13 caracteres")]
        [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", ErrorMessage = "El formato del RFC no es válido")]
        public string? BillingRfc { get; set; }

        [StringLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
        public string? BillingBusinessName { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email de facturación no es válido")]
        [StringLength(100, ErrorMessage = "El email de facturación no puede exceder 100 caracteres")]
        public string? BillingEmail { get; set; }

        [StringLength(300, ErrorMessage = "La dirección de facturación no puede exceder 300 caracteres")]
        public string? BillingAddress { get; set; }

        [StringLength(10, ErrorMessage = "El código postal de facturación no puede exceder 10 caracteres")]
        public string? BillingZip { get; set; }

        [StringLength(100, ErrorMessage = "El régimen fiscal no puede exceder 100 caracteres")]
        public string? BillingTaxRegime { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PREFERENCIAS Y NOTAS
        // ═══════════════════════════════════════════════════════════

        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
        public string? Notes { get; set; }

        public bool IsVip { get; set; } = false;

        public bool IsBlacklisted { get; set; } = false;

        [StringLength(500, ErrorMessage = "La razón de blacklist no puede exceder 500 caracteres")]
        public string? BlacklistReason { get; set; }

        // ═══════════════════════════════════════════════════════════
        // ORIGEN
        // ═══════════════════════════════════════════════════════════

        public string Source { get; set; } = "direct";
    }
}