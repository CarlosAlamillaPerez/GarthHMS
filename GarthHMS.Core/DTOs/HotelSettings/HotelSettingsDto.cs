using System;

namespace GarthHMS.Core.DTOs.HotelSettings
{
    /// <summary>
    /// DTO para devolver configuración del hotel
    /// </summary>
    public class HotelSettingsDto
    {
        public Guid HotelSettingsId { get; set; }
        public Guid HotelId { get; set; }

        // ===== INFORMACIÓN GENERAL =====
        public string? HotelName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        // ===== MODO DE OPERACIÓN =====
        public string OperationMode { get; set; } = null!;
        public bool CompanionsOptional { get; set; }

        // ===== HORARIOS =====
        public string CheckInTime { get; set; } = "15:00:00";
        public string CheckOutTime { get; set; } = "12:00:00";
        public string LateCheckoutTime { get; set; } = "14:00:00";
        public decimal LateCheckoutCharge { get; set; }

        // ===== POLÍTICAS =====
        public int CancellationHours { get; set; }
        public string? CancellationPolicyText { get; set; }
        public string CancellationPolicyType { get; set; } = "window";
        public int RefundPercentOnCancel { get; set; } = 0;
        public string? RefundTiers { get; set; }
        public int NoShowChargePercent { get; set; } = 100;

        // ===== IMPUESTOS =====
        public bool ChargesTaxes { get; set; }
        public decimal TaxIvaPercent { get; set; }
        public decimal TaxIshPercent { get; set; }

        // ===== ANTICIPOS =====
        public int MinDepositPercent { get; set; }

        // ===== HUÉSPEDES =====
        public bool RequireCompanionDetails { get; set; }

        // ===== FACTURACIÓN =====
        public bool CanInvoice { get; set; }
        public string? SatRfc { get; set; }
        public string? SatBusinessName { get; set; }
        public string? SatTaxRegime { get; set; }

        // ===== BRANDING =====
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = null!;
        public string SecondaryColor { get; set; } = null!;

        // ===== OTROS =====
        public string Timezone { get; set; } = null!;

        // ===== AUDIT =====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool AutoVerifyCard { get; set; }
        public bool AutoVerifyTransfer { get; set; }
    }
}