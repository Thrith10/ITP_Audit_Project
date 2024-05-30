using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using System.Globalization;

namespace PKFAuditManagement.Controllers
{
    public class QC7FormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public QC7FormController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> QC7FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve continuing engagement data from database
            var engagements = _context.QC7Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC7/QC7FormManagement.cshtml", engagements);
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult QC7FormCreation()
        {
            // Retrieve QC7Form data
            var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel());
            return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC7FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var engagements = _context.QC7Forms.ToList();
            return View("~/Views/General/QC7/QC7FormApprovalManagement.cshtml", engagements);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC7Form/ApproveQC7Form/{id}")]
        public IActionResult ApproveQC7Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID == id);

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
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SubmitQC7Form(QC7FormCreationViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Populate Sub Forms
            viewModel = RetrieveSubFormData(viewModel);

            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    // Save viewModel data to EngagementTable
                    var qc7form = new QC7Form
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
                        PriorYearFee = viewModel.PriorYearFee,
                        TimeCosts = viewModel.TimeCosts,
                        PriorYearRecoveryRate = viewModel.PriorYearRecoveryRate,
                        AnyOutstandingUnpaidAuditFees = viewModel.AnyOutstandingUnpaidAuditFees,
                        TypeOfClientActivities = viewModel.TypeOfClientActivities, 
                        RiskRatingPriorYear = viewModel.RiskRatingPriorYear,
                        AnySuspiciousTransactionReportFiled =  viewModel.AnySuspiciousTransactionReportFiled,
                        SuspiciousTransactionReportFiledComment = viewModel.SuspiciousTransactionReportFiledComment,
                        SafeguardReviewerName = viewModel.SafeguardReviewerName,
                        AnyOutstandingUnpaidNonAuditFees = viewModel.AnyOutstandingUnpaidNonAuditFees,
                        FeeConcentration = viewModel.FeeConcentration,
                        ProposedFeeCurrentYear = viewModel.ProposedFeeCurrentYear,
                        BudgetedTimeCost = viewModel.BudgetedTimeCost,
                        ProposedRecoveryRateCurrentYear = viewModel.ProposedRecoveryRateCurrentYear,
                        IsPublicInterestEntity = viewModel.IsPublicInterestEntity,
                        PublicInterestEntityType = viewModel.PublicInterestEntityType,
                        TransnationalEntity = viewModel.TransnationalEntity,
                        TransnationalAudit = viewModel.TransnationalAudit,
                        TransnationalAuditComment = viewModel.TransnationalAuditComment,
                        Status = "Pending",
                        FormSubmissionDate = DateTime.Now
                    };
                    _context.QC7Forms.Add(qc7form);
                    _context.SaveChanges();

                    // Retrieve the QC7FormID from the saved entity
                    int qc7formId = qc7form.QC7FormID;

                    var qc7formConclusion = new QC7FormConclusion
                    {
                        QC7FormID = qc7formId,
                        AnyRiskAssociated = viewModel.AnyRiskAssociated,
                        RiskExplanationCurrentYearPriorYear = viewModel.RiskExplanationCurrentYearPriorYear,
                        IsSafeguardApplied = viewModel.IsSafeguardApplied,
                        NatureOfSafeguard = viewModel.NatureOfSafeguard,
                        ContinuingEngagementRiskRated = viewModel.ContinuingEngagementRiskRated,
                        SafeguardReviewPartnerAssigned = viewModel.SafeguardReviewPartnerAssigned,
                        IsSuspiciousTransactionReportFiled = viewModel.IsSuspiciousTransactionReportFiled,
                        SuspiciousTransactionReportFiledRationale = viewModel.SuspiciousTransactionReportFiledRationale,
                        EngagementRetainedRejected = viewModel.EngagementRetainedRejected,
                        EMPreparedBy = viewModel.EMPreparedBy,
                        EMPreparedByDate = viewModel.EMPreparedByDate.Value,
                        EPHODApprovedBy = viewModel.EPHODApprovedBy,
                        EPHODApprovedByDate = viewModel.EPHODApprovedByDate.Value,
                        MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy,
                        MPHODQMPApprovedByDate = viewModel.MPHODQMPApprovedByDate.Value
                    };

                    // Add qc7formConclusion data to the database
                    _context.Add(qc7formConclusion);
                    _context.SaveChanges();

                    // Process the submitted data
                    foreach (var subForm in viewModel.SubForms)
                    {
                        foreach (var objective in subForm.Objectives)
                        {
                            foreach (var testDescription in objective.TestDescriptions)
                            {
                                // Populate the TestDescriptions with posted data
                                testDescription.Reference = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].Reference"];
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignBy"];
                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignDate"], "ddMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime signDate))
                                {
                                    testDescription.SignDate = signDate;
                                }
                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].Comment"];

                                // Save QC7FormTest
                                var qc7formTest = new QC7FormTest
                                {
                                    QC7FormID = qc7formId,
                                    QC7FormTestDescriptionID = testDescription.QC7FormTestDescriptionID,
                                    Reference = testDescription.Reference,
                                    SignOffDate = DateTime.Now,
                                    SignOffBy = testDescription.SignBy,
                                    Comments = testDescription.Comment
                                };

                                // Add qc7formTest to the context and save changes
                                _context.Add(qc7formTest);
                            }
                        }
                    }

                    // Save all pending changes to QC7FormTest entities at once
                    _context.SaveChanges();
                    transaction.Commit();

                    if (roles.Contains("Admin"))
                    {
                        // Redirect to admin-specific page
                        return RedirectToAction("QC7FormApprovalManagement", "QC7Form");
                    }
                    else
                    {
                        // Redirect to user-specific page
                        return RedirectToAction("QC7FormManagement", "QC7Form");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log the error
                    viewModel.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
                }
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Admin")]
        public IActionResult DeleteQC7Form(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var qc7Form = _context.QC7Forms.Find(id);

                if (qc7Form == null)
                {
                    return NotFound();
                }

                // Delete from QC7FormTests 
                var tests = _context.QC7FormTests.Where(t => t.QC7FormID == id);
                _context.QC7FormTests.RemoveRange(tests);

                // Delete from QC7FormConclusions
                var conclusions = _context.QC7FormConclusions.Where(c => c.QC7FormID == id);
                _context.QC7FormConclusions.RemoveRange(conclusions);

                // Delete the QC7Form itself
                _context.QC7Forms.Remove(qc7Form);

                _context.SaveChanges();

                transaction.Commit(); // Commit the transaction if all operations are successful

                return NoContent(); // Respond with 204 No Content
            }
            catch (Exception)
            {
                transaction.Rollback(); // Roll back the transaction if an error occurs
                throw;
            }
        }

        public QC7FormCreationViewModel RetrieveSubFormData(QC7FormCreationViewModel viewModel)
        {
            // Retrieve QC7Form data from the database
            var qc7SubForms = _context.QC7SubForms.ToList();
            var qc7FormObjectives = _context.QC7FormObjectives.ToList();
            var qc7FormTestDescriptions = _context.QC7FormTestDescriptions.ToList();

            // Populate SubForms
            viewModel.SubForms = qc7SubForms.Select(subForm => new QC7SubFormViewModel
            {
                QC7SubFormID = subForm.QC7SubFormID,
                SubFormType = subForm.SubFormType,
                Objectives = qc7FormObjectives
                    .Where(obj => obj.QC7SubFormID == subForm.QC7SubFormID)
                    .Select(obj => new QC7ObjectiveViewModel
                    {
                        QC7FormObjectiveID = obj.QC7FormObjectiveID,
                        Objective = obj.Objective,
                        TestDescriptions = qc7FormTestDescriptions
                            .Where(desc => desc.QC7FormObjectiveID == obj.QC7FormObjectiveID)
                            .Select(desc => new QC7TestDescriptionViewModel
                            {
                                QC7FormTestDescriptionID = desc.QC7FormTestDescriptionID,
                                Description = desc.Description,
                                Reference = "",
                                SignDate = DateTime.MinValue,
                                SignBy = "",
                                Comment = ""
                            }).ToList()
                    }).ToList()

            }).ToList();

            return viewModel;
        }
    }
}
