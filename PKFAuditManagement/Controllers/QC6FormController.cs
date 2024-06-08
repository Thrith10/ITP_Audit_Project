using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using System.Data;
using System.Globalization;

namespace PKFAuditManagement.Controllers
{
    public class QC6FormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public QC6FormController(IUserService userService, ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
        public async Task<IActionResult> QC6FormCreationAsync()
        {
            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            // Retrieve QC6Form data
            var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel { UserEmail = userEmail, AdminEmails = adminEmails.OrderBy(email => email).ToList() });

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
        public async Task<IActionResult> SubmitQC6Form(QC6FormCreationViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Populate Sub Forms
            viewModel = RetrieveSubFormData(viewModel);

            // Append UserEmail to viewModel
            viewModel.UserEmail = user.Email;

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            // Append emails to viewModel
            viewModel.AdminEmails = adminEmails.OrderBy(email => email).ToList();


            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    // Save viewModel data to EngagementTable
                    var qc6form = new QC6Form
                    {
                        FileReference = Helper.GenerateQCFormFileReference(),
                        CreatedBy = userId,
                        ProspectiveClient = viewModel.ProspectiveClient,
                        PeriodEnded = viewModel.PeriodEnded.Value,
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
                        EstimatedFee = viewModel.EstimatedFee.Value,
                        BudgetedTimeCost = viewModel.BudgetedTimeCost.Value,
                        BudgetedFeeRecoveryRate = viewModel.BudgetedFeeRecoveryRate.Value,
                        OutstandingUnpaidFees = viewModel.OutstandingUnpaidFees,
                        GrandTotal = viewModel.GrandTotal.Value,
                        AuditFee = viewModel.AuditFee.Value,
                        FeeConcentration = viewModel.FeeConcentration.Value,
                        ConflictsCheckDone = viewModel.ConflictsCheckDone,
                        TypeOfActivities = viewModel.TypeOfActivities,
                        ComplexityOfEngagement = viewModel.ComplexityOfEngagement,
                        PredecessorAuditor = viewModel.PredecessorAuditor,
                        ReasonsForDiscontinuance = viewModel.ReasonsForDiscontinuance,
                        PublicInterestEntity = viewModel.IsPublicInterestEntity,
                        PublicInterestEntityType = viewModel.PublicInterestEntityType,
                        IsSubForm2NotApplicable = viewModel.SubForm1NotApplicable, // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                        IsSubForm3NotApplicable = viewModel.SubForm2NotApplicable // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                    };

                    // Add qc6form data to intermediary datastore
                    _context.Add(qc6form);
                    _context.SaveChanges();

                    // Retrieve the QC6FormID from the saved entity
                    int qc6formId = qc6form.QC6FormID;

                    var qc6formConclusion = new QC6FormConclusion
                    {
                        QC6FormID = qc6formId,
                        AnySignificantRisk = viewModel.AnySignificantRisk,
                        SignificantRiskComment = viewModel.SignificantRiskComment,
                        NewEngagementRiskRating = viewModel.NewEngagementRiskRating,
                        NewEngagementRiskRatingReason = viewModel.NewEngagementRiskRatingReason,
                        EngagementSubjectedTo = viewModel.EngagementSubjectedTo,
                        SafeguardReviewerAssigned = viewModel.SafeguardReviewerAssigned,
                        IsNewEngagementAcceptance = viewModel.IsNewEngagementAcceptance,
                        IsSuspiciousTransactionReportFiled = viewModel.IsSuspiciousTransactionReportFiled,
                        SuspiciousTransactionReportFiledRationale = viewModel.SuspiciousTransactionReportFiledRationale,
                        Satisfaction = viewModel.Satisfaction,
                        PreparedBy = viewModel.ConclusionPreparedBy,
                        PreparedByDate = viewModel.ConclusionPreparedByDate.Value,
                        EPHODApprovedBy = viewModel.EPHODApprovedBy,
                        EPHODApprovedByDate = viewModel.EPHODApprovedByDate,
                        MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy,
                        MPHODQMPApprovedByDate = viewModel.MPHODQMPApprovedByDate
                    };

                    // Add qc6formConclusion data to the database
                    _context.Add(qc6formConclusion);
                    _context.SaveChanges();

                    // Process the submitted data
                    foreach (var subForm in viewModel.SubForms)
                    {
                        foreach (var objective in subForm.Objectives)
                        {
                            foreach (var testDescription in objective.TestDescriptions)
                            {
                                // Populate the TestDescriptions with posted data
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].SignBy"];
                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].SignDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                {
                                    // Use the parsed date
                                    testDescription.SignDate = date;
                                }
                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].Comment"];

                                // Save QC6FormTest
                                var qc6formTest = new QC6FormTest
                                {
                                    QC6FormID = qc6formId,
                                    QC6FormTestDescriptionID = testDescription.QC6FormTestDescriptionID,
                                    SignOffDate = testDescription.SignDate,
                                    SignOffBy = testDescription.SignBy,
                                    Comments = testDescription.Comment
                                };

                                // Add qc6formTest to the context and save changes
                                _context.Add(qc6formTest);
                            }
                        }
                    }

                    // Save all pending changes to QC6FormTest entities at once
                    _context.SaveChanges();

                    // Process the submitted data
                    foreach (var service in viewModel.Services)
                    {
                        // Save QC6FormFeeDetail
                        var qC6FormFeeDetail = new QC6FormFeeDetail
                        {
                            QC6FormID = qc6formId,
                            NatureOfService = service.NatureOfService,
                            Fee = service.Fee.Value,
                            OtherService = service.OtherService,
                        };

                        // Add qC6FormFeeDetail to the context and save changes
                        _context.Add(qC6FormFeeDetail);
                    }

                    // Save all pending changes to QC6FormFeeDetail entities at once
                    _context.SaveChanges();

                    transaction.Commit();
                    if (roles.Contains("Admin"))
                    {
                        // Redirect to admin-specific page
                        return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
                    }
                    else
                    {
                        // Redirect to user-specific page
                        return RedirectToAction("QC6FormManagement", "QC6Form");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log the error
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
                }
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Admin")]
        public IActionResult DeleteQC6Form(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var qc6Form = _context.QC6Forms.Find(id);

                if (qc6Form == null)
                {
                    return NotFound();
                }

                // Delete from QC6FormTests 
                var tests = _context.QC6FormTests.Where(t => t.QC6FormID == id);
                _context.QC6FormTests.RemoveRange(tests);

                // Delete from QC6FormConclusions
                var conclusions = _context.QC6FormConclusions.Where(c => c.QC6FormID == id);
                _context.QC6FormConclusions.RemoveRange(conclusions);

                // Delete from QC6FormFeeDetails
                var feeDetails = _context.QC6FormFeeDetails.Where(c => c.QC6FormID == id);
                _context.QC6FormFeeDetails.RemoveRange(feeDetails);

                // Delete the QC6Form itself
                _context.QC6Forms.Remove(qc6Form);

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

        public QC6FormCreationViewModel RetrieveSubFormData(QC6FormCreationViewModel viewModel)
        {
            // Retrieve QC6Form data from the database
            var qc6SubForms = _context.QC6SubForms.ToList();
            var qc6FormObjectives = _context.QC6FormObjectives.ToList();
            var qc6FormTestDescriptions = _context.QC6FormTestDescriptions.ToList();

            // Populate SubForms
            viewModel.SubForms = qc6SubForms.Select(subForm => new SubFormViewModel
            {
                QC6SubFormID = subForm.QC6SubFormID,
                SubFormType = subForm.SubFormType,
                Objectives = qc6FormObjectives
                    .Where(obj => obj.QC6SubFormID == subForm.QC6SubFormID)
                    .Select(obj => new ObjectiveViewModel
                    {
                        QC6FormObjectiveID = obj.QC6FormObjectiveID,
                        Objective = obj.Objective,
                        TestDescriptions = qc6FormTestDescriptions
                            .Where(desc => desc.QC6FormObjectiveID == obj.QC6FormObjectiveID)
                            .Select(desc => new TestDescriptionViewModel
                            {
                                QC6FormTestDescriptionID = desc.QC6FormTestDescriptionID,
                                Description = desc.Description,
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
