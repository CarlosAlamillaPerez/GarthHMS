using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Huésped (cliente que hace reservas)
    /// </summary>
    public class Guest
    {
        public int GuestId { get; set; }
        public int HotelId { get; set; }

        // INFORMACIÓN PERSONAL
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        // IDENTIFICACIÓN
        public string? IdType { get; set; }  // "INE", "Pasaporte", "Licencia"
        public string? IdNumber { get; set; }

        // PREFERENCIAS
        public bool IsSmoker { get; set; } = false;
        public bool TravelWithPets { get; set; } = false;
        public string? SpecialRequests { get; set; }

        // HISTORIAL
        public int TotalStays { get; set; } = 0;
        public DateTime? LastStayDate { get; set; }

        // NOTAS INTERNAS
        public string? InternalNotes { get; set; }

        // BLACKLIST
        public bool IsBlacklisted { get; set; } = false;
        public string? BlacklistReason { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
