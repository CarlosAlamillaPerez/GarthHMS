using System;

namespace GarthHMS.Core.Entities
{
    public class Role
    {
        public Guid RoleId { get; set; }
        public Guid HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxDiscountPercent { get; set; } = 0;
        public bool IsSystemRole { get; set; } = false;
        public bool IsManagerRole { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}