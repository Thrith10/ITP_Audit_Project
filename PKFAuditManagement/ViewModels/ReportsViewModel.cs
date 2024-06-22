using System;
using System.Collections.Generic;

namespace PKFAuditManagement.ViewModels
{
    public class ReportsViewModel
    {
        // QC6 Form properties
        public int QC6FormID { get; set; }
        public string FileReference { get; set; }
        public string ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string EngagementType { get; set; }
        public string PreparedBy { get; set; }
        public DateTime PreparedByDate { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public string PKFEntityProposingService { get; set; }
        public string SourceOfReferral { get; set; }
        public string NatureOfServiceForEstimateFee { get; set; }
        public decimal? EstimatedFee { get; set; }
        public decimal? BudgetedTimeCost { get; set; }
        public decimal? BudgetedFeeRecoveryRate { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        public decimal? AuditFee { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public string TypeOfActivities { get; set; }
        public string ComplexityOfEngagement { get; set; }
        public string PredecessorAuditor { get; set; }
        public string ReasonsForDiscontinuance { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string PublicInterestEntityType { get; set; }

        // Add other properties if needed
    }
}
