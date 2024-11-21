using Azure.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;
using Amazon.S3;
using OpenAI;
using SharpCompress.Common;

namespace PKFAuditManagement.Controllers
{
    public class QC6FormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _environment;
        private readonly IS3Service _s3Service;
        public QC6FormController(IUserService userService, ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IWebHostEnvironment environment, IS3Service s3Service)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _environment = environment;
            _s3Service = s3Service;
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

            // Retrieve engagement data from database
            var qc6forms = _context.QC6Forms.Where(e => e.CreatedBy.Equals(userId) && !e.IsTemplate).ToList();
            return View("~/Views/General/QC6/QC6FormManagement.cshtml", qc6forms);
        }

        [Authorize(Roles = "User,Non-Auditor,Admin,Reviewer")]
        public async Task<IActionResult> EditQC6Form(int id)
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var roles = await _userManager.GetRolesAsync(user);

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            try
            {
                var userId = user.Id;

                // Get the selected QC6 Form
                var qc6formData = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel());

                // Append combined emails to viewModel
                viewModel.AdminEmails = combinedEmails;

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionBs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionDs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

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
                viewModel.QC6FormID = id.ToString();
                viewModel.Status = qc6formData.Status;
                viewModel.RejectionReason = qc6formData.RejectionReason;
                viewModel.FileReference = qc6formData.FileReference;
                viewModel.ProspectiveClient = qc6formData.ProspectiveClient;
                viewModel.PeriodEnded = qc6formData.PeriodEnded;
                viewModel.EngagementType = qc6formData.EngagementType;
                viewModel.Industry = qc6formData.Industry;
                viewModel.PKFEntityProposingService = qc6formData.PKFEntityProposingService;
                viewModel.SourceOfReferral = qc6formData.SourceOfReferral;
                viewModel.NatureOfServiceForEstimateFee = qc6formData.NatureOfServiceForEstimateFee;
                viewModel.EstimatedFee = qc6formData.EstimatedFee;
                viewModel.BudgetedTimeCost = qc6formData.BudgetedTimeCost;
                viewModel.BudgetedFeeRecoveryRate = qc6formData.BudgetedFeeRecoveryRate;
                viewModel.BudgetedFeeRecoveryRateComment = qc6formData.BudgetedFeeRecoveryRateComment;
                viewModel.OutstandingUnpaidFees = qc6formData.OutstandingUnpaidFees;

                // Check if Outstanding Unpaid Fees is true
                if (viewModel.OutstandingUnpaidFees == true)
                {
                    viewModel.OutstandingUnpaidFeesComment = qc6formData.OutstandingUnpaidFeesComment;
                }

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
                        QC6FormFeeDetailID = feeDetail.QC6FormFeeDetailID,
                        NatureOfService = feeDetail.NatureOfService,
                        OtherService = feeDetail.OtherService,
                        Fee = feeDetail.Fee
                    });
                }

                // Retrieve documents for QC Form from database
                var documentData = _context.QCDocuments.Where(x => x.QC6FormID.Equals(id)).ToList();

                // Retrieve and set the file path for other documents
                if (documentData != null && documentData.Count > 0)
                {
                    // Check for "OtherDocuments" and set the OtherDocumentsFileName
                    var otherDocument = documentData.FirstOrDefault(doc => doc.DocumentType == "OtherDocuments");
                    if (otherDocument != null)
                    {
                        viewModel.OtherDocumentsFileName = otherDocument.FileName;
                    }
                    else
                    {
                        viewModel.OtherDocumentsFileName = string.Empty;
                    }

                    // Populate the AdditionalDocuments list
                    viewModel.AdditionalDocuments = documentData
                        .Where(doc => doc.DocumentType != "OtherDocuments") // Exclude "OtherDocuments"
                        .Select(doc => new DocumentViewModel
                        {
                            DocumentName = doc.DocumentType,
                            DocumentFileName = doc.FileName
                        }).ToList();
                }

                return View("~/Views/General/QC6/EditQC6Form.cshtml", viewModel);
            }
            catch
            {
                if (roles.Contains("Admin"))
                {
                    // Redirect to admin-specific page
                    return RedirectToAction("QC6FormApprovalManagement");
                }
                else
                {
                    // Redirect to user-specific page
                    return RedirectToAction("QC6FormManagement");
                }
            }
        }

        [Authorize(Roles = "User,Non-Auditor,Admin,Reviewer")]
        public async Task<IActionResult> UpdateQC6Form(QC6FormCreationViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Populate Sub Forms
            viewModel = RetrieveSubFormData(viewModel);

            // Append UserEmail to viewModel
            viewModel.UserEmail = user.Email;

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Append combined emails to viewModel
            viewModel.AdminEmails = combinedEmails;

            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC6/EditQC6Form.cshtml", viewModel);
            }

            try
            {
                // Get the current user's ID
                var userId = user?.Id;

                // Retrieve the existing QC6 form from the database
                var qc6form = await _context.QC6Forms.FindAsync(int.Parse(viewModel.QC6FormID));

                // Update the existing QC6Form with new values
                qc6form.ProspectiveClient = viewModel.ProspectiveClient;
                qc6form.PeriodEnded = viewModel.PeriodEnded.Value;
                qc6form.EngagementType = viewModel.EngagementType;
                qc6form.Industry = viewModel.Industry;
                qc6form.PKFEntityProposingService = viewModel.PKFEntityProposingService;
                qc6form.SourceOfReferral = viewModel.SourceOfReferral;
                qc6form.NatureOfServiceForEstimateFee = viewModel.NatureOfServiceForEstimateFee;
                qc6form.EstimatedFee = viewModel.EstimatedFee.Value;
                qc6form.BudgetedTimeCost = viewModel.BudgetedTimeCost.Value;
                qc6form.BudgetedFeeRecoveryRate = viewModel.BudgetedFeeRecoveryRate.Value;

                // Re-validate budgeted fee recovery rate for QC6 Form 
                if (viewModel.BudgetedFeeRecoveryRate >= 30)
                {
                    viewModel.BudgetedFeeRecoveryRateComment = null;
                }

                qc6form.BudgetedFeeRecoveryRateComment = viewModel.BudgetedFeeRecoveryRateComment;
                qc6form.OutstandingUnpaidFees = viewModel.OutstandingUnpaidFees;

                // Check if Outstanding Unpaid Fees is selected
                if (viewModel.OutstandingUnpaidFees == true)
                {
                    qc6form.OutstandingUnpaidFeesComment = viewModel.OutstandingUnpaidFeesComment;
                }
                else
                {
                    qc6form.OutstandingUnpaidFeesComment = null;
                }

                qc6form.GrandTotal = viewModel.GrandTotal.Value;
                qc6form.AuditFee = viewModel.AuditFee.Value;
                qc6form.FeeConcentration = viewModel.FeeConcentration.Value;
                qc6form.ConflictsCheckDone = viewModel.ConflictsCheckDone;
                qc6form.TypeOfActivities = viewModel.TypeOfActivities;
                qc6form.ComplexityOfEngagement = viewModel.ComplexityOfEngagement;
                qc6form.PredecessorAuditor = viewModel.PredecessorAuditor;
                qc6form.ReasonsForDiscontinuance = viewModel.ReasonsForDiscontinuance;
                qc6form.PublicInterestEntity = viewModel.IsPublicInterestEntity;

                // Check if Public Interest Entity is selected
                if (viewModel.IsPublicInterestEntity == true)
                {
                    qc6form.PublicInterestEntityType = viewModel.PublicInterestEntityType;
                }
                else
                {
                    qc6form.PublicInterestEntityType = null;
                }

                qc6form.IsSubForm2NotApplicable = viewModel.SubForm1NotApplicable;
                qc6form.IsSubForm3NotApplicable = viewModel.SubForm2NotApplicable;

                // Reset status to "Pending" if previously rejected
                if (qc6form.Status == "Rejected")
                {
                    qc6form.Status = "Pending";
                }

                // Update TNATNEAssessment
                var tnaTneAssessment = await _context.TNATNEAssessments.FirstOrDefaultAsync(a => a.QC6FormID == qc6form.QC6FormID);
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

                // Update QC6FormConclusion
                var qc6formConclusion = await _context.QC6FormConclusions.FirstOrDefaultAsync(c => c.QC6FormID == qc6form.QC6FormID);
                if (qc6formConclusion != null)
                {
                    qc6formConclusion.AnySignificantRisk = viewModel.AnySignificantRisk;

                    // Check if Significant Risk Checkbox is selected 
                    if (viewModel.AnySignificantRisk == true)
                    {
                        qc6formConclusion.SignificantRiskComment = viewModel.SignificantRiskComment;
                    }
                    else
                    {
                        qc6formConclusion.SignificantRiskComment = null;
                    }

                    qc6formConclusion.NewEngagementRiskRating = viewModel.NewEngagementRiskRating;
                    qc6formConclusion.NewEngagementRiskRatingReason = viewModel.NewEngagementRiskRatingReason;
                    qc6formConclusion.EngagementSubjectedTo = viewModel.EngagementSubjectedTo;
                    qc6formConclusion.SafeguardReviewerAssigned = viewModel.SafeguardReviewerAssigned;
                    qc6formConclusion.IsNewEngagementAcceptance = viewModel.IsNewEngagementAcceptance;
                    qc6formConclusion.IsSuspiciousTransactionReportFiled = viewModel.IsSuspiciousTransactionReportFiled;

                    // Check if Suspicious Transaction Report Filed Checkbox is selected 
                    if (viewModel.IsSuspiciousTransactionReportFiled == true)
                    {
                        qc6formConclusion.SuspiciousTransactionReportFiledRationale = viewModel.SuspiciousTransactionReportFiledRationale;
                    }
                    else
                    {
                        qc6formConclusion.SuspiciousTransactionReportFiledRationale = null;
                    }

                    qc6formConclusion.Satisfaction = viewModel.Satisfaction;
                    qc6formConclusion.EPHODApprovedBy = viewModel.EPHODApprovedBy;
                    qc6formConclusion.MPHODQMPApprovedBy = viewModel.MPHODQMPApprovedBy;
                    qc6formConclusion.EPHODApprovedByDate = viewModel.EPHODApprovedByDate;
                    qc6formConclusion.MPHODQMPApprovedByDate = viewModel.MPHODQMPApprovedByDate;
                }

                // Update QC6FormTest entities
                foreach (var subForm in viewModel.SubForms)
                {
                    // Check if the subform is applicable
                    bool isApplicable = (subForm.QC6SubFormID == 1) ||
                                        (subForm.QC6SubFormID == 2 && !viewModel.SubForm1NotApplicable) ||
                                        (subForm.QC6SubFormID == 3 && !viewModel.SubForm2NotApplicable);

                    foreach (var objective in subForm.Objectives)
                    {
                        foreach (var testDescription in objective.TestDescriptions)
                        {
                            var qc6formTest = await _context.QC6FormTests
                                .FirstOrDefaultAsync(t => t.QC6FormID == qc6form.QC6FormID && t.QC6FormTestDescriptionID == testDescription.QC6FormTestDescriptionID);

                            if (isApplicable)
                            {
                                // Populate the TestDescriptions with posted data
                                testDescription.SignBy = HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].SignBy"];

                                if (DateTime.TryParseExact(HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].SignDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                {
                                    // Use the parsed date
                                    testDescription.SignDate = date;
                                }

                                testDescription.Comment = HttpContext.Request.Form[$"SubForms[{subForm.QC6SubFormID}].Objectives[{objective.QC6FormObjectiveID}].TestDescriptions[{testDescription.QC6FormTestDescriptionID}].Comment"];
                            }
                            else
                            {
                                // SubForm is not applicable, set fields to null or minimum values
                                testDescription.SignBy = null;
                                testDescription.SignDate = DateTime.MinValue; // or use nullable DateTime and set to null if appropriate
                                testDescription.Comment = null;
                            }

                            if (qc6formTest != null)
                            {
                                qc6formTest.SignOffDate = testDescription.SignDate;
                                qc6formTest.SignOffBy = testDescription.SignBy;
                                qc6formTest.Comments = testDescription.Comment;
                            }
                            else
                            {
                                qc6formTest = new QC6FormTest
                                {
                                    QC6FormID = qc6form.QC6FormID,
                                    QC6FormTestDescriptionID = testDescription.QC6FormTestDescriptionID,
                                    SignOffDate = testDescription.SignDate,
                                    SignOffBy = testDescription.SignBy,
                                    Comments = testDescription.Comment
                                };
                                _context.Add(qc6formTest);
                            }
                        }
                    }
                }

                // Update QC6FormFeeDetail entities
                foreach (var service in viewModel.Services)
                {
                    var qc6formFeeDetail = await _context.QC6FormFeeDetails
                        .FirstOrDefaultAsync(f => f.QC6FormFeeDetailID == service.QC6FormFeeDetailID);

                    if (qc6formFeeDetail != null)
                    {
                        qc6formFeeDetail.NatureOfService = service.NatureOfService;

                        // Check if selection is Other Non-Audit Services
                        if (service.NatureOfService == "Other Non-Audit Services")
                        {
                            qc6formFeeDetail.OtherService = service.OtherService;
                        }
                        else
                        {
                            qc6formFeeDetail.OtherService = null;
                        }

                        qc6formFeeDetail.Fee = service.Fee.Value;
                    }
                    else
                    {
                        // Check if selection is Other Non-Audit Services
                        if (service.NatureOfService != "Other Non-Audit Services")
                        {
                            service.OtherService = null;
                        }

                        qc6formFeeDetail = new QC6FormFeeDetail
                        {
                            QC6FormID = qc6form.QC6FormID,
                            NatureOfService = service.NatureOfService,
                            OtherService = service.OtherService,
                            Fee = service.Fee.Value
                        };
                        _context.Add(qc6formFeeDetail);
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
                        var serviceToRemove = await _context.QC6FormFeeDetails
                            .FirstOrDefaultAsync(f => f.QC6FormFeeDetailID == id);

                        if (serviceToRemove != null)
                        {
                            _context.QC6FormFeeDetails.Remove(serviceToRemove);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Attempt to parse QC6FormID from the view model
                if (int.TryParse(viewModel.QC6FormID, out int parsedQc6FormId))
                {
                    // Find the existing document for Other Documents
                    var existingDocument = await _context.QCDocuments
                        .FirstOrDefaultAsync(x => x.QC6FormID == parsedQc6FormId && x.DocumentType == "OtherDocuments");

                    if (viewModel.OtherDocuments != null)
                    {
                        // Generate a unique file name for the new document
                        var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                        // Check if the document exists
                        if (existingDocument != null)
                        {
                            // Construct the file path
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-OtherDocuments", existingDocument.FileName);

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
                                QC6FormID = parsedQc6FormId,
                                FileName = uniqueFileName,
                                DocumentType = "OtherDocuments"
                            });

                            // Save all changes
                            await _context.SaveChangesAsync();
                        }

                        // Define the path for the new document
                        var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-OtherDocuments", uniqueFileName);

                        // Save the new document to the wwwroot folder
                        using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                        {
                            viewModel.OtherDocuments.CopyTo(fileStream);
                        }
                    }

                    // Handle deletion of files if requested
                    if (viewModel.DeleteExistingFile && viewModel.OtherDocuments == null)
                    {
                        // Find and delete the existing Other Document if requested
                        if (existingDocument != null)
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-OtherDocuments", existingDocument.FileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            _context.QCDocuments.Remove(existingDocument);

                            // Save all changes
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Handle AdditionalDocuments
                    if (viewModel.AdditionalDocuments != null && viewModel.AdditionalDocuments.Count > 0)
                    {
                        foreach (var document in viewModel.AdditionalDocuments)
                        {
                            // Find the existing document for AdditionalDocuments
                            var existingAdditionalDoc = await _context.QCDocuments
                                .FirstOrDefaultAsync(x => x.QC6FormID == parsedQc6FormId && x.DocumentType == document.OldDocumentName);

                            // Generate a unique file name by including the QC6Form ID and document file name
                            var uniqueFileName = parsedQc6FormId + "-" + document.DocumentName + "-" + Guid.NewGuid().ToString() + ".pdf";

                            // Generate the new folder path based on the updated document name
                            var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-AdditionalDocuments", uniqueFileName);

                            // Check if a file is uploaded
                            if (document.File != null && document.File.Length > 0)
                            {
                                // Handle existing document update or new document creation
                                if (existingAdditionalDoc != null)
                                {
                                    // Generate the old folder path
                                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-AdditionalDocuments/" + existingAdditionalDoc.FileName);

                                    // Remove the old file from the folder
                                    if (System.IO.File.Exists(oldFilePath))
                                    {
                                        System.IO.File.Delete(oldFilePath);
                                    }

                                    // Update document metadata
                                    existingAdditionalDoc.FileName = uniqueFileName;
                                    existingAdditionalDoc.DocumentType = document.DocumentName;
                                }
                                else
                                {
                                    // Add a new document if it doesn't exist
                                    _context.Add(new QCDocument
                                    {
                                        QC6FormID = parsedQc6FormId,
                                        FileName = uniqueFileName,
                                        DocumentType = document.DocumentName
                                    });
                                }
                                
                                // Save the updated file
                                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                                {
                                    await document.File.CopyToAsync(fileStream);
                                }

                                // Commit database changes
                                await _context.SaveChangesAsync();
                            }
                            // Handle document name change without file upload
                            else if (existingAdditionalDoc != null && document.OldDocumentName != document.DocumentName)
                            {
                                // Generate the old folder path
                                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-AdditionalDocuments/" + existingAdditionalDoc.FileName);

                                // Rename the file in the file system by moving it to the new file name
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Move(oldFilePath, newFilePath);
                                }

                                // Update document metadata with the new name
                                existingAdditionalDoc.DocumentType = document.DocumentName;
                                existingAdditionalDoc.FileName = uniqueFileName;

                                // Save the change in the document name in the database
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    // Check if any existing documents are deleted
                    if (viewModel.DeletedDocumentFilenames != null)
                    {
                        // Split through delimeter , in string
                        List<string> deletedNames = viewModel.DeletedDocumentFilenames.Split(',')
                                                                        .Select(name => name.Trim())
                                                                        .ToList();

                        // Loop through each filename in the list of deleted documents
                        foreach (var deletedFilename in deletedNames)
                        {
                            // Find the existing document for AdditionalDocuments
                            var existingAdditionalDoc = await _context.QCDocuments
                                .FirstOrDefaultAsync(x => x.QC6FormID == parsedQc6FormId && x.FileName == deletedFilename);

                            // Check if the document exists in the database
                            if (existingAdditionalDoc != null)
                            {
                                // If delete is requested and the document exists, remove it
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/QC6Form-AdditionalDocuments", existingAdditionalDoc.FileName);

                                // Remove the old file from wwwroot
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }

                                _context.QCDocuments.Remove(existingAdditionalDoc);
                            }
                        }

                        // Save changes to the database after processing all deletions
                        await _context.SaveChangesAsync();
                    }
                }  

                // Set the success message for the toast notification
                TempData["QC6UpdateMessage"] = "QC6 Form updated successfully.";

                // List of recipients
                var recipients = new List<string>
                    {
                        viewModel.UserEmail,
                        viewModel.EPHODApprovedBy,
                        viewModel.MPHODQMPApprovedBy
                    };

                // Subject and body of the email
                var subject = "QC6 Form Update";
                var body = $"The QC6 Form {viewModel.FileReference} has been updated. Please login to the Audit Management System to review the QC6 Form.";

                // Send the email to each recipient
                foreach (var recipient in recipients)
                {
                    await _emailSender.SendEmailAsync(recipient, subject, body);
                }

                if (roles.Contains("Admin") || roles.Contains("Reviewer"))
                {
                    // Redirect to admin-specific page
                    return RedirectToAction("QC6FormApprovalManagement");
                }
                else
                {
                    // Redirect to user-specific page
                    return RedirectToAction("QC6FormManagement");
                }
            }
            catch (Exception ex)
            {
                // Log the error
                viewModel.ErrorMessage = "Error updating the form, please try again!";
                return View("~/Views/General/QC6/EditQC6Form.cshtml", viewModel);
            }
        }

        [Authorize(Roles = "User,Non-Auditor,Admin,Reviewer")]
        public async Task<IActionResult> ViewQC6Form(int id)
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

                // Get the selected QC6 Form
                var qc6formData = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Populate Sub Forms
                var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel());

                // Retrieve TNATNEAssessment data
                var tnaTneAssessmentData = _context.TNATNEAssessments.FirstOrDefault(e => e.QC6FormID.Equals(id));

                // Retrieve TNATNEAssessment Section B data
                var tnaTNESectionBData = _context.TNATNESectionBs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

                // Retrieve TNATNEAssessment Section D data
                var tnaTNESectionDData = _context.TNATNESectionDs.FirstOrDefault(e => e.TNATNEAssessmentID.Equals(tnaTneAssessmentData.TNATNEAssessmentID));

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
                viewModel.QC6FormID = qc6formData.QC6FormID.ToString();
                viewModel.Status = qc6formData.Status;
                viewModel.RejectionReason = qc6formData.RejectionReason;
                viewModel.FileReference = qc6formData.FileReference;
                viewModel.ProspectiveClient = qc6formData.ProspectiveClient;
                viewModel.PeriodEnded = qc6formData.PeriodEnded;
                viewModel.EngagementType = qc6formData.EngagementType;
                viewModel.Industry = qc6formData.Industry;
                viewModel.PKFEntityProposingService = qc6formData.PKFEntityProposingService;
                viewModel.SourceOfReferral = qc6formData.SourceOfReferral;
                viewModel.NatureOfServiceForEstimateFee = qc6formData.NatureOfServiceForEstimateFee;
                viewModel.EstimatedFee = qc6formData.EstimatedFee;
                viewModel.BudgetedTimeCost = qc6formData.BudgetedTimeCost;
                viewModel.BudgetedFeeRecoveryRate = qc6formData.BudgetedFeeRecoveryRate;
                viewModel.BudgetedFeeRecoveryRateComment = qc6formData.BudgetedFeeRecoveryRateComment;

                viewModel.OutstandingUnpaidFees = qc6formData.OutstandingUnpaidFees;

                // Check if value is true, else don't set the comment 
                if (viewModel.OutstandingUnpaidFees == true)
                {
                    viewModel.OutstandingUnpaidFeesComment = qc6formData.OutstandingUnpaidFeesComment;
                }

                viewModel.GrandTotal = qc6formData.GrandTotal;
                viewModel.AuditFee = qc6formData.AuditFee;
                viewModel.FeeConcentration = qc6formData.FeeConcentration;
                viewModel.ConflictsCheckDone = qc6formData.ConflictsCheckDone;
                viewModel.TypeOfActivities = qc6formData.TypeOfActivities;
                viewModel.ComplexityOfEngagement = qc6formData.ComplexityOfEngagement;
                viewModel.PredecessorAuditor = qc6formData.PredecessorAuditor;
                viewModel.ReasonsForDiscontinuance = qc6formData.ReasonsForDiscontinuance;
                viewModel.IsPublicInterestEntity = qc6formData.PublicInterestEntity;

                // Check if selected, else don't set the type
                if (viewModel.IsPublicInterestEntity == true)
                {
                    viewModel.PublicInterestEntityType = qc6formData.PublicInterestEntityType;
                }

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
                
                if (conclusionData.EPHODApprovedByDate != null)
                {
                    viewModel.EPHODApprovedByDate = conclusionData.EPHODApprovedByDate;
                }

                if (conclusionData.MPHODQMPApprovedByDate != null)
                {
                    viewModel.MPHODQMPApprovedByDate = conclusionData.MPHODQMPApprovedByDate;
                }

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
                var documentData = _context.QCDocuments.Where(x => x.QC6FormID.Equals(id)).ToList();

                // Retrieve and set the file path for other documents
                if (documentData != null && documentData.Count > 0)
                {
                    // Check for "OtherDocuments" and set the OtherDocumentsFileName
                    var otherDocument = documentData.FirstOrDefault(doc => doc.DocumentType == "OtherDocuments");
                    if (otherDocument != null)
                    {
                        viewModel.OtherDocumentsFileName = otherDocument.FileName;
                    }
                    else
                    {
                        viewModel.OtherDocumentsFileName = string.Empty;
                    }

                    // Populate the AdditionalDocuments list
                    viewModel.AdditionalDocuments = documentData
                        .Where(doc => doc.DocumentType != "OtherDocuments") // Exclude "OtherDocuments"
                        .Select(doc => new DocumentViewModel
                        {
                            DocumentName = doc.DocumentType,
                            DocumentFileName = doc.FileName
                        }).ToList();
                }

                return View("~/Views/General/QC6/ViewQC6Form.cshtml", viewModel);
            }
            catch
            {
                if (roles.Contains("Admin") || roles.Contains("Reviewer"))
                {
                    // Redirect to admin-specific page
                    return RedirectToAction("QC6FormApprovalManagement");
                }
                else
                {
                    // Redirect to user-specific page
                    return RedirectToAction("QC6FormManagement");
                }
            }
        }

        [HttpGet]
        public IActionResult GetProspectiveClients(string term)
        {
            var prospectiveClients = _context.QC6Forms
                .Where(q => q.ProspectiveClient.Contains(term)) 
                .Select(q => q.ProspectiveClient)
                .ToList();

            return Json(prospectiveClients);
        }

        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
        public async Task<IActionResult> QC6FormCreationAsync()
        {
            // Path to the SSIC text file in the "uploads" folder
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var industriesFilePath = Path.Combine(uploadsFolder, "SSIC_Codes.txt");

            // Retrieve list of industries
            var industries = System.IO.File.ReadAllLines(industriesFilePath).ToList();

            // List to hold grouped industries (excluding the section headers)
            var groupedIndustries = new List<KeyValuePair<string, List<string>>>();

            // Variables to track the current group
            List<string> currentGroup = null;
            string currentGroupTitle = null;

            foreach (var line in industries)
            {
                // Check if the line starts with a letter (i.e., A, B, C, etc.)
                var firstChar = line.Substring(0, 1).ToUpper();

                // If it's a new group (i.e., a new section header), start a new group
                if (char.IsLetter(firstChar[0]) && (currentGroup == null || !line.StartsWith(currentGroupTitle)))
                {
                    // If a current group exists, add it to the list
                    if (currentGroup != null)
                    {
                        groupedIndustries.Add(new KeyValuePair<string, List<string>>(currentGroupTitle, currentGroup));
                    }

                    // Start a new group with the full section title (e.g., "A AGRICULTURE AND FISHING")
                    currentGroupTitle = line.Trim(); // Use the entire line as the title for the group
                    currentGroup = new List<string>(); // Start fresh for the new group
                }
                else
                {
                    // Add only industry codes under the current group (skip section headers)
                    if (line.Length > 2 && char.IsDigit(line[1])) // Ensure it's a valid industry code line
                    {
                        currentGroup.Add(line);  // Add the industry to the current group
                    }
                }
            }

            // Add the last group (after the loop ends)
            if (currentGroup != null && currentGroup.Count > 0)
            {
                groupedIndustries.Add(new KeyValuePair<string, List<string>>(currentGroupTitle, currentGroup));
            }

            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Retrieve all prospective clients
            var prospectiveClients = await _context.QC6Forms
                .Select(q => q.ProspectiveClient)
                .ToListAsync();

            // Retrieve QC6Form data
            var viewModel = RetrieveSubFormData(new QC6FormCreationViewModel { ProspectiveClients = prospectiveClients, UserEmail = userEmail, AdminEmails = combinedEmails, GroupedIndustries = groupedIndustries });

            return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin,Reviewer")]
        public async Task<IActionResult> QC6FormApprovalManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Check if the user is in the "Admin" role
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Retrieve QC6Form data from the database
            List<QC6Form> qc6forms;

            if (isAdmin)
            {
                // If the user is an Admin, retrieve all forms
                qc6forms = _context.QC6Forms.Where(e => !e.IsTemplate).ToList();
            }
            else
            {
                // Otherwise, retrieve only those created by the current user that are not templates
                qc6forms = _context.QC6Forms.Where(e => !e.IsTemplate && e.CreatedBy.Equals(userId)).ToList();
            }

            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve QC6FormConclusions where user is the first approver
            var qc6FormConclusionsFirstApprover = _context.QC6FormConclusions
                                        .Where(c => c.EPHODApprovedBy == userEmail)
                                        .Join(_context.QC6Forms.Where(form => !form.IsTemplate),
                                            conclusion => conclusion.QC6FormID,
                                            form => form.QC6FormID,
                                            (conclusion, form) => new QC6FormCombinedViewModel { Conclusion = conclusion, Form = form })
                                        .ToList();

            // Retrieve QC6FormConclusions where user is the second approver
            var qc6FormConclusionsSecondApprover = _context.QC6FormConclusions
                                        .Where(c => c.MPHODQMPApprovedBy == userEmail)
                                        .Join(_context.QC6Forms.Where(form => !form.IsTemplate),
                                            conclusion => conclusion.QC6FormID,
                                            form => form.QC6FormID,
                                            (conclusion, form) => new QC6FormCombinedViewModel { Conclusion = conclusion, Form = form })
                                        .ToList();

            var viewModel = new QC6FormAdminManagementViewModel
            {
                FirstApproverConclusions = qc6FormConclusionsFirstApprover,
                SecondApproverConclusions = qc6FormConclusionsSecondApprover,
                AllQC6Forms = qc6forms
            };

            return View("~/Views/General/QC6/QC6FormApprovalManagement.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin,Reviewer")]
        [HttpPost]
        [Route("/QC6Form/ApproveQC6Form/{id}")]
        public async Task<IActionResult> ApproveQC6Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID == id);
            var conclusion = _context.QC6FormConclusions.FirstOrDefault(e => e.QC6FormID == engagement.QC6FormID);
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
                // Check if EPHOD approved is not set
                if (conclusion.IsFirstApproved == false)
                {
                    // Check if current user is the first approver
                    if (currentUserEmail != conclusion.EPHODApprovedBy)
                    {
                        return Forbid();
                    } else
                    {
                        conclusion.IsFirstApproved = true;

                        // Update the engagement status to "Pending 2nd Approval"
                        engagement.Status = "Pending 2nd Approval";

                        _context.SaveChanges();

						// Send email to creator to notify on approval
						await _emailSender.SendEmailAsync(conclusion.PreparedBy, "QC6 Form Approval Notification",
	                        $"<p>Dear {conclusion.PreparedBy},</p>" +
	                        $"<p>Your new QC6 Form <strong>{engagement.FileReference}</strong> has been approved by: <strong>{conclusion.EPHODApprovedBy}</strong>.</p>" +
	                        $"<p>The QC6 Form is now awaiting the second approval.</p>" +
	                        $"<p>If you need further information, please log in to the Audit Management System.</p>" +
	                        $"<p>Thank you!</p>" +
	                        $"<p>Best regards,<br/>" +
	                        $"PKF Team</p>"
                        );

						// Send email to 2nd approver on action to take
						await _emailSender.SendEmailAsync(conclusion.MPHODQMPApprovedBy, "QC6 Form Action Required",
	                        $"<p>Dear {conclusion.MPHODQMPApprovedBy},</p>" +
	                        $"<p>A new QC6 Form <strong>{engagement.FileReference}</strong> has been approved by: <strong>{conclusion.EPHODApprovedBy}</strong>.</p>" +
	                        $"<p>You have been designated as the second approver. Please log in to the System to approve or reject the QC6 Form.</p>" +
	                        $"<p>Thank you for your attention!</p>" +
	                        $"<p>Best regards,<br/>" +
	                        $"PKF Team</p>"
                        );

                        return Ok(new { success = true, message = "The QC6 Form has been approved." });
                    }
                }
                // If EPHOD approval is already set, check if MPHODQMP approval date is not set
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

						// List of approvers
						var recipients = new List<string>
				        {
							conclusion.PreparedBy,
					        conclusion.EPHODApprovedBy,
							conclusion.MPHODQMPApprovedBy
						};

						// Send the email to all approvers on the creation of the QC6 form
						foreach (var recipient in recipients)
						{
							// Subject and body of the email
							var subject = "QC6 Form Approval";
							var body =
								$"<p>Dear {recipient},</p>" +
								$"<p>The QC6 Form <strong>{engagement.FileReference}</strong> has been successfully approved by the second approver: <strong>{conclusion.MPHODQMPApprovedBy}</strong>, and is currently active.</p>" +
								$"<p>If you need further information, please log in to the Audit Management System.</p>" +
								$"<p>Thank you for your attention!</p>" +
								$"<p>Best regards,<br/>" +
								$"PKF Team</p>";

							await _emailSender.SendEmailAsync(recipient, subject, body);
						}

                        return Ok(new { success = true, message = "The QC6 Form has been approved." });
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
        }

        [Authorize(Roles = "Admin,Reviewer")]
        [HttpPost]
        [Route("/QC6Form/RejectQC6Form/{id}")]
        public async Task<IActionResult> RejectQC6Form(int id)
        {
            // Get the request body as a string
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string requestBody = await reader.ReadToEndAsync();

                // Parse the request body JSON to extract the QC6FormID and RejectionReason
                JObject jsonBody = JObject.Parse(requestBody);
                int qc6FormId = (int)jsonBody["QC6FormID"];
                string rejectionReason = (string)jsonBody["RejectionReason"];

                // Retrieve engagement data from the database
                var engagement = _context.QC6Forms.FirstOrDefault(e => e.QC6FormID == qc6FormId);

                // Retrieve conclusion data from the database
                var conclusion = _context.QC6FormConclusions.FirstOrDefault(e => e.QC6FormID == engagement.QC6FormID);

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

						// Send email to creator to notify about the rejection
						await _emailSender.SendEmailAsync(conclusion.PreparedBy, "QC6 Form Rejection Notification",
							$"<p>Dear {conclusion.PreparedBy},</p>" +
							$"<p>Your QC6 Form has been rejected by: <strong>{conclusion.EPHODApprovedBy}</strong>.</p>" +
							$"<p>Please make the necessary amendments and resubmit the form.</p>" +
							$"<p>If you need further clarification, feel free to contact our support team.</p>" +
							$"<p>Thank you for your attention!</p>" +
							$"<p>Best regards,<br/>" +
							$"PKF Team</p>"
						);

						return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
                    }
                } else if (conclusion.IsSecondApproved == false)
                {
                    // Check if current user is the second approver
                    if (currentUserEmail != conclusion.MPHODQMPApprovedBy)
                    {
                        return Forbid();
                    } else
                    {
                        // Update the engagement status to "Rejected"
                        engagement.Status = "Rejected";
                        engagement.RejectionReason = rejectionReason; // Set the rejection reason

                        // Reset the status to false to repeat approval process
                        conclusion.IsFirstApproved = false;
                        conclusion.IsSecondApproved = false;

                        _context.SaveChanges();

						// Send email to creator to notify about the rejection
						await _emailSender.SendEmailAsync(conclusion.PreparedBy, "QC6 Form Rejection Notification",
							$"<p>Dear {conclusion.PreparedBy},</p>" +
							$"<p>Your QC6 Form has been rejected by: <strong>{conclusion.MPHODQMPApprovedBy}</strong>.</p>" +
							$"<p>Please make the necessary amendments and resubmit the form.</p>" +
							$"<p>If you need further clarification, feel free to contact our support team.</p>" +
							$"<p>Thank you for your attention!</p>" +
							$"<p>Best regards,<br/>" +
							$"PKF Team</p>"
						);

						return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
                    }
                }

                return RedirectToAction("QC6FormApprovalManagement", "QC6Form");
            }
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

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Append combined emails to viewModel
            viewModel.AdminEmails = combinedEmails;


            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }

            try
            {
                // Get the current user's ID
                var userId = user?.Id;

                // Re-validate outstanding unpaid fees for QC6 Form 
                if (viewModel.OutstandingUnpaidFees == false)
                {
                    viewModel.OutstandingUnpaidFeesComment = null;
                }

                // Re-validate budgeted fee recovery rate for QC6 Form 
                if (viewModel.BudgetedFeeRecoveryRate >= 30)
                {
                    viewModel.BudgetedFeeRecoveryRateComment = null;
                }

                // Re-validate predecessor auditor for QC6 Form 
                if (viewModel.PredecessorAuditor == false)
                {
                    viewModel.ReasonsForDiscontinuance = null;
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
                    Industry = viewModel.Industry,
                    Status = "Pending",
                    FormSubmissionDate = DateTime.Now,
                    PKFEntityProposingService = viewModel.PKFEntityProposingService,
                    SourceOfReferral = viewModel.SourceOfReferral,
                    NatureOfServiceForEstimateFee = viewModel.NatureOfServiceForEstimateFee,
                    EstimatedFee = viewModel.EstimatedFee.Value,
                    BudgetedTimeCost = viewModel.BudgetedTimeCost.Value,
                    BudgetedFeeRecoveryRate = viewModel.BudgetedFeeRecoveryRate.Value,
                    BudgetedFeeRecoveryRateComment = viewModel.BudgetedFeeRecoveryRateComment,
                    OutstandingUnpaidFees = viewModel.OutstandingUnpaidFees,
                    OutstandingUnpaidFeesComment = viewModel.OutstandingUnpaidFeesComment,
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
                            _context.SaveChanges();
                        }
                    }
                }

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
                    _context.SaveChanges();
                }

                // Check if null
                if (viewModel.OtherDocuments != null)
                {
                    // Generate a unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

                    // Get the path to wwwroot
                    var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "QC6Form-OtherDocuments");

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
                        QC6FormID = qc6formId,
                        FileName = uniqueFileName,
                        DocumentType = "OtherDocuments"
                    });

                    _context.SaveChanges();
                }

                // Check if null, else upload the files
                if (viewModel.AdditionalDocuments != null && viewModel.AdditionalDocuments.Count > 0)
                {
                    foreach (var document in viewModel.AdditionalDocuments)
                    {
                        if (document.File != null && document.File.Length > 0)
                        {
                            // Generate a unique filename
                            var uniqueFileName = qc6formId + "-" + document.DocumentName + "-" + Guid.NewGuid().ToString() + ".pdf";

                            // Get the path to wwwroot
                            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "QC6Form-AdditionalDocuments");

                            // Ensure the uploads folder exists
                            Directory.CreateDirectory(uploadsFolder);

                            // Get file path
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // Save the file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await document.File.CopyToAsync(stream);
                            }

                            // Add to the db context
                            _context.Add(new QCDocument
                            {
                                QC6FormID = qc6formId,
                                FileName = uniqueFileName,
                                DocumentType = document.DocumentName
                            });

                            _context.SaveChanges();
                        }
                    }
                }

				// Send email to first approver to notify on creation
				await _emailSender.SendEmailAsync(viewModel.EPHODApprovedBy, "QC6 Form Approval Required",
					$"<p>Dear {viewModel.EPHODApprovedBy},</p>" +
					$"<p>A new QC6 Form has been created with File Reference: <strong>{fileReference}</strong>.</p>" +
					$"<p>You have been designated as the first approver. Please log in to the Audit Management System to approve or reject the QC6 Form.</p>" +
					$"<p>Thank you for your attention!</p>" +
					$"<p>Best regards,<br/>" +
					$"PKF Team</p>"
				);

                // Set the success message for the toast notification
                TempData["QC6CreateMessage"] = "QC6 Form created successfully.";

                if (roles.Contains("Admin") || roles.Contains("Reviewer"))
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
                // Log the error
                ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Non-Auditor,User,Admin,Reviewer")]
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
                var tnaTNESectionB = _context.TNATNESectionBs.SingleOrDefault(t => t.TNATNEAssessmentID == tnaTNEAssessment.TNATNEAssessmentID);

                // Identify TNATNESectionD for TNATNEAssessment
                var tnaTNESectionD = _context.TNATNESectionDs.SingleOrDefault(t => t.TNATNEAssessmentID == tnaTNEAssessment.TNATNEAssessmentID);

                if (tnaTNESectionB != null)
                {
                    // Delete TNATNESectionB
                    _context.TNATNESectionBs.Remove(tnaTNESectionB);
                }

                if (tnaTNESectionD != null)
                {
                    // Delete TNATNESectionD
                    _context.TNATNESectionDs.Remove(tnaTNESectionD);
                }

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

                // Delete from Documents Section if present
                var documents = _context.QCDocuments.Where(c => c.QC6FormID == id).ToList();

                if (documents.Count > 0)
                {
                    foreach (var document in documents)
                    {
                        // Construct the file path
                        var uploadsFolder = document.DocumentType == "OtherDocuments"
                            ? Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "QC6Form-OtherDocuments")
                            : Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "QC6Form-AdditionalDocuments");

                        var filePath = Path.Combine(uploadsFolder, document.FileName);

                        // Delete the file if it exists
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Remove all documents from the database
                    _context.QCDocuments.RemoveRange(documents);
                    _context.SaveChanges();
                }

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

        public async Task<IActionResult> GenerateDefaultTemplateAsync()
        {
            // Initialise view model
            var viewModel = new QC6FormCreationViewModel();

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Populate Sub Forms
            viewModel = RetrieveSubFormData(viewModel);

            // Append UserEmail to viewModel
            viewModel.UserEmail = user.Email;

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Append combined emails to viewModel
            viewModel.AdminEmails = combinedEmails;

            // Retrieve QC6Form data from the database
            try
            {

                // Get the selected QC6 Form that is a template
                var qc6formData = _context.QC6Forms.FirstOrDefault(e => e.IsTemplate.Equals(true));

                if (qc6formData == null)
                {
                    viewModel.ErrorMessage = "No templates found!";
                    return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
                }

                // Get the selected QC6 Form ID 

                var id = qc6formData.QC6FormID;

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
                                testDescription.Comment = test.Comments;
                            }
                        }
                    }
                }

                // Append new data for QC6Form General Portion
                viewModel.QC6FormID = qc6formData.QC6FormID.ToString();
                viewModel.Status = qc6formData.Status;
                viewModel.FileReference = qc6formData.FileReference;
                viewModel.ProspectiveClient = qc6formData.ProspectiveClient;
                viewModel.PeriodEnded = qc6formData.PeriodEnded;
                viewModel.EngagementType = qc6formData.EngagementType;
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

                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }
            catch
            {
                viewModel.ErrorMessage = "There was an error while handling your request, please try again!";
                return View("~/Views/General/QC6/QC6FormCreation.cshtml", viewModel);
            }
        }

        public QC6FormCreationViewModel RetrieveSubFormData(QC6FormCreationViewModel viewModel)
        {
            // Retrieve QC6Form data from the database
            var qc6SubForms = _context.QC6SubForms.ToList();
            var qc6FormObjectives = _context.QC6FormObjectives.ToList();
            var qc6FormTestDescriptions = _context.QC6FormTestDescriptions.ToList();

            // Populate SubForms
            viewModel.SubForms = qc6SubForms.Select(subForm => new ViewModels.SubFormViewModel
            {
                QC6SubFormID = subForm.QC6SubFormID,
                SubFormType = subForm.SubFormType,
                Objectives = qc6FormObjectives
                    .Where(obj => obj.QC6SubFormID == subForm.QC6SubFormID)
                    .Select(obj => new ViewModels.ObjectiveViewModel
                    {
                        QC6FormObjectiveID = obj.QC6FormObjectiveID,
                        Objective = obj.Objective,
                        TestDescriptions = qc6FormTestDescriptions
                            .Where(desc => desc.QC6FormObjectiveID == obj.QC6FormObjectiveID)
                            .Select(desc => new ViewModels.TestDescriptionViewModel
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

        private async Task SaveFileAsync(IFormFile file, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        public IActionResult RetrieveNASFeeDetails(string clientName)
        {
            // Retrieve QC6 Forms where FileReference contains "NAS"
            var qc6Forms = _context.QC6Forms
                .Where(e => e.FileReference.Contains("NAS") && e.ProspectiveClient == clientName)
                .Select(f => f.QC6FormID)
                .ToList();

            // No clients found
            if (qc6Forms.Count == 0)
            {
                return Json(new { success = false, message = $"No client with name \"{clientName}\" found" });
            }

            // Join QC6Forms with QC6FormFeeDetails
            var feeDetails = _context.QC6FormFeeDetails
                .Where(fd => qc6Forms.Contains(fd.QC6FormID))
                .Join(
                    _context.QC6Forms,
                    fd => fd.QC6FormID,
                    f => f.QC6FormID,
                    (fd, f) => new
                    {
                        fd.QC6FormFeeDetailID,
                        fd.QC6FormID,
                        fd.Fee,
                        // Use OtherService if it's not null; otherwise, use NatureOfService
                        NatureOfService = fd.OtherService != null ? $"{fd.NatureOfService} ({fd.OtherService})" : fd.NatureOfService,
                        f.FileReference,
                        f.ProspectiveClient,
                        f.AuditFee,
                        f.GrandTotal,
                        f.FeeConcentration,
                        f.PeriodEnded,
                        f.Status
                    }
                )
                .ToList();


            // Return the combined data as JSON
            return Json(new { success = true, data = feeDetails });
        }

        public string ExtractDocumentName(string s3Key)
        {
            // Example s3Key: QC6FormUploads/OtherDocuments/DocumentName/c73a5d21-c0bc-4c6b-aaf5-0b20b58b16fd.pdf

            // Split by '/' to get the parts
            var parts = s3Key.Split('/');

            // Check if the array length is sufficient and get the document name
            if (parts.Length >= 4)
            {
                // The document name is the part before the last part (which is the filename)
                return parts[parts.Length - 2];
            }

            // Return an empty string or handle the case where the format is unexpected
            return string.Empty;
        }

        public async Task<IActionResult> GetAllClients()
        {
            var clientNames = await _context.QC6Forms
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .ToListAsync(); // Fetch all unique client names

            // Return the list of client names
            return Ok(clientNames);
        }

    }
}
