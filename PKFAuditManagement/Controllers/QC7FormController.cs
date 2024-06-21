using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using System.Globalization;
using System.Text;

namespace PKFAuditManagement.Controllers
{
    public class QC7FormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        public QC7FormController(IUserService userService, ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
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
        public async Task<IActionResult> QC7FormCreationAsync()
        {
            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            // Retrieve QC7Form data
            var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel { UserEmail = userEmail, AdminEmails = adminEmails.OrderBy(email => email).ToList() });

            return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC7FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var engagements = _context.QC7Forms.ToList();
            return View("~/Views/General/QC7/QC7FormApprovalManagement.cshtml", engagements);
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> EditQC7Form(int id)
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var roles = await _userManager.GetRolesAsync(user);

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            try
            {
                var userId = user.Id;

                // Get the selected QC7 Form
                var qc7formData = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel());

                // Append emails to viewModel
                viewModel.AdminEmails = adminEmails.OrderBy(email => email).ToList();

                // Retrieve Conclusion data
                var conclusionData = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve QC7 Form Test data
                var testData = _context.QC7FormTests.Where(e => e.QC7FormID.Equals(id)).ToList();

                // Loop through each SubForm in the viewModel
                foreach (var subForm in viewModel.SubForms)
                {
                    // Loop through each Objective in the SubForm
                    foreach (var objective in subForm.Objectives)
                    {
                        // Loop through each TestDescription in the Objective
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            // Find the corresponding QC7FormTest data for the TestDescription
                            var test = testData.FirstOrDefault(t => t.QC7FormTestDescriptionID == testDescription.QC7FormTestDescriptionID);

                            if (test != null)
                            {
                                // Populate the TestDescription with QC7FormTest data
                                testDescription.SignBy = test.SignOffBy;
                                testDescription.SignDate = test.SignOffDate.Value;
                                testDescription.Comment = test.Comments;
                            }
                        }
                    }
                }

                // Append new data for QC7Form General Portion
                viewModel.QC7FormID = id.ToString();
                viewModel.Status = qc7formData.Status;
                viewModel.RejectionReason = qc7formData.RejectionReason;
                viewModel.FileReference = qc7formData.FileReference;
                viewModel.Client = qc7formData.Client;
                viewModel.PeriodEnded = qc7formData.PeriodEnded;
                viewModel.EngagementType = qc7formData.EngagementType;
                viewModel.PreparedBy = qc7formData.PreparedBy;
                viewModel.PreparedByDate = qc7formData.PreparedByDate;
                viewModel.ReviewedBy = qc7formData.ReviewedBy;
                viewModel.ReviewedByDate = qc7formData.ReviewedByDate;
                viewModel.PriorYearFee = qc7formData.PriorYearFee;
                viewModel.TimeCosts = qc7formData.TimeCosts;
                viewModel.PriorYearRecoveryRate = qc7formData.PriorYearRecoveryRate;
                viewModel.AnyOutstandingUnpaidAuditFees = qc7formData.AnyOutstandingUnpaidAuditFees;
                viewModel.TypeOfClientActivities = qc7formData.TypeOfClientActivities;
                viewModel.RiskRatingPriorYear = qc7formData.RiskRatingPriorYear;
                viewModel.AnySuspiciousTransactionReportFiled = qc7formData.AnySuspiciousTransactionReportFiled;
                viewModel.SuspiciousTransactionReportFiledComment = qc7formData.SuspiciousTransactionReportFiledComment;
                viewModel.SafeguardReviewerName = qc7formData.SafeguardReviewerName;
                viewModel.AnyOutstandingUnpaidNonAuditFees = qc7formData.AnyOutstandingUnpaidNonAuditFees;
                viewModel.FeeConcentration = qc7formData.FeeConcentration;
                viewModel.ProposedFeeCurrentYear = qc7formData.ProposedFeeCurrentYear;
                viewModel.BudgetedTimeCost = qc7formData.BudgetedTimeCost;
                viewModel.ProposedRecoveryRateCurrentYear = qc7formData.ProposedRecoveryRateCurrentYear;
                viewModel.IsPublicInterestEntity = qc7formData.IsPublicInterestEntity;
                viewModel.PublicInterestEntityType = qc7formData.PublicInterestEntityType;
                viewModel.SubForm1NotApplicable = qc7formData.IsSubForm2NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.SubForm2NotApplicable = qc7formData.IsSubForm3NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3

                // Append ConclusionData for QC7FormConclusion
                viewModel.AnyRiskAssociated = conclusionData.AnyRiskAssociated;

                if (viewModel.AnyRiskAssociated == true)
                {
                    viewModel.RiskExplanationCurrentYearPriorYear = conclusionData.RiskExplanationCurrentYearPriorYear;
                    viewModel.IsSafeguardApplied = conclusionData.IsSafeguardApplied;
                    viewModel.NatureOfSafeguard = conclusionData.NatureOfSafeguard;
                }

                viewModel.ContinuingEngagementRiskRated = conclusionData.ContinuingEngagementRiskRated;
                viewModel.SafeguardReviewPartnerAssigned = conclusionData.SafeguardReviewPartnerAssigned;

                if (viewModel.IsSuspiciousTransactionReportFiled == true)
                {
                    viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                }

                viewModel.EngagementRetainedRejected = conclusionData.EngagementRetainedRejected;
                viewModel.EMPreparedBy = conclusionData.EMPreparedBy;
                viewModel.EMPreparedByDate = conclusionData.EMPreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;

                if (conclusionData.EPHODApprovedByDate != null)
                {
                    viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                }

                if (conclusionData.MPHODQMPApprovedByDate != null)
                {
                    viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;
                }

                return View("~/Views/General/QC7/EditQC7Form.cshtml", viewModel);
            }
            catch
            {
                if (roles.Contains("Admin"))
                {
                    // Redirect to admin-specific page
                    return RedirectToAction("QC7FormApprovalManagement");
                }
                else
                {
                    // Redirect to user-specific page
                    return RedirectToAction("QC7FormManagement");
                }
            }
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> UpdateQC7Form(QC7FormCreationViewModel viewModel)
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

                return View("~/Views/General/QC7/EditQC7Form.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    // Re-validate form inputs for QC7 Form
                    if (viewModel.IsPublicInterestEntity == true)
                    {
                        viewModel.PublicInterestEntityType = null;
                    }

                    // Retrieve the existing QC7 form from the database
                    var qc7form = await _context.QC7Forms.FindAsync(int.Parse(viewModel.QC7FormID));

                    // Update the existing QC7Form with new values
                    qc7form.Client = viewModel.Client;
                    qc7form.PeriodEnded = viewModel.PeriodEnded.Value;
                    qc7form.EngagementType = viewModel.EngagementType;
                    qc7form.PreparedBy = viewModel.PreparedBy;
                    qc7form.PreparedByDate = viewModel.PreparedByDate;
                    qc7form.ReviewedBy = viewModel.ReviewedBy;
                    qc7form.ReviewedByDate = viewModel.ReviewedByDate;
                    qc7form.PriorYearFee = viewModel.PriorYearFee.Value;
                    qc7form.TimeCosts = viewModel.TimeCosts.Value;
                    qc7form.PriorYearRecoveryRate = 500m;
                    qc7form.AnyOutstandingUnpaidAuditFees = viewModel.AnyOutstandingUnpaidAuditFees;
                    qc7form.TypeOfClientActivities = viewModel.TypeOfClientActivities;
                    qc7form.RiskRatingPriorYear = viewModel.RiskRatingPriorYear;
                    qc7form.AnySuspiciousTransactionReportFiled = viewModel.AnySuspiciousTransactionReportFiled;
                    qc7form.SuspiciousTransactionReportFiledComment = viewModel.SuspiciousTransactionReportFiledComment;
                    qc7form.SafeguardReviewerName = viewModel.SafeguardReviewerName;
                    qc7form.AnyOutstandingUnpaidNonAuditFees = viewModel.AnyOutstandingUnpaidNonAuditFees;
                    qc7form.FeeConcentration = viewModel.FeeConcentration;
                    qc7form.ProposedFeeCurrentYear = viewModel.ProposedFeeCurrentYear.Value;
                    qc7form.BudgetedTimeCost = viewModel.BudgetedTimeCost.Value;
                    qc7form.ProposedRecoveryRateCurrentYear = viewModel.ProposedRecoveryRateCurrentYear.Value;
                    qc7form.IsPublicInterestEntity = viewModel.IsPublicInterestEntity;
                    qc7form.PublicInterestEntityType = viewModel.PublicInterestEntityType;
                    qc7form.IsSubForm2NotApplicable = viewModel.SubForm1NotApplicable;
                    qc7form.IsSubForm3NotApplicable = viewModel.SubForm2NotApplicable;

                    // Reset status to "Pending" if previously rejected
                    qc7form.Status = "Pending";

                    // Update QC7FormConclusion
                    var qc7formConclusion = await _context.QC7FormConclusions.FirstOrDefaultAsync(c => c.QC7FormID == qc7form.QC7FormID);
                    if (qc7formConclusion != null)
                    {
                        qc7formConclusion.AnyRiskAssociated = viewModel.AnyRiskAssociated;

                        if (viewModel.AnyRiskAssociated == true)
                        {
                            qc7formConclusion.RiskExplanationCurrentYearPriorYear = viewModel.RiskExplanationCurrentYearPriorYear;
                            qc7formConclusion.IsSafeguardApplied = viewModel.IsSafeguardApplied;
                            qc7formConclusion.NatureOfSafeguard = viewModel.NatureOfSafeguard;
                        }

                        qc7formConclusion.ContinuingEngagementRiskRated = viewModel.ContinuingEngagementRiskRated;
                        qc7formConclusion.SafeguardReviewPartnerAssigned = viewModel.SafeguardReviewPartnerAssigned;
                        qc7formConclusion.IsSuspiciousTransactionReportFiled = viewModel.IsSuspiciousTransactionReportFiled;

                        if (viewModel.IsSuspiciousTransactionReportFiled == true)
                        {
                            viewModel.SuspiciousTransactionReportFiledRationale = viewModel.SuspiciousTransactionReportFiledRationale;
                        }

                        qc7formConclusion.EngagementRetainedRejected = viewModel.EngagementRetainedRejected;
                        /*                      
        *                      qc6formConclusion.PreparedBy = viewModel.ConclusionPreparedBy;
                            qc6formConclusion.PreparedByDate = viewModel.ConclusionPreparedByDate.Value;
                            qc6formConclusion.EPHODApprovedBy = viewModel.EPHODApprovedBy;
                            qc6formConclusion.MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy;
                        */

                        // Reset the approval dates on every update request 
                        qc7formConclusion.EPHODApprovedByDate = null;
                        qc7formConclusion.MPHODQMPApprovedByDate = null;
                    }

                    // Update QC7FormTest entities
                    foreach (var subForm in viewModel.SubForms)
                    {
                        foreach (var objective in subForm.Objectives)
                        {
                            foreach (var testDescription in objective.TestDescriptions)
                            {
                                var qc7formTest = await _context.QC7FormTests
                                    .FirstOrDefaultAsync(t => t.QC7FormID == qc7form.QC7FormID && t.QC7FormTestDescriptionID == testDescription.QC7FormTestDescriptionID);

                                // Populate the TestDescriptions with posted data
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignBy"];
                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                {
                                    // Use the parsed date
                                    testDescription.SignDate = date;
                                }
                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].Comment"];

                                if (qc7formTest != null)
                                {
                                    qc7formTest.SignOffDate = testDescription.SignDate;
                                    qc7formTest.SignOffBy = testDescription.SignBy;
                                    qc7formTest.Comments = testDescription.Comment;
                                }
                                else
                                {
                                    qc7formTest = new QC7FormTest
                                    {
                                        QC7FormID = qc7form.QC7FormID,
                                        QC7FormTestDescriptionID = testDescription.QC7FormTestDescriptionID,
                                        SignOffDate = testDescription.SignDate,
                                        SignOffBy = testDescription.SignBy,
                                        Comments = testDescription.Comment
                                    };
                                    _context.Add(qc7formTest);
                                }
                            }
                        }
                    }

                    // Save all changes
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    // Set the success message for the toast notification
                    TempData["QC7UpdateMessage"] = "QC7 Form updated successfully.";

                    if (roles.Contains("Admin"))
                    {
                        // Redirect to admin-specific page
                        return RedirectToAction("QC7FormApprovalManagement");
                    }
                    else
                    {
                        // Redirect to user-specific page
                        return RedirectToAction("QC7FormManagement");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log the error
                    viewModel.ErrorMessage = "Error updating the form, please try again!";
                    return View("~/Views/General/QC7/EditQC7Form.cshtml", viewModel);
                }
            }
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> ViewQC7Form(int id)
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var roles = await _userManager.GetRolesAsync(user);

            try
            {
                var userId = user.Id;

                // Get the selected QC7 Form
                var QC7formData = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel());

/*                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEA7ssessments.FirstOrDefault(e => e.QC7FormID.Equals(id));*/

                // Retrieve TNATNEAssessment Section B data
                //var tnaTNESectionBData = _context.TNATNESectionB.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                //var tnaTNESectionDData = _context.TNATNESectionD.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve Conclusion data
                var conclusionData = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID.Equals(id));

/*                // Retrieve FeeDetail data
                var feeDetailData = _context.QC7FormFeeDetails.Where(e => e.QC7FormID.Equals(id)).ToList();*/

                // Retrieve QC7 Form Test data
                var testData = _context.QC7FormTests.Where(e => e.QC7FormID.Equals(id)).ToList();

                // Loop through each SubForm in the viewModel
                foreach (var subForm in viewModel.SubForms)
                {
                    // Loop through each Objective in the SubForm
                    foreach (var objective in subForm.Objectives)
                    {
                        // Loop through each TestDescription in the Objective
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            // Find the corresponding QC7FormTest data for the TestDescription
                            var test = testData.FirstOrDefault(t => t.QC7FormTestDescriptionID == testDescription.QC7FormTestDescriptionID);

                            if (test != null)
                            {
                                // Populate the TestDescription with QC7FormTest data
                                testDescription.SignBy = test.SignOffBy;
                                testDescription.SignDate = test.SignOffDate.Value;
                                testDescription.Comment = test.Comments;
                            }
                        }
                    }
                }

                // Append new data for QC7Form General Portion
                viewModel.QC7FormID = QC7formData.QC7FormID.ToString();
                viewModel.Status = QC7formData.Status;
                viewModel.RejectionReason = QC7formData.RejectionReason;
                viewModel.FileReference = QC7formData.FileReference;
                viewModel.Client = QC7formData.Client;
                viewModel.PeriodEnded = QC7formData.PeriodEnded;
                viewModel.EngagementType = QC7formData.EngagementType;
                viewModel.PreparedBy = QC7formData.PreparedBy;
                viewModel.PreparedByDate = QC7formData.PreparedByDate;
                viewModel.ReviewedBy = QC7formData.ReviewedBy;
                viewModel.ReviewedByDate = QC7formData.ReviewedByDate;
                viewModel.PriorYearFee = QC7formData.PriorYearFee;
                viewModel.TimeCosts = QC7formData.TimeCosts;
                viewModel.PriorYearRecoveryRate = QC7formData.PriorYearRecoveryRate;
                viewModel.AnyOutstandingUnpaidAuditFees = QC7formData.AnyOutstandingUnpaidAuditFees;
                viewModel.TypeOfClientActivities = QC7formData.TypeOfClientActivities;
                viewModel.RiskRatingPriorYear = QC7formData.RiskRatingPriorYear;
                viewModel.AnySuspiciousTransactionReportFiled = QC7formData.AnySuspiciousTransactionReportFiled;
                viewModel.SuspiciousTransactionReportFiledComment = QC7formData.SuspiciousTransactionReportFiledComment;
                viewModel.SafeguardReviewerName = QC7formData.SafeguardReviewerName;
                viewModel.AnyOutstandingUnpaidNonAuditFees = QC7formData.AnyOutstandingUnpaidNonAuditFees;
                viewModel.FeeConcentration = QC7formData.FeeConcentration;
                viewModel.ProposedFeeCurrentYear = QC7formData.ProposedFeeCurrentYear;
                viewModel.BudgetedTimeCost = QC7formData.BudgetedTimeCost;
                viewModel.ProposedRecoveryRateCurrentYear = QC7formData.ProposedRecoveryRateCurrentYear;
                viewModel.IsPublicInterestEntity = QC7formData.IsPublicInterestEntity;
                viewModel.PublicInterestEntityType = QC7formData.PublicInterestEntityType;
                viewModel.SubForm1NotApplicable = QC7formData.IsSubForm2NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.SubForm2NotApplicable = QC7formData.IsSubForm3NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3

                // Append ConclusionData for QC7FormConclusion
                viewModel.AnyRiskAssociated = conclusionData.AnyRiskAssociated;
                
                if (viewModel.AnyRiskAssociated == true)
                {
                    viewModel.RiskExplanationCurrentYearPriorYear = conclusionData.RiskExplanationCurrentYearPriorYear;
                    viewModel.IsSafeguardApplied = conclusionData.IsSafeguardApplied;
                    viewModel.NatureOfSafeguard = conclusionData.NatureOfSafeguard;
                }
                viewModel.ContinuingEngagementRiskRated = conclusionData.ContinuingEngagementRiskRated;
                viewModel.SafeguardReviewPartnerAssigned = conclusionData.SafeguardReviewPartnerAssigned;

                if (viewModel.IsSuspiciousTransactionReportFiled == true)
                {
                    viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                }

                viewModel.EngagementRetainedRejected = conclusionData.EngagementRetainedRejected;
                viewModel.EMPreparedBy = conclusionData.EMPreparedBy;
                viewModel.EMPreparedByDate = conclusionData.EMPreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;

                if (conclusionData.EPHODApprovedByDate != null)
                {
                    viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                }

                if (conclusionData.MPHODQMPApprovedByDate != null)
                {
                    viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;
                }

                /* Append TNATNEAssessment data for TNATNEAssessmentViewModel
                viewModel.TNATNEAssessment.SectionCEvaluation = tnaTneAssessmentData.SectionCEvaluation;
                viewModel.TNATNEAssessment.SectionB.IsAudit = tnaTNESectionBData.IsAudit;
                viewModel.TNATNEAssessment.SectionB.Q1 = tnaTNESectionBData.Q1;
                viewModel.TNATNEAssessment.SectionB.Q2 = tnaTNESectionBData.Q2;
                viewModel.TNATNEAssessment.SectionB.Q3 = tnaTNESectionBData.Q3;
                viewModel.TNATNEAssessment.SectionB.Q4 = tnaTNESectionBData.Q4;
                viewModel.TNATNEAssessment.SectionB.Q5 = tnaTNESectionBData.Q5;
                viewModel.TNATNEAssessment.SectionD.Q1Comment = tnaTNESectionDData.Q1Comment;
                viewModel.TNATNEAssessment.SectionD.Q2Comment = tnaTNESectionDData.Q2Comment;
                viewModel.TNATNEAssessment.SectionD.Q3Comment = tnaTNESectionDData.Q3Comment;
                viewModel.TNATNEAssessment.SectionD.Q4Comment = tnaTNESectionDData.Q4Comment;
                viewModel.TNATNEAssessment.SectionD.Q5Comment = tnaTNESectionDData.Q5Comment;
                */

/*                // Append FeeDetail data for Services
                foreach (var feeDetail in feeDetailData)
                {
                    viewModel.Services.Add(new FeeDetailViewModel
                    {
                        NatureOfService = feeDetail.NatureOfService,
                        OtherService = feeDetail.OtherService,
                        Fee = feeDetail.Fee
                    });
                }*/
                return View("~/Views/General/QC7/ViewQC7Form.cshtml", viewModel);
            }
            catch
            {
                if (roles.Contains("Admin"))
                {
                    // Redirect to admin-specific page
                    return RedirectToAction("QC7FormApprovalManagement");
                }
                else
                {
                    // Redirect to user-specific page
                    return RedirectToAction("QC7FormManagement");
                }
            }
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

                return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
            }

            // Begin a transaction to ensure atomicity
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    // Re-validate form inputs for QC7 Form
                    if (viewModel.IsPublicInterestEntity == true)
                    {
                        viewModel.PublicInterestEntityType = null;
                    }

                    // QCForm File Reference will contain _NAS for Non-Auditor role creation
                    string fileReference = Helper.GenerateQCFormFileReference();


                    // Save viewModel data to EngagementTable
                    var qc7form = new QC7Form
                    {
                        FileReference = fileReference,
                        CreatedBy = userId,
                        Client = viewModel.Client,
                        PeriodEnded = viewModel.PeriodEnded.Value,
                        EngagementType = viewModel.EngagementType,
                        PreparedBy = viewModel.PreparedBy,
                        PreparedByDate = viewModel.PreparedByDate,
                        ReviewedBy = viewModel.ReviewedBy,
                        ReviewedByDate = viewModel.ReviewedByDate,
                        PriorYearFee = viewModel.PriorYearFee.Value,
                        TimeCosts = viewModel.TimeCosts.Value,
                        PriorYearRecoveryRate = viewModel.PriorYearRecoveryRate.Value,
                        AnyOutstandingUnpaidAuditFees = viewModel.AnyOutstandingUnpaidAuditFees,
                        TypeOfClientActivities = viewModel.TypeOfClientActivities, 
                        RiskRatingPriorYear = viewModel.RiskRatingPriorYear,
                        AnySuspiciousTransactionReportFiled =  viewModel.AnySuspiciousTransactionReportFiled,
                        SuspiciousTransactionReportFiledComment = viewModel.SuspiciousTransactionReportFiledComment,
                        SafeguardReviewerName = viewModel.SafeguardReviewerName,
                        AnyOutstandingUnpaidNonAuditFees = viewModel.AnyOutstandingUnpaidNonAuditFees,
                        FeeConcentration = viewModel.FeeConcentration,
                        ProposedFeeCurrentYear = viewModel.ProposedFeeCurrentYear.Value,
                        BudgetedTimeCost = viewModel.BudgetedTimeCost.Value,
                        ProposedRecoveryRateCurrentYear = viewModel.ProposedRecoveryRateCurrentYear.Value,
                        IsPublicInterestEntity = viewModel.IsPublicInterestEntity,
                        PublicInterestEntityType = viewModel.PublicInterestEntityType,
                        Status = "Pending",
                        FormSubmissionDate = DateTime.Now,
                        IsSubForm2NotApplicable = viewModel.SubForm1NotApplicable, // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                        IsSubForm3NotApplicable = viewModel.SubForm2NotApplicable // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
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
                        EPHODApprovedByDate = viewModel.EPHODApprovedByDate,
                        MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy,
                        MPHODQMPApprovedByDate = viewModel.MPHODQMPApprovedByDate
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
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignBy"];
                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                {
                                    // Use the parsed date
                                    testDescription.SignDate = date;
                                }
                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].Comment"];

                                // Save QC7FormTest
                                var qc7formTest = new QC7FormTest
                                {
                                    QC7FormID = qc7formId,
                                    QC7FormTestDescriptionID = testDescription.QC7FormTestDescriptionID,
                                    SignOffDate = testDescription.SignDate,
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

                    await _emailSender.SendEmailAsync(viewModel.EPHODApprovedBy, "QC7 Form Creation",
$"A new QC7 Form has been created with File Reference: {fileReference} and you've been designated as the first approver. Please login to the Audit Management System to approve or reject the QC7 Form.");

                    // Set the success message for the toast notification
                    TempData["QC7CreateMessage"] = "QC7 Form created successfully.";

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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC7Form/ApproveQC7Form/{id}")]
        public async Task<IActionResult> ApproveQC7Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID == id);
            var conclusion = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID == engagement.QC7FormID);
            if (engagement == null)
            {
                return NotFound();
            }

            try
            {
                // Check if EPHOD approval date is not set
                if (conclusion.EPHODApprovedByDate == null)
                {
                    conclusion.EPHODApprovedByDate = DateTime.Now;

                    // Update the engagement status to "Pending 2nd Approval"
                    engagement.Status = "Pending 2nd Approval";

                    await _emailSender.SendEmailAsync(conclusion.MPHODQMPApprovedBy, "QC7 Form Creation",
$"A new QC7 Form has been approved by: {conclusion.EPHODApprovedBy} and you've been designated as the second approver. Please login to the Audit Management System to approve or reject the QC7 Form.");

                }
                // If EPHOD approval date is already set, check if MPHODQMP approval date is not set
                else if (conclusion.MPHODQMPApprovedByDate == null)
                {
                    conclusion.MPHODQMPApprovedByDate = DateTime.Now;

                    // Update the engagement status to "Pending 2nd Approval"
                    engagement.Status = "Approved";

                    // Clear Rejection Reason
                    engagement.RejectionReason = null;

                    await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC7 Form Creation",
$"Your QC7 Form {engagement.FileReference} has been approved by: {conclusion.EPHODApprovedBy}. Please login to the Audit Management System to view the QC7 Form.");

                }

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
            }


            return RedirectToAction("QC7FormApprovalManagement", "QC7Form");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC7Form/RejectQC7Form/{id}")]
        public async Task<IActionResult> RejectQC7Form(int id)
        {
            // Get the request body as a string
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string requestBody = await reader.ReadToEndAsync();

                // Parse the request body JSON to extract the QC7FormID and RejectionReason
                JObject jsonBody = JObject.Parse(requestBody);
                int qc6FormId = (int)jsonBody["QC7FormID"];
                string rejectionReason = (string)jsonBody["RejectionReason"];

                // Retrieve engagement data from the database
                var engagement = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID == qc6FormId);

                // Retrieve conclusion data from the database
                var conclusion = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID == engagement.QC7FormID);

                if (engagement == null)
                {
                    return NotFound();
                }

                // Update the engagement status to "Rejected"
                engagement.Status = "Rejected";
                engagement.RejectionReason = rejectionReason; // Set the rejection reason

                // Reset the dates to null to repeat approval process
                conclusion.EPHODApprovedByDate = null;
                conclusion.MPHODQMPApprovedByDate = null;

                _context.SaveChanges();

                return RedirectToAction("QC7FormApprovalManagement", "QC7Form");
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
