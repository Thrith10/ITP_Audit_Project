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

            // Step 2: Map the data to the ViewModel
            var reportData = qc6Forms.Select(qc6Form => new ReportsViewModel
            {
                QC6FormID = qc6Form.QC6FormID,
                FileReference = qc6Form.FileReference,
                ProspectiveClient = qc6Form.ProspectiveClient,
                PeriodEnded = qc6Form.PeriodEnded,
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

            // Step 3: Pass the report data to the view
            return View("~/Views/General/Reports/GenerateReports.cshtml", reportData);
        }
    }
}
