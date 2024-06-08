using Microsoft.AspNetCore.Identity;
using PKFAuditManagement.Models;
using System.Security.Claims;

namespace PKFAuditManagement.Services
{
    public interface IUserService
    {
        Task<string> GetUserEmailAsync(ClaimsPrincipal user);
        Task<bool> IsUserInRoleAsync(ClaimsPrincipal user, string roleName);
        Task<IEnumerable<string>> GetUserEmailsInRoleAsync(string roleName);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        public async Task<bool> IsUserInRoleAsync(ClaimsPrincipal user, string roleName)
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

            return await _userManager.IsInRoleAsync(applicationUser, roleName);
        }

        public async Task<IEnumerable<string>> GetUserEmailsInRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                throw new Exception($"Role '{roleName}' not found.");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            var userEmails = new List<string>();

            foreach (var user in usersInRole)
            {
                userEmails.Add(user.Email);
            }

            return userEmails;
        }
    }

}
