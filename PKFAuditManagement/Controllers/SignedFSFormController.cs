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

namespace PKFAuditManagement.Controllers
{
    public class SignedFSFormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public SignedFSFormController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> SignedFSFormManagementAsync()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);

            // Retrieve signed FS data from database with unique clients
            var signedFSForms = _context.SignedFSForm
                .Where(e => e.UserEmail.Equals(user.Email))
                .GroupBy(e => e.Client) 
                .Select(g => g.First()) 
                .ToList();

            return View("~/Views/General/SignedFS/SignedFSFormManagement.cshtml", signedFSForms);
        }

        [HttpGet]

        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public IActionResult ScheduleEmails()
        {
            var model = new SignedFSFormViewModel
            {
                UserEmail = User.Identity.Name // Assuming the user email is set in the identity
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

                // Create the scheduled job entries
                var jobs = new List<SignedFSForm>
                {
                    new SignedFSForm
                    {
                        Client = model.Client,
                        AuditedReportDate = model.AuditedReportDate,
                        PartnerEmail = model.PartnerEmail,
                        UserEmail = model.UserEmail,
                        FilePath = filePath,
                        ScheduleDate = DateTime.Now.AddMinutes(1), // ScheduleDate for demo
                        EmailType = "First Reminder",
                        EmailBody = "This is the first reminder. Please take necessary actions.",
                        IsProcessed = false
                    },
                    new SignedFSForm
                    {
                        Client = model.Client,
                        AuditedReportDate = model.AuditedReportDate,
                        PartnerEmail = model.PartnerEmail,
                        UserEmail = model.UserEmail,
                        FilePath = filePath,
                        ScheduleDate = DateTime.Now.AddMinutes(2), // ScheduleDate for demo
                        EmailType = "Second Reminder",
                        EmailBody = "This is the second reminder. Please take necessary actions.",
                        IsProcessed = false
                    },
                    new SignedFSForm
                    {
                        Client = model.Client,
                        AuditedReportDate = model.AuditedReportDate,
                        PartnerEmail = model.PartnerEmail,
                        UserEmail = model.UserEmail,
                        FilePath = filePath,
                        ScheduleDate = DateTime.Now.AddMinutes(3), // ScheduleDate for demo
                        EmailType = "Final Reminder",
                        EmailBody = "This is the final reminder. Please take necessary actions.",
                        IsProcessed = false
                    }
                };

                _context.SignedFSForm.AddRange(jobs);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Form submitted successfully!";

                return RedirectToAction("ScheduleEmails");
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
                TempData["ErrorMessage"] = "Scheduled email job not found.";
                return RedirectToAction("ScheduleEmails");
            }

            var model = new SignedFSFormViewModel
            {
                Id = job.Id,
                Client = job.Client,
                AuditedReportDate = job.AuditedReportDate,
                PartnerEmail = job.PartnerEmail,
                UserEmail = job.UserEmail,
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
                    TempData["ErrorMessage"] = "Scheduled email job not found.";
                    return RedirectToAction("ScheduleEmails");
                }

                // Update the job properties
                job.Client = model.Client;
                job.AuditedReportDate = model.AuditedReportDate;
                job.PartnerEmail = model.PartnerEmail;
                job.UserEmail = model.UserEmail;

                // Handle file upload if a new file is provided
/*                if (model.FinancialStatement != null)
                {
                    var filePath = Path.Combine("wwwroot/files", model.FinancialStatement.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FinancialStatement.CopyToAsync(stream);
                    }
                    job.FilePath = filePath;
                }*/

                _context.SignedFSForm.Update(job);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Scheduled email job updated successfully!";
                return RedirectToAction("ScheduleEmails");
            }

            ViewBag.PartnerEmailOptions = _context.SignedFSForm
                .Select(x => x.PartnerEmail)
                .Distinct()
                .ToList();

            return View("~/Views/General/SignedFS/EditSignedFS.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Non-Auditor,User,Admin")]
        public async Task<IActionResult> DeleteSignedFS()
        {
            try
            {
                var jobs = await _context.SignedFSForm.ToListAsync();
                if (jobs.Count == 0)
                {
                    return Json(new { success = false, message = "No scheduled email jobs found." });
                }

                _context.SignedFSForm.RemoveRange(jobs);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Scheduled email job deleted successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting scheduled email jobs." });
            }
        }


        // For single deletion but to edit the logic
        /*        [HttpPost]
                [Authorize(Roles = "Non-Auditor,User,Admin")]
                public async Task<IActionResult> DeleteSignedFS(int id)
                {
                    try
                    {
                        var job = await _context.SignedFSForm.FindAsync(id);
                        if (job == null)
                        {
                            TempData["ErrorMessage"] = "Scheduled email job not found.";
                            return RedirectToAction("ScheduleEmails");
                        }

                        _context.SignedFSForm.Remove(job);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Scheduled email job deleted successfully!";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    return RedirectToAction("ScheduleEmails");
                }*/

    }
}
