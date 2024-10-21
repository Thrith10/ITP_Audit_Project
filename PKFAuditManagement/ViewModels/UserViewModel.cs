using Microsoft.AspNetCore.Mvc;

namespace PKFAuditManagement.ViewModels
{
    public class UserViewModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public bool IsLockedOut { get; set; }
    }

}
