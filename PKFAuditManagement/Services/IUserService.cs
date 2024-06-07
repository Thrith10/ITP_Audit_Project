using Microsoft.AspNetCore.Identity;
using PKFAuditManagement.Models;
using System.Security.Claims;

namespace PKFAuditManagement.Services
{
    public interface IUserService
    {
        Task<string> GetUserEmailAsync(ClaimsPrincipal user);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<CustomUser> _userManager;

        public UserService(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetUserEmailAsync(ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var applicationUser = await _userManager.GetUserAsync(user);

            if (applicationUser == null)
            {
                throw new Exception("User not found.");
            }

            return applicationUser.Email ?? "No email available";
        }

    }

}
