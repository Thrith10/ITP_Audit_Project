using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;

namespace PKFAuditManagement.Controllers
{
    public class QC6FormController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QC6FormController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Auditor")]
        public IActionResult QC6FormManagement()
        {
            // Retrieve engagement data from database
            var engagements = _context.Engagements.ToList();
            return View("~/Views/General/QC6/QC6FormManagement.cshtml", engagements);
        }

        [Authorize(Roles = "Auditor")]
        public IActionResult QC6FormCreation()
        {
            var viewModel = new QC6FormViewModel();
            return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC6FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var engagements = _context.Engagements.ToList();
            return View("~/Views/General/QC6/QC6FormApprovalManagement.cshtml", engagements);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC6Form/ApproveQC6Form/{id}")]
        public IActionResult ApproveQC6Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.Engagements.FirstOrDefault(e => e.EngagementID == id);

            if (engagement == null)
            {
                return NotFound();
            }

            // Update the engagement status to "Approved"
            engagement.Status = "Approved";
            _context.SaveChanges();

            return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Auditor")]
        public IActionResult SubmitQC6Form(QC6FormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Save viewModel data to EngagementTable
                    var engagementData = new Engagement
                    {
                        FileReference = Helper.GenerateQCFormFileReference(),
                        ProspectiveClient = viewModel.ProspectiveClient,
                        PeriodEnded = viewModel.PeriodEnded,
                        EngagementType = viewModel.EngagementType,
                        PreparedBy = viewModel.PreparedBy,
                        PreparedByDate = viewModel.PreparedByDate,
                        ReviewedBy = viewModel.ReviewedBy,
                        ReviewedByDate = viewModel.ReviewedByDate,
                        Status = "Pending",
                        FormSubmissionDate = DateTime.Now
                    };
                    _context.Engagements.Add(engagementData);
                    _context.SaveChanges();

/*                    // Save to Table2
                    var table2 = new Table2
                    {
                        Age = viewModel.Age,
                        // Map other properties
                    };
                    _context.Table2s.Add(table2);
                    _context.SaveChanges();*/

                    transaction.Commit();
                    return RedirectToAction("QC6FormManagement", "QC6Form");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // Log the error
                    viewModel.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
                }
            }
        }
    }
}
