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
    public class QC6FormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QC6FormController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> QC6FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve engagement data from database
            var qc6forms = _context.QC6Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC6/QC6FormManagement.cshtml", qc6forms);
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult QC6FormCreation()
        {
            // Retrieve QC6Form data from the database
            var qc6SubForms = _context.QC6SubForms.ToList();
            var qc6FormObjectives = _context.QC6FormObjectives.ToList();
            var qc6FormTestDescriptions = _context.QC6FormTestDescriptions.ToList();

            // Create ViewModel and populate it with the retrieved data
            var viewModel = new QC6FormViewModel
            {
                QC6SubForms = qc6SubForms,
                QC6FormObjectives = qc6FormObjectives,
                QC6FormTestDescriptions = qc6FormTestDescriptions
            };

            return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC6FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var qc6forms = _context.QC6Forms.ToList();
            return View("~/Views/General/QC6/QC6FormApprovalManagement.cshtml", qc6forms);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC6Form/ApproveQC6Form/{id}")]
        public IActionResult ApproveQC6Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID == id);

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
        public async Task<IActionResult> SubmitQC6Form(QC6FormViewModel viewModel)
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
                    // Get the current user's ID
                    var user = await _userManager.GetUserAsync(User);
                    var userId = user?.Id;

                    // Save viewModel data to EngagementTable
                    var qc6form = new QC6Form
                    {
                        FileReference = Helper.GenerateQCFormFileReference(),
                        CreatedBy = userId,
                        ProspectiveClient = viewModel.ProspectiveClient,
                        PeriodEnded = viewModel.PeriodEnded,
                        EngagementType = viewModel.EngagementType,
                        PreparedBy = viewModel.PreparedBy,
                        PreparedByDate = viewModel.PreparedByDate,
                        ReviewedBy = viewModel.ReviewedBy,
                        ReviewedByDate = viewModel.ReviewedByDate,
                        Status = "Pending",
                        FormSubmissionDate = DateTime.Now,
                        PKFEntityProposingService = viewModel.PKFEntityProposingService,
                        SourceOfReferral = viewModel.SourceOfReferral,
                        NatureOfServiceForEstimateFee = viewModel.NatureOfServiceForEstimateFee,
                        EstimatedFee = viewModel.EstimatedFee,
                        BudgetedTimeCost = viewModel.BudgetedTimeCost,
                        FeeFromServices = viewModel.FeeFromServices,
                        OutstandingUnpaidFees = viewModel.OutstandingUnpaidFees,
                        FeeConcentration = viewModel.FeeConcentration,
                        ConflictsCheckDone = viewModel.ConflictsCheckDone,
                        TypeOfActivities = viewModel.TypeOfActivities,
                        ComplexityOfEngagement = viewModel.ComplexityOfEngagement,
                        PredecessorAuditor = viewModel.PredecessorAuditor,
                        ReasonsForDiscontinuance = viewModel.ReasonsForDiscontinuance,
                        PublicInterestEntity = viewModel.IsPublicInterestEntity,
                        TypeOfPIE = viewModel.TypeOfPIE,
                        TransnationalEntity = viewModel.TransnationalEntity,
                        TransnationalAudit = viewModel.TransnationalAudit
                    };

                    _context.Add(qc6form);
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
