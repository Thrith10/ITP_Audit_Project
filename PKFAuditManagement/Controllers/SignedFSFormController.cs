using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.InkML;
using PKFAuditManagement.Services;
using Microsoft.Extensions.Hosting;

namespace PKFAuditManagement.Controllers
{
    public class SignedFSFormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public SignedFSFormController(ApplicationDbContext context, UserManager<CustomUser> userManager, IUserService userService, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
            _environment = environment;
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> SignedFSFormManagementAsync()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Get the current user's ID
            var userId = user?.Id;

            // Get the current user's email
            var userEmail = user?.Email;

            // Check if the user is in the "Admin" role
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Retrieve signedFS data from the database
            List<SignedFSForm> signedFS;

            if (isAdmin)
            {
                // If the user is an Admin, retrieve all signed FS
                signedFS = _context.SignedFSForm.ToList();
            }
            else
            {
                // Otherwise, retrieve only those created by the current user
                signedFS = _context.SignedFSForm.Where(e => e.UserEmail.Equals(userEmail)).ToList();
            }

            // Retrieve signed FS data from database
            var signedFSForms = _context.SignedFSForm.ToList();

            return View("~/Views/General/SignedFS/SignedFSFormManagement.cshtml", signedFSForms);
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> ScheduleEmailsAsync()
        {
            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            var model = new SignedFSFormViewModel
            {
                UserEmail = User.Identity.Name,
                ClientNames = clientNames,
                PartnerEmailOptions = combinedEmails
            };

            return View("~/Views/General/SignedFS/SignedFSForm.cshtml", model);
        }

        [HttpPost]

        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> ScheduleEmails(SignedFSFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Generate a unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                // Get the path to wwwroot
                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "SignedFS-FinancialStatements");

                // Ensure the uploads folder exists
                Directory.CreateDirectory(uploadsFolder);

                // Get file path
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to wwwroot/uploads/SignedFS-FinancialStatements
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FinancialStatement.CopyToAsync(stream);
                }

                // Create the Signed FS entry
                var signedFs = new SignedFSForm
                {
                    Client = model.Client,
                    AuditedReportDate = model.AuditedReportDate,
                    FinancialYearEnd = model.FinancialYearEnd,
                    PartnerEmail = model.PartnerEmail,
                    UserEmail = model.UserEmail,
                    FilePath = uniqueFileName,
                    IsProcessed = true
                };

                _context.SignedFSForm.Add(signedFs);
                await _context.SaveChangesAsync();

                return RedirectToAction("SignedFSFormManagement");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> EditSignedFS(int id)
        {
            var job = await _context.SignedFSForm.FindAsync(id);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Signed Financial Statement not found.";
                return RedirectToAction("SignedFSFormManagement");
            }

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names


            var model = new SignedFSFormViewModel
            {
                Id = job.Id,
                Client = job.Client,
                ClientNames = clientNames,
                AuditedReportDate = job.AuditedReportDate,
                FinancialYearEnd = job.FinancialYearEnd,
                PartnerEmail = job.PartnerEmail,
                UserEmail = job.UserEmail,
                FinancialStatementFileName = job.FilePath,
                PartnerEmailOptions = combinedEmails,
                FinancialStatement = null // or set to a default value
            };

            return View("~/Views/General/SignedFS/EditSignedFS.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> UpdateSignedFS(SignedFSFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var job = await _context.SignedFSForm.FindAsync(model.Id);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Signed Financial Statement not found.";
                    return RedirectToAction("ScheduleEmails");
                }

                // Update the job properties
                job.Client = model.Client;
                job.AuditedReportDate = model.AuditedReportDate;
                job.FinancialYearEnd = model.FinancialYearEnd;
                job.PartnerEmail = model.PartnerEmail;
                job.UserEmail = model.UserEmail;

                if (model.FinancialStatement != null)
                {
                    // Generate a unique file name for the new document
                    var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                    // Construct the file path
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/SignedFS-FinancialStatements", job.FilePath);

                    // Remove the old file from wwwroot
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Update the old document
                    job.FilePath = uniqueFileName;

                    // Save all changes
                    await _context.SaveChangesAsync();

                    // Define the path for the new document
                    var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/SignedFS-FinancialStatements", uniqueFileName);

                    // Save the new document to the wwwroot folder
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        model.FinancialStatement.CopyTo(fileStream);
                    }
                }

                _context.SignedFSForm.Update(job);
                await _context.SaveChangesAsync();

                return RedirectToAction("SignedFSFormManagement");
            }

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Set to viewbag and model
            ViewBag.PartnerEmailOptions = combinedEmails;
            model.PartnerEmailOptions = combinedEmails;

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names


            // Append client names to model
            model.ClientNames = clientNames;

            return View("~/Views/General/SignedFS/EditSignedFS.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> DeleteSignedFS(int id)
        {
            try
            {
                var fs = await _context.SignedFSForm.FindAsync(id);
                if (fs == null)
                {
                    return Json(new { success = false, message = "No signed financial statement found." });
                }

                // Construct the file path
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/SignedFS-FinancialStatements", fs.FilePath);

                // Remove the file from wwwroot
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.SignedFSForm.Remove(fs);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Signed Financial Statement deleted successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the signed financial statement." });
            }
        }
    }
}
