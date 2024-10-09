using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.ViewModels;
using PKFAuditManagement.Models;
using System.Linq;
using PKFAuditManagement.Data;
using System.Collections.Generic;

namespace PKFAuditManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GenerateReports()
        {
            // Step 1: Retrieve the QC6 forms and their corresponding conclusions from the database
            var qc6Forms = _context.QC6Forms.Where(e => !e.IsTemplate).ToList();
            var qc6FormConclusions = _context.QC6FormConclusions.ToList();  // Retrieve all QC6FormConclusions

            // Retrieve QC7Forms and their corresponding conclusions from the database
            var qc7Forms = _context.QC7Forms.ToList();
            var qc7FormConclusions = _context.QC7FormConclusions.ToList();  // Retrieve all QC7FormConclusions

            var qc35Forms = _context.QC35Forms.ToList();
            var signedFSForms = _context.SignedFSForm.ToList();

            // Step 2: Map the data to the ViewModel, linking QC6FormConclusion with QC6Forms based on QC6FormID
            var reportDataQC6 = qc6Forms.Select(qc6Form =>
            {
                var conclusion = qc6FormConclusions
                    .FirstOrDefault(c => c.QC6FormID == qc6Form.QC6FormID);  // Find the related QC6FormConclusion by QC6FormID

                return new ReportsViewModel
                {
                    FormType = "QC6",
                    QC6FormID = qc6Form.QC6FormID,
                    FileReference = qc6Form.FileReference ?? string.Empty,
                    ProspectiveClient = qc6Form.ProspectiveClient ?? string.Empty,
                    PeriodEnded = qc6Form.PeriodEnded ?? DateTime.MinValue,
                    EngagementType = qc6Form.EngagementType ?? string.Empty,
                    PreparedBy = conclusion?.PreparedBy ?? qc6Form.PreparedBy ?? string.Empty,  // Take from QC6FormConclusion or fallback to QC6Form
                    PreparedByDate = conclusion?.PreparedByDate ?? qc6Form.PreparedByDate,  // Take from QC6FormConclusion or fallback to QC6Form
                    ReviewedBy = qc6Form.ReviewedBy ?? string.Empty,
                    ReviewedByDate = qc6Form.ReviewedByDate ?? DateTime.MinValue,
                    Status = qc6Form.Status ?? string.Empty,
                    RejectionReason = qc6Form.RejectionReason ?? string.Empty,
                    FormSubmissionDate = qc6Form.FormSubmissionDate,
                    PKFEntityProposingService = qc6Form.PKFEntityProposingService ?? string.Empty,
                    SourceOfReferral = qc6Form.SourceOfReferral ?? string.Empty,
                    NatureOfServiceForEstimateFee = qc6Form.NatureOfServiceForEstimateFee ?? string.Empty,
                    EstimatedFee = qc6Form.EstimatedFee,
                    BudgetedTimeCost = qc6Form.BudgetedTimeCost,
                    BudgetedFeeRecoveryRate = qc6Form.BudgetedFeeRecoveryRate,
                    OutstandingUnpaidFees = qc6Form.OutstandingUnpaidFees,
                    AuditFee = qc6Form.AuditFee,
                    GrandTotal = qc6Form.GrandTotal,
                    FeeConcentration = qc6Form.FeeConcentration,
                    ConflictsCheckDone = qc6Form.ConflictsCheckDone,
                    TypeOfActivities = qc6Form.TypeOfActivities ?? string.Empty,
                    ComplexityOfEngagement = qc6Form.ComplexityOfEngagement ?? string.Empty,
                    PredecessorAuditor = qc6Form.PredecessorAuditor ?? string.Empty,
                    ReasonsForDiscontinuance = qc6Form.ReasonsForDiscontinuance ?? string.Empty,
                    IsPublicInterestEntity = qc6Form.PublicInterestEntity,
                    PublicInterestEntityType = qc6Form.PublicInterestEntityType ?? string.Empty,

                    // New fields from QC6FormConclusion
                    MPHODQMPApprovedByDate = conclusion?.MPHODQMPApprovedByDate
                };
            }).ToList();

            // Map the QC7 data to the ViewModel, linking QC7FormConclusion with QC7Forms based on QC7FormID
            var reportDataQC7 = qc7Forms.Select(qc7Form =>
            {
                var conclusion = qc7FormConclusions
                    .FirstOrDefault(c => c.QC7FormID == qc7Form.QC7FormID);  // Find the related QC7FormConclusion by QC7FormID

                return new ReportsViewModel
                {
                    FormType = "QC7",
                    QC7FormID = qc7Form.QC7FormID,
                    FileReference = qc7Form.FileReference,
                    ProspectiveClient = qc7Form.Client,
                    PeriodEnded = qc7Form.PeriodEnded,
                    EngagementType = qc7Form.EngagementType,
                    PreparedBy = conclusion?.EMPreparedBy ?? qc7Form.PreparedBy,  // Take from QC7FormConclusion or fallback to QC7Form
                    PreparedByDate = conclusion?.EMPreparedByDate ?? qc7Form.PreparedByDate,  // Take from QC7FormConclusion or fallback
                    MPHODQMPApprovedByDate = conclusion?.MPHODQMPApprovedByDate,  // MPHODQMPApprovedByDate from QC7FormConclusion
                    ReviewedBy = qc7Form.ReviewedBy,
                    ReviewedByDate = qc7Form.ReviewedByDate,
                    Status = qc7Form.Status,
                    RejectionReason = qc7Form.RejectionReason,
                    FormSubmissionDate = qc7Form.FormSubmissionDate,
                    PKFEntityProposingService = qc7Form.EngagementType,
                    SourceOfReferral = qc7Form.Client,
                    NatureOfServiceForEstimateFee = qc7Form.TypeOfClientActivities,
                    EstimatedFee = qc7Form.ProposedFeeCurrentYear,
                    BudgetedTimeCost = qc7Form.BudgetedTimeCost,
                    BudgetedFeeRecoveryRate = qc7Form.ProposedRecoveryRateCurrentYear,
                    OutstandingUnpaidFees = qc7Form.AnyOutstandingUnpaidAuditFees,
                    AuditFee = qc7Form.PriorYearFee,
                    GrandTotal = qc7Form.TimeCosts,
                    FeeConcentration = qc7Form.FeeConcentration,
                    ConflictsCheckDone = qc7Form.AnySuspiciousTransactionReportFiled,
                    TypeOfActivities = qc7Form.TypeOfClientActivities,
                    ComplexityOfEngagement = qc7Form.RiskRatingPriorYear,
                    PredecessorAuditor = qc7Form.SafeguardReviewerName,
                    ReasonsForDiscontinuance = qc7Form.SuspiciousTransactionReportFiledComment,
                    IsPublicInterestEntity = qc7Form.IsPublicInterestEntity,
                    PublicInterestEntityType = qc7Form.PublicInterestEntityType
                };
            }).ToList();


            var reportDataQC35 = qc35Forms.Select(form => new ReportsViewModel
            {
                FormType = "QC35",
                QC35FormID = form.QC35FormID,
                CreatedBy = form.CreatedBy,
                ClientName = form.ClientName,
                ReportingYearEnd = form.ReportingYearEnd,
                PartnerName = form.PartnerName,
                ManagerName = form.ManagerName,
                Status = form.Status
            }).ToList();

            // Map SignedFSForm data to ViewModel
            var reportDataSignedFS = signedFSForms.Select(form => new ReportsViewModel
            {
                FormType = "SignedFS",
                Client = form.Client,
                SignedFSFormID = form.Id,
                AuditedReportDate = form.AuditedReportDate,
                PartnerEmail = form.PartnerEmail,
                UserEmail = form.UserEmail,
                FilePath = form.FilePath
            }).ToList();

            // Client Status data (from DisplayStatus)
            var clientStatusData = qc6Forms.Select(qc6 =>
            {
                var qc7Form = _context.QC7Forms.FirstOrDefault(q => q.Client == qc6.ProspectiveClient);
                var qc7FormConclusion = qc7Form != null ? _context.QC7FormConclusions.FirstOrDefault(c => c.QC7FormID == qc7Form.QC7FormID) : null;

                return new ReportsViewModel
                {
                    FormType = "ClientStatus",
                    CSClientName = qc6.ProspectiveClient ?? "Unknown",
                    ClientStatusFinancialYearEnd = qc6.PeriodEnded,  // Client Status Financial Year End
                    QC6FirstApprover = _context.QC6FormConclusions.FirstOrDefault(c => c.QC6FormID == qc6.QC6FormID)?.PreparedBy ?? "Not Assigned",
                    QC6SecondApprover = _context.QC6FormConclusions.FirstOrDefault(c => c.QC6FormID == qc6.QC6FormID)?.MPHODQMPApprovedBy ?? "Not Assigned",
                    QC7FirstApprover = qc7FormConclusion?.EMPreparedBy ?? "Not Assigned",
                    QC7SecondApprover = qc7FormConclusion?.MPHODQMPApprovedBy ?? "Not Assigned",
                    QC6Status = qc6.Status ?? "Null",
                    QC7Status = qc7Form?.Status ?? "Null",
                    QC35Status = _context.QC35Forms.FirstOrDefault(q => q.ClientName == qc6.ProspectiveClient)?.Status ?? "Null",
                    SignedFSStatus = _context.SignedFSForm.FirstOrDefault(q => q.Client == qc6.ProspectiveClient)?.IsProcessed == true ? "Processed" : "Not Uploaded"
                };
            }).ToList();

            // Concatenate all report data
            var reportData = reportDataQC6
                .Concat(reportDataQC7)
                .Concat(reportDataQC35)
                .Concat(reportDataSignedFS)
                .Concat(clientStatusData)  // Add Client Status data
                .ToList();

            // Step 3: Pass the report data to the view
            return View("~/Views/General/Reports/GenerateReports.cshtml", reportData);
        }
    }
}
