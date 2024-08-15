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
            // Step 1: Retrieve the QC6 forms from the database
            var qc6Forms = _context.QC6Forms.Where(e => !e.IsTemplate).ToList();
            var qc7Forms = _context.QC7Forms.ToList();
            var qc35Forms = _context.QC35Forms.ToList();
            var signedFSForms = _context.SignedFSForm.ToList();
            // Step 2: Map the data to the ViewModel
            var reportDataQC6 = qc6Forms.Select(qc6Form => new ReportsViewModel
            {
                FormType = "QC6",
                QC6FormID = qc6Form.QC6FormID,
                FileReference = qc6Form.FileReference ?? string.Empty,  // Provide default value if null
                ProspectiveClient = qc6Form.ProspectiveClient ?? string.Empty,  // Provide default value if null
                PeriodEnded = qc6Form.PeriodEnded ?? DateTime.MinValue,  // Provide default value if null
                EngagementType = qc6Form.EngagementType ?? string.Empty,  // Provide default value if null
                PreparedBy = qc6Form.PreparedBy ?? string.Empty,  // Provide default value if null
                PreparedByDate = qc6Form.PreparedByDate,  // Non-nullable, use directly
                ReviewedBy = qc6Form.ReviewedBy ?? string.Empty,  // Provide default value if null
                ReviewedByDate = qc6Form.ReviewedByDate ?? DateTime.MinValue,  // Provide default value if null
                Status = qc6Form.Status ?? string.Empty,  // Provide default value if null
                RejectionReason = qc6Form.RejectionReason ?? string.Empty,  // Provide default value if null
                FormSubmissionDate = qc6Form.FormSubmissionDate,  // Non-nullable, use directly
                PKFEntityProposingService = qc6Form.PKFEntityProposingService ?? string.Empty,  // Provide default value if null
                SourceOfReferral = qc6Form.SourceOfReferral ?? string.Empty,  // Provide default value if null
                NatureOfServiceForEstimateFee = qc6Form.NatureOfServiceForEstimateFee ?? string.Empty,  // Provide default value if null
                EstimatedFee = qc6Form.EstimatedFee,  // Provide default value if null
                BudgetedTimeCost = qc6Form.BudgetedTimeCost,  // Provide default value if null
                BudgetedFeeRecoveryRate = qc6Form.BudgetedFeeRecoveryRate,  // Provide default value if null
                OutstandingUnpaidFees = qc6Form.OutstandingUnpaidFees,  // Non-nullable, use directly
                AuditFee = qc6Form.AuditFee,  // Provide default value if null
                GrandTotal = qc6Form.GrandTotal,  // Provide default value if null
                FeeConcentration = qc6Form.FeeConcentration,  // Provide default value if null
                ConflictsCheckDone = qc6Form.ConflictsCheckDone,  // Non-nullable, use directly
                TypeOfActivities = qc6Form.TypeOfActivities ?? string.Empty,  // Provide default value if null
                ComplexityOfEngagement = qc6Form.ComplexityOfEngagement ?? string.Empty,  // Provide default value if null
                PredecessorAuditor = qc6Form.PredecessorAuditor ?? string.Empty,  // Provide default value if null
                ReasonsForDiscontinuance = qc6Form.ReasonsForDiscontinuance ?? string.Empty,  // Provide default value if null
                IsPublicInterestEntity = qc6Form.PublicInterestEntity,  // Non-nullable, use directly
                PublicInterestEntityType = qc6Form.PublicInterestEntityType ?? string.Empty  // Provide default value if null
            }).ToList();

            var reportDataQC7 = qc7Forms.Select(qc7Form => new ReportsViewModel
            {
                FormType = "QC7",
                QC7FormID = qc7Form.QC7FormID,
                FileReference = qc7Form.FileReference,
                ProspectiveClient = qc7Form.Client,
                PeriodEnded = qc7Form.PeriodEnded,
                EngagementType = qc7Form.EngagementType,
                PreparedBy = qc7Form.PreparedBy,
                PreparedByDate = qc7Form.PreparedByDate,
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
                GrandTotal = qc7Form.TimeCosts, // Example calculation
                FeeConcentration = qc7Form.FeeConcentration,
                ConflictsCheckDone = qc7Form.AnySuspiciousTransactionReportFiled,
                TypeOfActivities = qc7Form.TypeOfClientActivities,
                ComplexityOfEngagement = qc7Form.RiskRatingPriorYear,
                PredecessorAuditor = qc7Form.SafeguardReviewerName,
                ReasonsForDiscontinuance = qc7Form.SuspiciousTransactionReportFiledComment,
                IsPublicInterestEntity = qc7Form.IsPublicInterestEntity,
                PublicInterestEntityType = qc7Form.PublicInterestEntityType,
                // Add other fields as needed
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
                SignedFSFormID = form.Id,
                AuditedReportDate = form.AuditedReportDate,
                PartnerEmail = form.PartnerEmail,
                UserEmail = form.UserEmail,
                FilePath = form.FilePath,
                ScheduleDate = form.ScheduleDate,
                EmailType = form.EmailType,
                EmailBody = form.EmailBody,
                IsProcessed = form.IsProcessed
            }).ToList();

            // Concatenate all report data
            var reportData = reportDataQC6
                .Concat(reportDataQC7)
                .Concat(reportDataQC35)
                .Concat(reportDataSignedFS)
                .ToList();

            // Step 3: Pass the report data to the view
            return View("~/Views/General/Reports/GenerateReports.cshtml", reportData);
        }
    }
}
