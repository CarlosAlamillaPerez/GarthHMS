using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.DTOs
{
    /// <summary>
    /// DTO para información básica del hotel
    /// </summary>
    public class HotelDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsMotel { get; set; }
        public bool IsActive { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty;
    }
}
