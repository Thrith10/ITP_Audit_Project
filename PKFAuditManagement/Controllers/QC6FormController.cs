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

        [Authorize(Roles = "Non-Auditor,User")]
        public async Task<IActionResult> QC6FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var userId = user?.Id;

            if (TempData.ContainsKey("ToastMessage"))
            {
                string toastMessage;
                toastMessage = TempData["ToastMessage"].ToString();
            }

            // Retrieve engagement data from database
            var qc6forms = _context.QC6Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC6/QC6FormManagement.cshtml", qc6forms);
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> EditQC6Form(int id)
        {
            try
            {
                // Get the current user's ID
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userId = user.Id;

                // Get the selected QC6 Form
                var qc6formData = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel());

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionB.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionD.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve Conclusion data
                var conclusionData = _context.QC6FormConclusions.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve FeeDetail data
                var feeDetailData = _context.QC6FormFeeDetails.Where(e => e.QC6FormID.Equals(id)).ToList();

                // Retrieve QC6 Form Test data
                var testData = _context.QC6FormTests.Where(e => e.QC6FormID.Equals(id)).ToList();

                // Loop through each SubForm in the viewModel
                foreach (var subForm in viewModel.SubForms)
                {
                    // Loop through each Objective in the SubForm
                    foreach (var objective in subForm.Objectives)
                    {
                        // Loop through each TestDescription in the Objective
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            // Find the corresponding QC6FormTest data for the TestDescription
                            var test = testData.FirstOrDefault(t => t.QC6FormTestDescriptionID == testDescription.QC6FormTestDescriptionID);

                            if (test != null)
                            {
                                // Populate the TestDescription with QC6FormTest data
                                testDescription.SignBy = test.SignOffBy;
                                testDescription.SignDate = test.SignOffDate.Value;
                                testDescription.Comment = test.Comments;
                            }
                        }
                    }
                }

                // Append new data for QC6Form General Portion
                viewModel.FileReference = qc6formData.FileReference;
                viewModel.ProspectiveClient = qc6formData.ProspectiveClient;
                viewModel.PeriodEnded = qc6formData.PeriodEnded;
                viewModel.EngagementType = qc6formData.EngagementType;
                viewModel.PreparedBy = qc6formData.PreparedBy;
                viewModel.PreparedByDate = qc6formData.PreparedByDate;
                viewModel.ReviewedBy = qc6formData.ReviewedBy;
                viewModel.ReviewedByDate = qc6formData.ReviewedByDate.Value;
                viewModel.PKFEntityProposingService = qc6formData.PKFEntityProposingService;
                viewModel.SourceOfReferral = qc6formData.SourceOfReferral;
                viewModel.NatureOfServiceForEstimateFee = qc6formData.NatureOfServiceForEstimateFee;
                viewModel.EstimatedFee = qc6formData.EstimatedFee;
                viewModel.BudgetedTimeCost = qc6formData.BudgetedTimeCost;
                viewModel.BudgetedFeeRecoveryRate = qc6formData.BudgetedFeeRecoveryRate;
                viewModel.OutstandingUnpaidFees = qc6formData.OutstandingUnpaidFees;
                viewModel.GrandTotal = qc6formData.GrandTotal;
                viewModel.AuditFee = qc6formData.AuditFee;
                viewModel.FeeConcentration = qc6formData.FeeConcentration;
                viewModel.ConflictsCheckDone = qc6formData.ConflictsCheckDone;
                viewModel.TypeOfActivities = qc6formData.TypeOfActivities;
                viewModel.ComplexityOfEngagement = qc6formData.ComplexityOfEngagement;
                viewModel.PredecessorAuditor = qc6formData.PredecessorAuditor;
                viewModel.ReasonsForDiscontinuance = qc6formData.ReasonsForDiscontinuance;
                viewModel.IsPublicInterestEntity = qc6formData.PublicInterestEntity;
                viewModel.PublicInterestEntityType = qc6formData.PublicInterestEntityType;
                viewModel.SubForm1NotApplicable = qc6formData.IsSubForm2NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.SubForm2NotApplicable = qc6formData.IsSubForm3NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3

                // Append ConclusionData for QC6FormConclusion
                viewModel.AnySignificantRisk = conclusionData.AnySignificantRisk;
                viewModel.SignificantRiskComment = conclusionData.SignificantRiskComment;
                viewModel.NewEngagementRiskRating = conclusionData.NewEngagementRiskRating;
                viewModel.NewEngagementRiskRatingReason = conclusionData.NewEngagementRiskRatingReason;
                viewModel.EngagementSubjectedTo = conclusionData.EngagementSubjectedTo;
                viewModel.SafeguardReviewerAssigned = conclusionData.SafeguardReviewerAssigned;
                viewModel.IsNewEngagementAcceptance = conclusionData.IsNewEngagementAcceptance;
                viewModel.IsSuspiciousTransactionReportFiled = conclusionData.IsSuspiciousTransactionReportFiled;
                viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                viewModel.Satisfaction = conclusionData.Satisfaction;
                viewModel.ConclusionPreparedBy = conclusionData.PreparedBy;
                viewModel.ConclusionPreparedByDate = conclusionData.PreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;

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
                return View("~/Views/General/QC6/EditQC6Form.cshtml", viewModel);
            }
            catch
            {
                return RedirectToAction("QC6FormManagement");
            }
        }

        [Authorize(Roles = "User,Non-Auditor,Admin")]
        public async Task<IActionResult> ViewQC6Form(int id)
        {
            try
            {
                // Get the current user's ID
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userId = user.Id;

                // Get the selected QC6 Form
                var qc6formData = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel());

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionB.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionD.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve Conclusion data
                var conclusionData = _context.QC6FormConclusions.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve FeeDetail data
                var feeDetailData = _context.QC6FormFeeDetails.Where(e => e.QC6FormID.Equals(id)).ToList();

                // Retrieve QC6 Form Test data
                var testData = _context.QC6FormTests.Where(e => e.QC6FormID.Equals(id)).ToList();

                // Loop through each SubForm in the viewModel
                foreach (var subForm in viewModel.SubForms)
                {
                    // Loop through each Objective in the SubForm
                    foreach (var objective in subForm.Objectives)
                    {
                        // Loop through each TestDescription in the Objective
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            // Find the corresponding QC6FormTest data for the TestDescription
                            var test = testData.FirstOrDefault(t => t.QC6FormTestDescriptionID == testDescription.QC6FormTestDescriptionID);

                            if (test != null)
                            {
                                // Populate the TestDescription with QC6FormTest data
                                testDescription.SignBy = test.SignOffBy;
                                testDescription.SignDate = test.SignOffDate.Value;
                                testDescription.Comment = test.Comments;
                            }
                        }
                    }
                }

                // Append new data for QC6Form General Portion
                viewModel.FileReference = qc6formData.FileReference;
                viewModel.ProspectiveClient = qc6formData.ProspectiveClient;
                viewModel.PeriodEnded = qc6formData.PeriodEnded;
                viewModel.EngagementType = qc6formData.EngagementType;
                viewModel.PreparedBy = qc6formData.PreparedBy;
                viewModel.PreparedByDate = qc6formData.PreparedByDate;
                viewModel.ReviewedBy = qc6formData.ReviewedBy;
                viewModel.ReviewedByDate = qc6formData.ReviewedByDate.Value;
                viewModel.PKFEntityProposingService = qc6formData.PKFEntityProposingService;
                viewModel.SourceOfReferral = qc6formData.SourceOfReferral;
                viewModel.NatureOfServiceForEstimateFee = qc6formData.NatureOfServiceForEstimateFee;
                viewModel.EstimatedFee = qc6formData.EstimatedFee;
                viewModel.BudgetedTimeCost = qc6formData.BudgetedTimeCost;
                viewModel.BudgetedFeeRecoveryRate = qc6formData.BudgetedFeeRecoveryRate;
                viewModel.OutstandingUnpaidFees = qc6formData.OutstandingUnpaidFees;
                viewModel.GrandTotal = qc6formData.GrandTotal;
                viewModel.AuditFee = qc6formData.AuditFee;
                viewModel.FeeConcentration = qc6formData.FeeConcentration;
                viewModel.ConflictsCheckDone = qc6formData.ConflictsCheckDone;
                viewModel.TypeOfActivities = qc6formData.TypeOfActivities;
                viewModel.ComplexityOfEngagement = qc6formData.ComplexityOfEngagement;
                viewModel.PredecessorAuditor = qc6formData.PredecessorAuditor;
                viewModel.ReasonsForDiscontinuance = qc6formData.ReasonsForDiscontinuance;
                viewModel.IsPublicInterestEntity = qc6formData.PublicInterestEntity;
                viewModel.PublicInterestEntityType = qc6formData.PublicInterestEntityType;
                viewModel.SubForm1NotApplicable = qc6formData.IsSubForm2NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3
                viewModel.SubForm2NotApplicable = qc6formData.IsSubForm3NotApplicable; // ViewModel SubForm index starts from 0, while database stored as 1, 2 and 3

                // Append ConclusionData for QC6FormConclusion
                viewModel.AnySignificantRisk = conclusionData.AnySignificantRisk;
                viewModel.SignificantRiskComment = conclusionData.SignificantRiskComment;
                viewModel.NewEngagementRiskRating = conclusionData.NewEngagementRiskRating;
                viewModel.NewEngagementRiskRatingReason = conclusionData.NewEngagementRiskRatingReason;
                viewModel.EngagementSubjectedTo = conclusionData.EngagementSubjectedTo;
                viewModel.SafeguardReviewerAssigned = conclusionData.SafeguardReviewerAssigned;
                viewModel.IsNewEngagementAcceptance = conclusionData.IsNewEngagementAcceptance;
                viewModel.IsSuspiciousTransactionReportFiled = conclusionData.IsSuspiciousTransactionReportFiled;
                viewModel.SuspiciousTransactionReportFiledRationale = conclusionData.SuspiciousTransactionReportFiledRationale;
                viewModel.Satisfaction = conclusionData.Satisfaction;
                viewModel.ConclusionPreparedBy = conclusionData.PreparedBy;
                viewModel.ConclusionPreparedByDate = conclusionData.PreparedByDate;
                viewModel.EPHODApprovedBy = conclusionData.EPHODApprovedBy;
                viewModel.MPHODQMPApprovedBy = conclusionData.MPHODQMPApprovedBy;

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
                return View("~/Views/General/QC6/ViewQC6Form.cshtml", viewModel);
            }
            catch
            {
                return RedirectToAction("QC6FormManagement");
            }
        }

        [Authorize(Roles = "Non-Auditor,User,Admin")]
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

                    // Re-validate form inputs for QC6 Form
                    if (viewModel.IsPublicInterestEntity == true)
                    {
                        viewModel.PublicInterestEntityType = null;
                    }

                    // QCForm File Reference will contain _NAS for Non-Auditor role creation
                    string fileReference = Helper.GenerateQCFormFileReference();

                    if (roles.Contains("Non-Auditor"))
                    {
                        // Modify the fileReference if the "Non-Auditor" role is present
                        fileReference += "_NAS";
                    }

                    // Save viewModel data to QC6Form
                    var qc6form = new QC6Form
                    {
                        FileReference = fileReference,
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

                    var tnaTneAssessment = new TNATNEAssessment
                    {
                        QC6FormID = qc6formId,
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

                    // Set the success message for the toast notification
                    TempData["ToastMessage"] = "QC6 Form created successfully.";

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
        [Authorize(Roles = "Non-Auditor,User,Admin")]
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

                // Identify TNATNEAssessment for QC6Form
                var tnaTNEAssessment = _context.TNATNEAssessments.SingleOrDefault(t => t.QC6FormID == id);

                // Identify TNATNESectionB for TNATNEAssessment
                var tnaTNESectionB = _context.TNATNESectionB.SingleOrDefault(t => t.TNATNEAssessmentID == tnaTNEAssessment.TNATNEAssessmentID);

                // Identify TNATNESectionD for TNATNEAssessment
                var tnaTNESectionD = _context.TNATNESectionD.SingleOrDefault(t => t.TNATNEAssessmentID == tnaTNEAssessment.TNATNEAssessmentID);

                // Delete TNATNESectionB
                _context.TNATNESectionB.Remove(tnaTNESectionB);

                // Delete TNATNESectionD
                _context.TNATNESectionD.Remove(tnaTNESectionD);

                // Delete TNATNEAssessment
                _context.TNATNEAssessments.Remove(tnaTNEAssessment);

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
