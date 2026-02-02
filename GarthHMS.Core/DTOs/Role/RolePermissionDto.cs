using System;
using System.Collections.Generic;

namespace GarthHMS.Core.DTOs.Role
{
    public class RolePermissionDto
    {
        public Guid RoleId { get; set; }
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }

    public class PermissionDto
    {
        public Guid PermissionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsAssigned { get; set; } = false; // Para marcar si el rol tiene este permiso
    }
}