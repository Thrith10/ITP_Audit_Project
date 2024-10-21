using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;
using System.Security.Claims;
using System.Text;

namespace PKFAuditManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public AdminController(ApplicationDbContext context, UserManager<CustomUser> userManager, 
            RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "Admin,Reviewer")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAccount()
        {
            var viewModel = new UserViewModel();
            return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditAccount()
        {
            return View("~/Views/Admin/EditAccount.cshtml");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AccountManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault(); // Since each user has only one role
                var isLockedOut = await _userManager.IsLockedOutAsync(user); // Check if user is locked out

                userViewModels.Add(new UserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = role,
                    IsLockedOut = isLockedOut // Pass the lockout status to the view
                });
            }

            return View(userViewModels);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitUserForm(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewBag.Errors = errors;
                TempData["ErrorMessage"] = "Invalid form submission. Please correct the errors and try again.";
                return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = new CustomUser
                    {
                        UserName = viewModel.UserName,
                        Email = viewModel.Email,
                        FullName = viewModel.FullName,
                        LockoutEnabled = true // Enable account lockout for the new user
                    };

                    var defaultPassword = GenerateRandomPassword(); // Generate a random password
                    var result = await _userManager.CreateAsync(user, defaultPassword);

                    if (result.Succeeded)
                    {
                        // Verify the password to ensure it's stored correctly
                        var isPasswordValid = await _userManager.CheckPasswordAsync(user, defaultPassword);
                        Console.WriteLine($"Is password valid after creation: {isPasswordValid}");

                        // You can add roles or any other operations here
                        await _userManager.AddToRoleAsync(user, viewModel.Role);

                        // Add claim to force password change on first login
                        await _userManager.AddClaimAsync(user, new Claim("ForcePasswordChange", "true"));

                        // Send email with the default password
                        await _emailSender.SendEmailAsync(viewModel.Email, "Your account has been created",
                            $"Your account has been created. Your temporary password is {defaultPassword}. " +
                            $"Please change your password after logging in for the first time.");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Account created successfully!";
                        return RedirectToAction("AccountManagement"); 
                    }
                    else
                    {
                        transaction.Rollback();
                        ViewBag.Errors = result.Errors;
                        TempData["ErrorMessage"] = result.Errors.Select(e => e.Description).First();
                        return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID cannot be null or empty." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Set LockoutEnd to a very high value to suspend indefinitely
                    user.LockoutEnd = DateTimeOffset.MaxValue;

                    // Update the user lockout status in the database
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "User suspended successfully!" });
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Failed to suspend the user. Please try again." });
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred while processing your request. Please try again." });
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID cannot be null or empty." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Set LockoutEnd to null to activate the user
                    user.LockoutEnd = null;

                    // Update the user lockout status in the database
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "User activated successfully!" });
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Failed to activate the user. Please try again." });
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred while processing your request. Please try again." });
                }
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var viewModel = new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() // Assuming single role
            };

            return View("~/Views/Admin/EditAccount.cshtml", viewModel);         

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewBag.Errors = errors;
                TempData["ErrorMessage"] = "Invalid form submission. Please correct the errors and try again.";
                return View("~/Views/Admin/EditAccount.cshtml", viewModel);
            }

            var user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("EditAccount", new { id = viewModel.UserId });
            }

            user.UserName = viewModel.UserName;
            user.Email = viewModel.Email;
            user.FullName = viewModel.FullName;

            var existingRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, existingRoles);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, viewModel.Role);
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("AccountManagement", new { id = viewModel.UserId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update the account. Please try again.";
                ViewBag.Errors = result.Errors;
                TempData["ErrorMessage"] = result.Errors.Select(e => e.Description).First();
                return View("~/Views/Admin/EditAccount.cshtml", viewModel);
            }
        }

        private string GenerateRandomPassword()
        {
            // Ensure this method generates a password that meets the complexity requirements
            // Example: 8 characters long with uppercase, lowercase, digits, and special characters
            var options = new PasswordOptions
            {
                RequiredLength = 8,
                RequiredUniqueChars = 1,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < options.RequiredLength
                || chars.Distinct().Count() < options.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult GenerateReport()
        {
            return View();
        }

    }
}
