// GarthHMS.Core/DTOs/Guest/GuestListDto.cs
using System;

namespace GarthHMS.Core.DTOs.Guest
{
    /// <summary>
    /// DTO simplificado para lista de huéspedes (Bootstrap Table)
    /// </summary>
    public class GuestListDto
    {
        public Guid GuestId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsVip { get; set; }
        public bool IsBlacklisted { get; set; }
        public string Source { get; set; } = "direct";
        public string SourceText { get; set; } = string.Empty;
        public int TotalStays { get; set; }
        public DateTime? LastStayDate { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Total de registros (para paginación) - Viene del SP
        /// </summary>
        public int TotalCount { get; set; }
    }
}