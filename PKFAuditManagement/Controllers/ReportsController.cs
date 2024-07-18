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
            var qc6Forms = _context.QC6Forms.ToList();
            var qc7Forms = _context.QC7Forms.ToList();
            var qc35Forms = _context.QC35Forms.ToList();
            var signedFSForms = _context.SignedFSForm.ToList();
            // Step 2: Map the data to the ViewModel
            var reportDataQC6 = qc6Forms.Select(qc6Form => new ReportsViewModel
            {
                FormType = "QC6",
                QC6FormID = qc6Form.QC6FormID,
                FileReference = qc6Form.FileReference,
                ProspectiveClient = qc6Form.ProspectiveClient,
                PeriodEnded = qc6Form.PeriodEnded.Value,
                EngagementType = qc6Form.EngagementType,
                PreparedBy = qc6Form.PreparedBy,
                PreparedByDate = qc6Form.PreparedByDate,
                ReviewedBy = qc6Form.ReviewedBy,
                ReviewedByDate = qc6Form.ReviewedByDate,
                Status = qc6Form.Status,
                RejectionReason = qc6Form.RejectionReason,
                FormSubmissionDate = qc6Form.FormSubmissionDate,
                PKFEntityProposingService = qc6Form.PKFEntityProposingService,
                SourceOfReferral = qc6Form.SourceOfReferral,
                NatureOfServiceForEstimateFee = qc6Form.NatureOfServiceForEstimateFee,
                EstimatedFee = qc6Form.EstimatedFee,
                BudgetedTimeCost = qc6Form.BudgetedTimeCost,
                BudgetedFeeRecoveryRate = qc6Form.BudgetedFeeRecoveryRate,
                OutstandingUnpaidFees = qc6Form.OutstandingUnpaidFees,
                AuditFee = qc6Form.AuditFee,
                GrandTotal = qc6Form.GrandTotal,
                FeeConcentration = qc6Form.FeeConcentration,
                ConflictsCheckDone = qc6Form.ConflictsCheckDone,
                TypeOfActivities = qc6Form.TypeOfActivities,
                ComplexityOfEngagement = qc6Form.ComplexityOfEngagement,
                PredecessorAuditor = qc6Form.PredecessorAuditor,
                ReasonsForDiscontinuance = qc6Form.ReasonsForDiscontinuance,
                IsPublicInterestEntity = qc6Form.PublicInterestEntity,
                PublicInterestEntityType = qc6Form.PublicInterestEntityType,
                // Add other fields as needed
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
