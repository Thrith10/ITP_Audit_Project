using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
//using PKFAuditManagement.Migrations;
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
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender;
        public QC7FormController(IUserService userService, ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IWebHostEnvironment environment)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _environment = environment; 
        }

        [Authorize(Roles = "User,Non-Auditor")]
        public async Task<IActionResult> QC7FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve continuing engagement data from database
            var engagements = _context.QC7Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC7/QC7FormManagement.cshtml", engagements);
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> QC7FormCreationAsync()
        {
            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve all emails for users in the "Admin" role
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Retrieve QC7Form data
            var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel { UserEmail = userEmail, ClientNames = clientNames, AdminEmails = adminEmails.OrderBy(email => email).ToList(), IsNewForm = true });

            return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> QC7FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var qc7forms = _context.QC7Forms.ToList();

            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve QC7FormConclusions where user is the first approver
            var qc7FormConclusionsFirstApprover = _context.QC7FormConclusions
                                        .Where(c => c.EPHODApprovedBy == userEmail)
                                        .Join(_context.QC7Forms,
                                            conclusion => conclusion.QC7FormID,
                                            form => form.QC7FormID,
                                            (conclusion, form) => new QC7FormCombinedViewModel { Conclusion = conclusion, Form = form })
                                        .ToList();

            // Retrieve QC7FormConclusions where user is the second approver
            var qc7FormConclusionsSecondApprover = _context.QC7FormConclusions
                                        .Where(c => c.MPHODQMPApprovedBy == userEmail)
                                        .Join(_context.QC7Forms,
                                            conclusion => conclusion.QC7FormID,
                                            form => form.QC7FormID,
                                            (conclusion, form) => new QC7FormCombinedViewModel { Conclusion = conclusion, Form = form })
                                        .ToList();

            var viewModel = new QC7FormAdminManagementViewModel
            {
                FirstApproverConclusions = qc7FormConclusionsFirstApprover,
                SecondApproverConclusions = qc7FormConclusionsSecondApprover,
                AllQC7Forms = qc7forms
            };

            return View("~/Views/General/QC7/QC7FormApprovalManagement.cshtml", viewModel);
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

                // Retrieve FeeDetail data
                var feeDetailData = _context.QC7FormFeeDetails.Where(e => e.QC7FormID.Equals(id)).ToList();

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionBs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionDs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve QC7 Form Test data
                var testData = _context.QC7FormTests.Where(e => e.QC7FormID.Equals(id)).ToList();

                // Retrieve client names for display
                var clientNames = await _context.QC6Forms
                                                 .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                                 .Select(c => c.ProspectiveClient) // Select the column with client names
                                                 .Distinct() // Ensure unique client names
                                                 .OrderBy(name => name) // Order client names from A to Z
                                                 .ToListAsync(); // Fetch the ordered list of unique client names

                // Append clientNames to viewModel
                viewModel.ClientNames = clientNames;

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
                viewModel.GrandTotal = qc7formData.GrandTotal;
                viewModel.AuditFee = qc7formData.AuditFee;
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
                viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;

                // Append TNATNEAssessment data for TNATNEAssessmentViewModel
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

                // Clear the existing Services list
                viewModel.Services.Clear();

                // Append FeeDetail data for Services
                foreach (var feeDetail in feeDetailData)
                {
                    viewModel.Services.Add(new FeeDetailViewModel
                    {
                        QC6FormFeeDetailID = feeDetail.QC7FormFeeDetailID,
                        NatureOfService = feeDetail.NatureOfService,
                        OtherService = feeDetail.OtherService,
                        Fee = feeDetail.Fee
                    });
                }

                // Load existing documents
                var documentData = _context.QCDocuments.Where(x => x.QC7FormID == id).FirstOrDefault();

                // Retrieve and set the file path for other documents
                if (documentData != null)
                {
                    viewModel.OtherDocumentsFileName = documentData.FileName;
                }

                return View("~/Views/General/QC7/EditQC7Form.cshtml", viewModel);
            }
            catch (Exception ex)
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

            try
            {
                // Get the current user's ID
                var userId = user?.Id;

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
                qc7form.PriorYearRecoveryRate = viewModel.PriorYearRecoveryRate.Value;
                qc7form.AnyOutstandingUnpaidAuditFees = viewModel.AnyOutstandingUnpaidAuditFees;
                qc7form.TypeOfClientActivities = viewModel.TypeOfClientActivities;
                qc7form.RiskRatingPriorYear = viewModel.RiskRatingPriorYear;
                qc7form.AnySuspiciousTransactionReportFiled = viewModel.AnySuspiciousTransactionReportFiled;
                qc7form.GrandTotal = viewModel.GrandTotal.Value;
                qc7form.AuditFee = viewModel.AuditFee.Value;


                // Set to null if not selected
                if (qc7form.AnySuspiciousTransactionReportFiled == false)
                {
                    qc7form.SuspiciousTransactionReportFiledComment = null;
                }
                else
                {
                    qc7form.SuspiciousTransactionReportFiledComment = viewModel.SuspiciousTransactionReportFiledComment;
                }

                qc7form.SafeguardReviewerName = viewModel.SafeguardReviewerName;
                qc7form.AnyOutstandingUnpaidNonAuditFees = viewModel.AnyOutstandingUnpaidNonAuditFees;
                qc7form.FeeConcentration = viewModel.FeeConcentration.Value;
                qc7form.ProposedFeeCurrentYear = viewModel.ProposedFeeCurrentYear.Value;
                qc7form.BudgetedTimeCost = viewModel.BudgetedTimeCost.Value;
                qc7form.ProposedRecoveryRateCurrentYear = viewModel.ProposedRecoveryRateCurrentYear.Value;
                qc7form.IsPublicInterestEntity = viewModel.IsPublicInterestEntity;

                // Check if Public Interest Entity is selected
                if (viewModel.IsPublicInterestEntity == true)
                {
                    qc7form.PublicInterestEntityType = viewModel.PublicInterestEntityType;
                }
                else
                {
                    qc7form.PublicInterestEntityType = null;
                }

                qc7form.IsSubForm2NotApplicable = viewModel.SubForm1NotApplicable;
                qc7form.IsSubForm3NotApplicable = viewModel.SubForm2NotApplicable;

                // Reset status to "Pending" if previously rejected
                if (qc7form.Status == "Rejected")
                {
                    qc7form.Status = "Pending";
                }

                // Update TNATNEAssessment
                var tnaTneAssessment = await _context.TNATNEAssessments.FirstOrDefaultAsync(a => a.QC7FormID == qc7form.QC7FormID);
                if (tnaTneAssessment != null)
                {
                    tnaTneAssessment.SectionCEvaluation = viewModel.TNATNEAssessment.SectionCEvaluation;

                }

                // Update TNATNESectionB with foreign key linking the two tables (TNATNESectionB and TNATNEAssessment)
                var tnaTNESectionB = await _context.TNATNESectionBs.FirstOrDefaultAsync(b => b.TNATNEAssessmentID == tnaTneAssessment.TNATNEAssessmentID);
                if (tnaTNESectionB != null)
                {
                    tnaTNESectionB.IsAudit = viewModel.TNATNEAssessment.SectionB.IsAudit;

                    if (viewModel.TNATNEAssessment.SectionB.IsAudit == "Audit")
                    {
                        tnaTNESectionB.Q1 = viewModel.TNATNEAssessment.SectionB.Q1;
                        tnaTNESectionB.Q2 = viewModel.TNATNEAssessment.SectionB.Q2;
                        tnaTNESectionB.Q3 = viewModel.TNATNEAssessment.SectionB.Q3;
                        tnaTNESectionB.Q4 = viewModel.TNATNEAssessment.SectionB.Q4;
                        tnaTNESectionB.Q5 = viewModel.TNATNEAssessment.SectionB.Q5;
                    }
                    else
                    {
                        tnaTNESectionB.Q5 = viewModel.TNATNEAssessment.SectionB.Q5;
                    }
                }

                // Update TNATNESectionD with foreign key linking the two tables (TNATNESectionD and TNATNEAssessment)
                var tnaTNESectionD = await _context.TNATNESectionDs.FirstOrDefaultAsync(d => d.TNATNEAssessmentID == tnaTneAssessment.TNATNEAssessmentID);
                if (tnaTNESectionD != null)
                {
                    tnaTNESectionD.Q1Comment = viewModel.TNATNEAssessment.SectionD.Q1Comment;
                    tnaTNESectionD.Q2Comment = viewModel.TNATNEAssessment.SectionD.Q2Comment;
                    tnaTNESectionD.Q3Comment = viewModel.TNATNEAssessment.SectionD.Q3Comment;
                    tnaTNESectionD.Q4Comment = viewModel.TNATNEAssessment.SectionD.Q4Comment;
                    tnaTNESectionD.Q5Comment = viewModel.TNATNEAssessment.SectionD.Q5Comment;
                }

                // Update QC7FormConclusion
                var qc7formConclusion = await _context.QC7FormConclusions.FirstOrDefaultAsync(c => c.QC7FormID == qc7form.QC7FormID);
                if (qc7formConclusion != null)
                {
                    qc7formConclusion.AnyRiskAssociated = viewModel.AnyRiskAssociated;

                    // Set to null if not selected
                    if (viewModel.AnyRiskAssociated == true)
                    {
                        qc7formConclusion.RiskExplanationCurrentYearPriorYear = viewModel.RiskExplanationCurrentYearPriorYear;
                        qc7formConclusion.IsSafeguardApplied = viewModel.IsSafeguardApplied;
                        qc7formConclusion.NatureOfSafeguard = viewModel.NatureOfSafeguard;
                    }
                    else
                    {
                        qc7formConclusion.RiskExplanationCurrentYearPriorYear = null;
                        qc7formConclusion.IsSafeguardApplied = false;
                        qc7formConclusion.NatureOfSafeguard = null;
                    }

                    qc7formConclusion.ContinuingEngagementRiskRated = viewModel.ContinuingEngagementRiskRated;
                    qc7formConclusion.SafeguardReviewPartnerAssigned = viewModel.SafeguardReviewPartnerAssigned;
                    qc7formConclusion.IsSuspiciousTransactionReportFiled = viewModel.IsSuspiciousTransactionReportFiled;

                    // Set to null if not selected
                    if (viewModel.IsSuspiciousTransactionReportFiled == true)
                    {
                        qc7formConclusion.SuspiciousTransactionReportFiledRationale = viewModel.SuspiciousTransactionReportFiledRationale;
                    }
                    else
                    {
                        qc7formConclusion.SuspiciousTransactionReportFiledRationale = null;
                    }

                    qc7formConclusion.EngagementRetainedRejected = viewModel.EngagementRetainedRejected;
                    qc7formConclusion.EPHODApprovedBy = viewModel.EPHODApprovedBy;
                    qc7formConclusion.MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy;
                    qc7formConclusion.EPHODApprovedByDate = viewModel.EPHODApprovedByDate;
                    qc7formConclusion.MPHODQMPApprovedByDate = viewModel.MPHODQMPApprovedByDate;
                }

                // Update QC7FormTest entities
                foreach (var subForm in viewModel.SubForms)
                {
                    // Check if the subform is applicable
                    bool isApplicable = (subForm.QC7SubFormID == 1) ||
                                        (subForm.QC7SubFormID == 2 && !viewModel.SubForm1NotApplicable) ||
                                        (subForm.QC7SubFormID == 3 && !viewModel.SubForm2NotApplicable);

                    foreach (var objective in subForm.Objectives)
                    {
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            var qc7formTest = await _context.QC7FormTests
                                .FirstOrDefaultAsync(t => t.QC7FormID == qc7form.QC7FormID && t.QC7FormTestDescriptionID == testDescription.QC7FormTestDescriptionID);

                            if (isApplicable)
                            {
                                // Populate the TestDescriptions with posted data
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignBy"];
                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].SignDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                {
                                    // Use the parsed date
                                    testDescription.SignDate = date;
                                }
                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC7SubFormID}].Objectives[{objective.QC7FormObjectiveID}].TestDescriptions[{testDescription.QC7FormTestDescriptionID}].Comment"];

                            }
                            else
                            {
                                // SubForm is not applicable, set fields to null or minimum values
                                testDescription.SignBy = null;
                                testDescription.SignDate = DateTime.MinValue; // or use nullable DateTime and set to null if appropriate
                                testDescription.Comment = null;
                            }


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

                // Update QC7FormFeeDetail entities
                foreach (var service in viewModel.Services)
                {
                    var qc7formFeeDetail = await _context.QC7FormFeeDetails
                        .FirstOrDefaultAsync(f => f.QC7FormFeeDetailID == service.QC6FormFeeDetailID);

                    if (qc7formFeeDetail != null)
                    {
                        qc7formFeeDetail.NatureOfService = service.NatureOfService;

                        // Check if selection is Other Non-Audit Services
                        if (service.NatureOfService == "Other Non-Audit Services")
                        {
                            qc7formFeeDetail.OtherService = service.OtherService;
                        }
                        else
                        {
                            qc7formFeeDetail.OtherService = null;
                        }

                        qc7formFeeDetail.Fee = service.Fee.Value;
                    }
                    else
                    {
                        // Check if selection is Other Non-Audit Services
                        if (service.NatureOfService != "Other Non-Audit Services")
                        {
                            service.OtherService = null;
                        }

                        qc7formFeeDetail = new QC7FormFeeDetail
                        {
                            QC7FormID = qc7form.QC7FormID,
                            NatureOfService = service.NatureOfService,
                            OtherService = service.OtherService,
                            Fee = service.Fee.Value
                        };
                        _context.Add(qc7formFeeDetail);
                    }
                }

                // Retrieve and process removed services
                var removedServices = Request.Form["RemovedServices[]"];
                var removedServiceIds = removedServices
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (removedServiceIds.Any())
                {
                    foreach (var id in removedServiceIds)
                    {
                        var serviceToRemove = await _context.QC7FormFeeDetails
                            .FirstOrDefaultAsync(f => f.QC7FormFeeDetailID == id);

                        if (serviceToRemove != null)
                        {
                            _context.QC7FormFeeDetails.Remove(serviceToRemove);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Attempt to parse QC7FormID from the view model
                if (int.TryParse(viewModel.QC7FormID, out int parsedQc7FormId))
                {
                    // Find the existing document
                    var existingDocument = await _context.QCDocuments
                        .FirstOrDefaultAsync(x => x.QC7FormID == parsedQc7FormId);

                    if (viewModel.OtherDocuments != null)
                    {
                        // Generate a unique file name for the new document
                        var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                        // Check if the document exists
                        if (existingDocument != null)
                        {
                            // Construct the file path
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC7Form-OtherDocuments", existingDocument.FileName);

                            // Remove the old file from wwwroot
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }

                            // Update the old document
                            existingDocument.FileName = uniqueFileName;

                            // Save all changes
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // Add to the db context
                            _context.Add(new QCDocument
                            {
                                QC6FormID = parsedQc7FormId,
                                FileName = uniqueFileName,
                                DocumentType = "OtherDocuments"
                            });

                            // Save all changes
                            await _context.SaveChangesAsync();
                        }

                        // Define the path for the new document
                        var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC7Form-OtherDocuments", uniqueFileName);

                        // Save the new document to the wwwroot folder
                        using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                        {
                            viewModel.OtherDocuments.CopyTo(fileStream);
                        }
                    }

                    // Remove the old document record if delete is requested and no new file is uploaded
                    if (viewModel.DeleteExistingFile && viewModel.OtherDocuments == null)
                    {
                        // Construct the file path
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC7Form-OtherDocuments", existingDocument.FileName);

                        // Remove the old file from wwwroot
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                        _context.QCDocuments.Remove(existingDocument);
                        await _context.SaveChangesAsync();
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Set the success message for the toast notification
                TempData["QC7UpdateMessage"] = "QC7 Form updated successfully.";

                // List of recipients
                var recipients = new List<string>
                    {
                        viewModel.EPHODApprovedBy,
                        viewModel.MPHODQMPApprovedBy
                    };

                // Subject and body of the email
                var subject = "QC7 Form Update";
                var body = $"The QC7 Form {viewModel.FileReference} has been updated. Please login to the Audit Management System to review the QC7 Form.";

                // Send the email to each recipient
                foreach (var recipient in recipients)
                {
                    await _emailSender.SendEmailAsync(recipient, subject, body);
                }

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
                // Log the error
                viewModel.ErrorMessage = "Error updating the form, please try again!";
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

        public async Task<IActionResult> RetrievePastQC7Data(string selectedClient)
        {
            // Populate Sub Forms
            var viewModel = RetrieveSubFormData(new QC7FormCreationViewModel());

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

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Append client names to viewModel
            viewModel.ClientNames = clientNames;

            // Set selection so that it doesn't get refreshed
            viewModel.SelectedClient = selectedClient;

            // Retrieve QC7Form data from the database
            try
            {
                // Get the most recent QC7 Form that matches the clientName based on ApprovalDate from QC7FormsConclusion
                var qc7formData = (from form in _context.QC7Forms
                                   join conclusion in _context.QC7FormConclusions
                                   on form.QC7FormID equals conclusion.QC7FormID
                                   where form.Client == selectedClient
                                   orderby conclusion.MPHODQMPApprovedByDate descending
                                   select form)
                                         .FirstOrDefault();

                if (qc7formData == null)
                {
                    viewModel.ErrorMessage = "No past QC7 data found for the selected client!";

                    // Loop through each SubForm in the viewModel
                    foreach (var subForm in viewModel.SubForms)
                    {
                        // Loop through each Objective in the SubForm
                        foreach (var objective in subForm.Objectives)
                        {
                            // Loop through each TestDescription in the Objective
                            foreach (var testDescription in objective.TestDescriptions)
                            {
                                // Populate the TestDescription with QC7FormTest data
                                testDescription.SignBy = viewModel.UserEmail;
                                testDescription.SignDate = DateTime.Now;
                            }
                        }
                    }

                    return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
                }

                // Get the selected QC7 Form ID 

                var id = qc7formData.QC7FormID;

                // Retrieve Conclusion data
                var conclusionData = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve FeeDetail data
                var feeDetailData = _context.QC7FormFeeDetails.Where(e => e.QC7FormID.Equals(id)).ToList();

                // Append emails to viewModel
                viewModel.AdminEmails = adminEmails.OrderBy(email => email).ToList();

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionBs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionDs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

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
                viewModel.QC7FormID = qc7formData.QC7FormID.ToString();
                viewModel.Status = qc7formData.Status;
                viewModel.Client = qc7formData.Client;
                viewModel.PeriodEnded = qc7formData.PeriodEnded;
                viewModel.EngagementType = qc7formData.EngagementType;
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
                viewModel.GrandTotal = qc7formData.GrandTotal;
                viewModel.AuditFee = qc7formData.AuditFee;
                viewModel.FeeConcentration = qc7formData.FeeConcentration;
                viewModel.ProposedFeeCurrentYear = qc7formData.ProposedFeeCurrentYear;
                viewModel.BudgetedTimeCost = qc7formData.BudgetedTimeCost;
                viewModel.ProposedRecoveryRateCurrentYear = qc7formData.ProposedRecoveryRateCurrentYear;
                viewModel.IsPublicInterestEntity = qc7formData.IsPublicInterestEntity;
                viewModel.PublicInterestEntityType = qc7formData.PublicInterestEntityType;
                viewModel.SubForm1NotApplicable = qc7formData.IsSubForm2NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.SubForm2NotApplicable = qc7formData.IsSubForm3NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.PreparedBy = qc7formData.PreparedBy;
                viewModel.PreparedByDate = qc7formData.PreparedByDate;
                viewModel.ReviewedBy = qc7formData.ReviewedBy;
                viewModel.ReviewedByDate = qc7formData.ReviewedByDate;

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
                viewModel.IsSuspiciousTransactionReportFiled = conclusionData.IsSuspiciousTransactionReportFiled;

                if (viewModel.IsSuspiciousTransactionReportFiled == true)
                {
                    viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                }

                viewModel.EngagementRetainedRejected = conclusionData.EngagementRetainedRejected;
                viewModel.EMPreparedBy = conclusionData.EMPreparedBy;
                viewModel.EMPreparedByDate = conclusionData.EMPreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;
                viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;

                // Append TNATNEAssessment data for TNATNEAssessmentViewModel
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

                // Clear the existing Services list
                viewModel.Services.Clear();

                // Append FeeDetail data for Services
                foreach (var feeDetail in feeDetailData)
                {
                    viewModel.Services.Add(new FeeDetailViewModel
                    {
                        NatureOfService = feeDetail.NatureOfService,
                        OtherService = feeDetail.OtherService,
                        Fee = feeDetail.Fee
                    });
                }

                return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
            }
            catch
            {
                viewModel.ErrorMessage = "There was an error while handling your request, please try again!";
                return View("~/Views/General/QC7/QC7FormCreation.cshtml", viewModel);
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

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionBs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionDs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve Conclusion data
                var conclusionData = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID.Equals(id));

                // Retrieve FeeDetail data
                var feeDetailData = _context.QC7FormFeeDetails.Where(e => e.QC7FormID.Equals(id)).ToList();

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
                viewModel.GrandTotal = QC7formData.GrandTotal;
                viewModel.AuditFee = QC7formData.AuditFee;
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

                viewModel.IsSuspiciousTransactionReportFiled = conclusionData.IsSuspiciousTransactionReportFiled;
                if (viewModel.IsSuspiciousTransactionReportFiled == true)
                {
                    viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                }

                viewModel.EngagementRetainedRejected = conclusionData.EngagementRetainedRejected;
                viewModel.EMPreparedBy = conclusionData.EMPreparedBy;
                viewModel.EMPreparedByDate = conclusionData.EMPreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;
                viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;

                // Append TNATNEAssessment data for TNATNEAssessmentViewModel
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


                // Clear the existing Services list
                viewModel.Services.Clear();

                // Append FeeDetail data for Services
                foreach (var feeDetail in feeDetailData)
                {
                    viewModel.Services.Add(new FeeDetailViewModel
                    {
                        NatureOfService = feeDetail.NatureOfService,
                        OtherService = feeDetail.OtherService,
                        Fee = feeDetail.Fee
                    });
                }

                // Retrieve documents for QC Form from database
                var documentData = _context.QCDocuments.Where(x => x.QC7FormID.Equals(id)).FirstOrDefault();

                // Retrieve and set the file path for other documents
                if (documentData != null)
                {
                    viewModel.OtherDocumentsFileName = documentData.FileName;
                }

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
        [Authorize(Roles = "User,Non-Auditor,Admin")]
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
                    AnySuspiciousTransactionReportFiled = viewModel.AnySuspiciousTransactionReportFiled,
                    SuspiciousTransactionReportFiledComment = viewModel.SuspiciousTransactionReportFiledComment,
                    SafeguardReviewerName = viewModel.SafeguardReviewerName,
                    AnyOutstandingUnpaidNonAuditFees = viewModel.AnyOutstandingUnpaidNonAuditFees,
                    GrandTotal = viewModel.GrandTotal.Value,
                    AuditFee = viewModel.AuditFee.Value,
                    FeeConcentration = viewModel.FeeConcentration.Value,
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

                var tnaTneAssessment = new TNATNEAssessment
                {
                    QC7FormID = qc7formId,
                    SectionCEvaluation = viewModel.TNATNEAssessment.SectionCEvaluation,
                };

                // Add tnaTneAssessment data to the database
                _context.Add(tnaTneAssessment);
                _context.SaveChanges();

                // Retrieve the tnaTneAssessmentID from the saved entity
                int tnaTneAssessmentID = tnaTneAssessment.TNATNEAssessmentID;

                var tnaTNESectionB = new TNATNESectionB
                {
                    TNATNEAssessmentID = tnaTneAssessmentID,
                    IsAudit = viewModel.TNATNEAssessment.SectionB.IsAudit
                };

                // Save Q1 - Q4 if IsAudit == true, else only Save Q5
                if (viewModel.TNATNEAssessment.SectionB.IsAudit == "Audit")
                {
                    tnaTNESectionB.Q1 = viewModel.TNATNEAssessment.SectionB.Q1;
                    tnaTNESectionB.Q2 = viewModel.TNATNEAssessment.SectionB.Q2;
                    tnaTNESectionB.Q3 = viewModel.TNATNEAssessment.SectionB.Q3;
                    tnaTNESectionB.Q4 = viewModel.TNATNEAssessment.SectionB.Q4;
                    tnaTNESectionB.Q5 = viewModel.TNATNEAssessment.SectionB.Q5;
                }
                else
                {
                    tnaTNESectionB.Q5 = viewModel.TNATNEAssessment.SectionB.Q5;
                }

                // Add tnaTNESectionB data to the database
                _context.Add(tnaTNESectionB);
                _context.SaveChanges();

                var tnaTNESectionD = new TNATNESectionD
                {
                    TNATNEAssessmentID = tnaTneAssessmentID,
                    Q1Comment = viewModel.TNATNEAssessment.SectionD.Q1Comment,
                    Q2Comment = viewModel.TNATNEAssessment.SectionD.Q2Comment,
                    Q3Comment = viewModel.TNATNEAssessment.SectionD.Q3Comment,
                    Q4Comment = viewModel.TNATNEAssessment.SectionD.Q4Comment,
                    Q5Comment = viewModel.TNATNEAssessment.SectionD.Q5Comment,
                };

                // Add tnaTNESectionD data to the database
                _context.Add(tnaTNESectionD);
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

                // Process the submitted data
                foreach (var service in viewModel.Services)
                {
                    // Save QC7FormFeeDetail
                    var qC7FormFeeDetail = new QC7FormFeeDetail
                    {
                        QC7FormID = qc7formId,
                        NatureOfService = service.NatureOfService,
                        Fee = service.Fee.Value,
                        OtherService = service.OtherService,
                    };

                    // Add qC7FormFeeDetail to the context and save changes
                    _context.Add(qC7FormFeeDetail);
                }

                _context.SaveChanges();

                // Root folder name in S3
                var uploadsRootFolder = "QC6FormDocuments";

                // Check if null
                if (viewModel.OtherDocuments != null)
                {
                    // Generate a unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                    // Get the path to wwwroot
                    var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "QC7Form-OtherDocuments");

                    // Ensure the uploads folder exists
                    Directory.CreateDirectory(uploadsFolder);

                    // Get file path
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to wwwroot/uploads/OtherDocuments
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.OtherDocuments.CopyToAsync(stream);
                    }

                    // Add to the db context
                    _context.Add(new QCDocument
                    {
                        QC7FormID = qc7formId,
                        FileName = uniqueFileName,
                        DocumentType = "OtherDocuments"
                    });

                    _context.SaveChanges();
                }

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
                // Log the error
                viewModel.ErrorMessage = "An error occurred while processing your request. Please try again.";
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
        }

        [HttpDelete]
        [Authorize(Roles = "User,Admin,Non-Auditor")]
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

            // Get the current user's email
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var currentUserEmail = user?.Email;

            try
            {
                // Check if EPHOD approval is not set
                if (conclusion.IsFirstApproved == false)
                {                    // Check if current user is the first approver
                    if (currentUserEmail != conclusion.EPHODApprovedBy)
                    {
                        return Forbid();
                    }
                    else
                    {
                        conclusion.IsFirstApproved = true;

                        // Update the engagement status to "Pending 2nd Approval"
                        engagement.Status = "Pending 2nd Approval";

                        _context.SaveChanges();

                        // Send email to creator to notify on approval
                        await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC7 Form Creation",
                            $"Your new QC7 Form has been approved by: {conclusion.EPHODApprovedBy}. The QC7 Form is awaiting the second approval.");

                        // Send email to 2nd approver on action to take
                        await _emailSender.SendEmailAsync(conclusion.MPHODQMPApprovedBy, "QC7 Form Creation",
                            $"A new QC7 Form {engagement.FileReference} has been approved by: {conclusion.EPHODApprovedBy} and you've been designated as the second approver. Please login to the Audit Management System to approve or reject the QC7 Form.");

                        return Ok(new { success = true, message = "The QC7 Form has been approved." });
                    }
                }
                // If EPHOD approval is already set, check if MPHODQMP approval is not set
                else if (conclusion.IsSecondApproved == false)
                {
                    // Check if current user is the second approver
                    if (currentUserEmail != conclusion.MPHODQMPApprovedBy)
                    {
                        return Forbid();
                    }
                    else
                    {
                        conclusion.IsSecondApproved = true;

                        // Update the engagement status to "Approved"
                        engagement.Status = "Approved";

                        // Clear Rejection Reason
                        engagement.RejectionReason = null;

                        _context.SaveChanges();

                        // Send email to creator to notify on creation
                        await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC7 Form Creation",
                            $"Your QC7 Form {engagement.FileReference} has been approved by: {conclusion.EPHODApprovedBy}. Please login to the Audit Management System to view the QC7 Form.");

                        // List of approvers
                        var recipients = new List<string>
                        {
                        conclusion.EPHODApprovedBy,
                        conclusion.MPHODQMPApprovedBy
                        };

                        // Subject and body of the email
                        var subject = "QC7 Form Update";
                        var body = $"The QC7 Form {engagement.FileReference} has been successfully approved and is currently active.";

                        // Send the email to all approvers on the creation of the QC7 form
                        foreach (var recipient in recipients)
                        {
                            await _emailSender.SendEmailAsync(recipient, subject, body);
                        }

                        return Ok(new { success = true, message = "The QC7 Form has been approved." });
                    }
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
                int qc7FormId = (int)jsonBody["QC7FormID"];
                string rejectionReason = (string)jsonBody["RejectionReason"];
                    
                // Retrieve engagement data from the database
                var engagement = _context.QC7Forms.FirstOrDefault(e => e.QC7FormID == qc7FormId);

                // Retrieve conclusion data from the database
                var conclusion = _context.QC7FormConclusions.FirstOrDefault(e => e.QC7FormID == engagement.QC7FormID);

                if (engagement == null)
                {
                    return NotFound();
                }

                // Get the current user's email
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var currentUserEmail = user?.Email;

                // Check if EPHOD approved is not set
                if (conclusion.IsFirstApproved == false)
                {
                    // Check if current user is the first approver
                    if (currentUserEmail != conclusion.EPHODApprovedBy)
                    {
                        return Forbid();
                    }
                    else
                    {
                        // Update the engagement status to "Rejected"
                        engagement.Status = "Rejected";
                        engagement.RejectionReason = rejectionReason; // Set the rejection reason

                        // Reset the status to false to repeat approval process
                        conclusion.IsFirstApproved = false;
                        conclusion.IsSecondApproved = false;

                        _context.SaveChanges();

                        // Send email to creator to notify on rejection
                        await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC7 Form Creation",
                            $"Your new QC7 Form has been rejected by: {conclusion.EPHODApprovedBy}. Please make the necessary amendments and submit the form.");

                        return Ok(new { success = true, message = "The QC7 Form has been rejected." });
                    }
                }
                else if (conclusion.IsSecondApproved == false)
                {
                    // Check if current user is the second approver
                    if (currentUserEmail != conclusion.MPHODQMPApprovedBy)
                    {
                        return Forbid();
                    }
                    else
                    {
                        // Update the engagement status to "Rejected"
                        engagement.Status = "Rejected";
                        engagement.RejectionReason = rejectionReason; // Set the rejection reason

                        // Reset the status to false to repeat approval process
                        conclusion.IsFirstApproved = false;
                        conclusion.IsSecondApproved = false;

                        _context.SaveChanges();

                        // Send email to creator to notify on rejection
                        await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC7 Form Creation",
                            $"Your new QC7 Form has been rejected by: {conclusion.EPHODApprovedBy}. Please make the necessary amendments and submit the form.");

                        return Ok(new { success = true, message = "The QC7 Form has been rejected." });
                    }
                }

                return BadRequest();
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

        public IActionResult RetrieveNASFeeDetails()
        {
            // Retrieve QC7 Forms where FileReference contains "NAS"
            var qc7Forms = _context.QC7Forms
                .Where(e => e.FileReference.Contains("NAS"))
                .Select(f => f.QC7FormID)
                .ToList();

            // Join QC7Forms with QC7FormFeeDetails
            var feeDetails = _context.QC7FormFeeDetails
                .Where(fd => qc7Forms.Contains(fd.QC7FormID))
                .Join(
                    _context.QC7Forms,
                    fd => fd.QC7FormID,
                    f => f.QC7FormID,
                    (fd, f) => new
                    {
                        fd.QC7FormFeeDetailID,
                        fd.QC7FormID,
                        fd.Fee,
                        fd.NatureOfService,
                        f.FileReference,
                        f.PeriodEnded,
                        f.Status
                    })
                .ToList();

            // Return the combined data as JSON
            return Json(feeDetails);
        }
    }
}
