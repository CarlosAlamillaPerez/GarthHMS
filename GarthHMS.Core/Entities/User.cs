using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Usuario del sistema
    /// </summary>
    public class User
    {
        // IDs
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("hotel_id")]
        public Guid HotelId { get; set; }

        [Column("role_id")]
        public Guid RoleId { get; set; }

        // AUTENTICACIÓN
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("must_change_password")]
        public bool MustChangePassword { get; set; } = true;

        // INFORMACIÓN PERSONAL
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        // Propiedad calculada (no en BD)
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // ESTADO Y SEGURIDAD
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("failed_login_attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("locked_until")]
        public DateTime? LockedUntil { get; set; }

        // AUDITORÍA
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}