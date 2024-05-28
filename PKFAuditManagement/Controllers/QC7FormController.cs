using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;

namespace PKFAuditManagement.Controllers
{
    public class QC7FormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QC7FormController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> QC7FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve continuing engagement data from database
            var engagements = _context.ContinuingEngagements.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC7/QC7FormManagement.cshtml", engagements);
        }

        [Authorize(Roles = "User")]
        public IActionResult QC7FormCreation()
        {
            var viewModel = new QC7FormViewModel();
            return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC7FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var engagements = _context.ContinuingEngagements.ToList();
            return View("~/Views/General/QC7/QC7FormApprovalManagement.cshtml", engagements);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC7Form/ApproveQC7Form/{id}")]
        public IActionResult ApproveQC7Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.ContinuingEngagements.FirstOrDefault(e => e.ContinuingEngagementId == id);

            if (engagement == null)
            {
                return NotFound();
            }

            // Update the engagement status to "Approved"
            engagement.Status = "Approved";
            _context.SaveChanges();

            return RedirectToAction("QC7FormApprovalManagement", "QC7Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SubmitQC7Form(QC7FormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get the current user's ID
                    var user = await _userManager.GetUserAsync(User);
                    var userId = user?.Id;

                    // Save viewModel data to EngagementTable
                    var engagementData = new ContinuingEngagement
                    {
                        FileReference = Helper.GenerateQCFormFileReference(),
                        CreatedBy = userId,
                        Client = viewModel.Client,
                        PeriodEnded = viewModel.PeriodEnded,
                        EngagementType = viewModel.EngagementType,
                        PreparedBy = viewModel.PreparedBy,
                        PreparedByDate = viewModel.PreparedByDate,
                        ReviewedBy = viewModel.ReviewedBy,
                        ReviewedByDate = viewModel.ReviewedByDate,
                        Status = "Pending",
                        FormSubmissionDate = DateTime.Now
                    };
                    _context.ContinuingEngagements.Add(engagementData);
                    _context.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("QC7FormManagement", "QC7Form");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // Log the error
                    viewModel.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
                }
            }
        }
    }
}
