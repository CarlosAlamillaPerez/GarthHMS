using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.DTOs
{
    /// <summary>
    /// DTO para habitación
    /// </summary>
    public class RoomDto
    {
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public RoomStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public bool AllowsPets { get; set; }
        public bool IsSmoking { get; set; }
        public bool IsActive { get; set; }
    }
}
