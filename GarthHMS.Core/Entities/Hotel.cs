using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Representa un hotel/motel cliente del sistema
    /// Multi-tenancy: Cada hotel es un tenant independiente
    /// </summary>
    public class Hotel
    {
        public int HotelId { get; set; }

        // INFORMACIÓN BÁSICA
        public string HotelName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string? RFC { get; set; }
        public string? TaxRegime { get; set; }

        // CONTACTO
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? WhatsApp { get; set; }
        public string? Website { get; set; }

        // DIRECCIÓN
        public string? Street { get; set; }
        public string? ExteriorNumber { get; set; }
        public string? InteriorNumber { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; } = "México";

        // CONFIGURACIÓN
        public string TimeZone { get; set; } = "America/Mexico_City";
        public string Currency { get; set; } = "MXN";
        public bool IsMotel { get; set; } = false;  // true = por horas, false = por noche
        public TimeSpan? DefaultCheckInTime { get; set; } = new TimeSpan(15, 0, 0); // 3:00 PM
        public TimeSpan? DefaultCheckOutTime { get; set; } = new TimeSpan(12, 0, 0); // 12:00 PM
        public int? MaxPetsAllowed { get; set; } = 0;
        public int? MaxGuestsPerRoom { get; set; } = 4;

        // FACTURACIÓN
        public bool RequiresTaxInvoice { get; set; } = true;
        public string? SATCertificatePath { get; set; }

        // SUBSCRIPCIÓN
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string SubscriptionPlan { get; set; } = "Basic"; // Basic, Premium

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
