// GarthHMS.Core/DTOs/Guest/GuestSearchDto.cs
using System;

namespace GarthHMS.Core.DTOs.Guest
{
    /// <summary>
    /// DTO simplificado para búsqueda rápida de huéspedes (autocomplete)
    /// </summary>
    public class GuestSearchDto
    {
        public Guid GuestId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsVip { get; set; }
        public bool IsBlacklisted { get; set; }
        public int TotalStays { get; set; }

        /// <summary>
        /// Texto para mostrar en el autocomplete
        /// Formato: "Juan Pérez - 555-1234 - juan@email.com"
        /// </summary>
        public string DisplayText => BuildDisplayText();

        private string BuildDisplayText()
        {
            var text = FullName;

            if (!string.IsNullOrWhiteSpace(Phone))
            {
                text += $" - {Phone}";
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                text += $" - {Email}";
            }

            if (IsVip)
            {
                text += " ⭐";
            }

            if (IsBlacklisted)
            {
                text += " ⚠️";
            }

            return text;
        }
    }
}