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

namespace PKFAuditManagement.Controllers
{
    public class SignedFSFormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public SignedFSFormController(ApplicationDbContext context, UserManager<CustomUser> userManager, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> SignedFSFormManagementAsync()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);

            // Retrieve signed FS data from database
            var signedFSForms = _context.SignedFSForm.ToList();

            return View("~/Views/General/SignedFS/SignedFSFormManagement.cshtml", signedFSForms);
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> ScheduleEmailsAsync()
        {
            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            var model = new SignedFSFormViewModel
            {
                UserEmail = User.Identity.Name,
                ClientNames = clientNames,
                PartnerEmailOptions = adminEmails.OrderBy(email => email).ToList()
            };

            return View("~/Views/General/SignedFS/SignedFSForm.cshtml", model);
        }

        [HttpPost]

        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> ScheduleEmails(SignedFSFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save the uploaded file (Financial Statement)
                var filePath = string.Empty;
                if (model.FinancialStatement != null)
                {
                    filePath = Path.Combine(filePath, model.FinancialStatement.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FinancialStatement.CopyToAsync(stream);
                    }
                }

                // Create the Signed FS entry
                var signedFs = new SignedFSForm
                {
                    Client = model.Client,
                    AuditedReportDate = model.AuditedReportDate,
                    FinancialYearEnd = model.FinancialYearEnd,
                    PartnerEmail = model.PartnerEmail,
                    UserEmail = model.UserEmail,
                    FilePath = filePath
                };

                _context.SignedFSForm.Add(signedFs);
                await _context.SaveChangesAsync();

                return RedirectToAction("SignedFSFormManagement");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> EditSignedFS(int id)
        {
            var job = await _context.SignedFSForm.FindAsync(id);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Signed Financial Statement not found.";
                return RedirectToAction("SignedFSFormManagement");
            }

            // Retrieve admin emails
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

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
                PartnerEmailOptions = adminEmails.OrderBy(email => email).ToList(),
                FinancialStatement = null // or set to a default value
            };

            return View("~/Views/General/SignedFS/EditSignedFS.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Non-Auditor,User,Admin")]
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

                //// Handle file upload if a new file is provided
                //if (model.FinancialStatement != null)
                //{
                //    var filePath = Path.Combine("wwwroot/files", model.FinancialStatement.FileName);
                //    using (var stream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await model.FinancialStatement.CopyToAsync(stream);
                //    }
                //    job.FilePath = filePath;
                //}

                _context.SignedFSForm.Update(job);
                await _context.SaveChangesAsync();

                return RedirectToAction("SignedFSFormManagement");
            }

            // Retrieve admin emails
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            // Set to viewbag and model
            ViewBag.PartnerEmailOptions = adminEmails.OrderBy(email => email).ToList();
            model.PartnerEmailOptions = adminEmails.OrderBy(email => email).ToList();

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
        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> DeleteSignedFS(int id)
        {
            try
            {
                var fs = await _context.SignedFSForm.FindAsync(id);
                if (fs == null)
                {
                    return Json(new { success = false, message = "No signed financial statement found." });
                }

                _context.SignedFSForm.RemoveRange(fs);
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
