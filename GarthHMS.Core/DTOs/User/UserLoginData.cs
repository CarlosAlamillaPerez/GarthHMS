// GarthHMS.Core/DTOs/UserLoginData.cs
namespace GarthHMS.Core.DTOs
{
    public class UserLoginData
    {
        public Guid UserId { get; set; }
        public Guid HotelId { get; set; }
        public Guid RoleId { get; set; }
        public string UserRoleText { get; set; } = "";
        public bool IsManagerRole { get; set; }
        public int MaxDiscountPercent { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}