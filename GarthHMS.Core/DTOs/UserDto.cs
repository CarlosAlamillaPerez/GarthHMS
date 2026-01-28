using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.Enums;

namespace GarthHMS.Core.DTOs
{
    /// <summary>
    /// DTO para mostrar información de usuario
    /// </summary>
    public class UserDto
    {
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public UserRole UserRole { get; set; }
        public string UserRoleText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal MaxDiscountPercent { get; set; }
    }
}
