using System;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Configuración general del hotel
    /// Un registro único por hotel
    /// </summary>
    public class HotelSettings
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
        public string OperationMode { get; set; } = "hotel"; // hotel, motel, hybrid
        public bool CompanionsOptional { get; set; } = false;

        // ===== HORARIOS =====
        public string CheckInTime { get; set; } = "15:00:00";
        public string CheckOutTime { get; set; } = "12:00:00";
        public string LateCheckoutTime { get; set; } = "14:00:00";
        public decimal LateCheckoutCharge { get; set; } = 0;

        // ===== POLÍTICAS =====
        public int CancellationHours { get; set; } = 24;
        public string? CancellationPolicyText { get; set; }
        public string CancellationPolicyType { get; set; } = "window";
        public int RefundPercentOnCancel { get; set; } = 0;
        public string? RefundTiers { get; set; }
        public int NoShowChargePercent { get; set; } = 100;

        // ===== IMPUESTOS =====
        public bool ChargesTaxes { get; set; } = true;
        public decimal TaxIvaPercent { get; set; } = 16.00m;
        public decimal TaxIshPercent { get; set; } = 3.00m;

        // ===== ANTICIPOS =====
        public int MinDepositPercent { get; set; } = 50;

        // ===== HUÉSPEDES =====
        public bool RequireCompanionDetails { get; set; } = false;

        // ===== FACTURACIÓN =====
        public bool CanInvoice { get; set; } = false;
        public string? SatRfc { get; set; }
        public string? SatBusinessName { get; set; }
        public string? SatTaxRegime { get; set; }

        // ===== BRANDING =====
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#2BA49A";
        public string SecondaryColor { get; set; } = "#D9C9B6";

        // ===== OTROS =====
        public string Timezone { get; set; } = "America/Mexico_City";

        // ===== AUDIT =====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool AutoVerifyCard { get; set; } = false;
        public bool AutoVerifyTransfer { get; set; } = false;
        public bool BlockCheckinIfBalance { get; set; } = false;
    }
}