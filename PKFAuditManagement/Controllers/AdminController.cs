using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;


namespace PKFAuditManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult CreateAccount()
        {
            var viewModel = new UserViewModel();
            return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
        }

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

                userViewModels.Add(new UserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = role
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
                        PhoneNumber = viewModel.PhoneNumber,
                        FullName = viewModel.FullName
                    };

                    var result = await _userManager.CreateAsync(user, viewModel.Password);

                    if (result.Succeeded)
                    {
                        // You can add roles or any other operations here
                        await _userManager.AddToRoleAsync(user, viewModel.Role);

                        await transaction.CommitAsync();
                        return RedirectToAction("AccountManagement", "Admin");
                    }
                    else
                    {
                        transaction.Rollback();
                        ViewBag.Errors = result.Errors;
                        return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/Admin/CreateAccount.cshtml", viewModel);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        return RedirectToAction("AccountManagement", "Admin");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        ViewBag.Errors = result.Errors;
                        return RedirectToAction("AccountManagement", "Admin");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return RedirectToAction("AccountManagement", "Admin");
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
                PhoneNumber = user.PhoneNumber,
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
                return View("~/Views/Admin/EditAccount.cshtml", viewModel);
            }

            var user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.UserName = viewModel.UserName;
            user.Email = viewModel.Email;
            user.PhoneNumber = viewModel.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("AccountManagement", "Admin");
            }
            else
            {
                ViewBag.Errors = result.Errors;
                return View("~/Views/Admin/EditAccount.cshtml", viewModel);
            }
        }
    }
}
