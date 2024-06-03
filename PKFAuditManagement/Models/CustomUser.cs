using Microsoft.AspNetCore.Identity;
namespace PKFAuditManagement.Models
{
    public class CustomUser: IdentityUser
    {
        public string? FullName { get; set; }

    }
}
