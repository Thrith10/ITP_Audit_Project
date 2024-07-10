using System;
using System.Collections.Generic;

namespace PKFAuditManagement.ViewModels
{
    public class ReportsViewModel
    {
        public string FormType { get; set; } // "QC6" or "QC7"

        // QC6 properties
        public int QC6FormID { get; set; }
        public string FileReference { get; set; }
        public string ProspectiveClient { get; set; }
        public DateTime PeriodEnded { get; set; }
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
        public decimal EstimatedFee { get; set; }
        public decimal BudgetedTimeCost { get; set; }
        public decimal BudgetedFeeRecoveryRate { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        public decimal AuditFee { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public string TypeOfActivities { get; set; }
        public string ComplexityOfEngagement { get; set; }
        public string PredecessorAuditor { get; set; }
        public string ReasonsForDiscontinuance { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string PublicInterestEntityType { get; set; }

        // QC7 properties
        public int QC7FormID { get; set; }
        public string Client { get; set; }
        public DateTime PeriodEndedQC7 { get; set; }
        public string EngagementTypeQC7 { get; set; }
        public string PreparedByQC7 { get; set; }
        public DateTime PreparedByDateQC7 { get; set; }
        public string ReviewedByQC7 { get; set; }
        public DateTime ReviewedByDateQC7 { get; set; }
        public string StatusQC7 { get; set; }
        public string RejectionReasonQC7 { get; set; }
        public DateTime FormSubmissionDateQC7 { get; set; }
        public decimal PriorYearFee { get; set; }
        public decimal TimeCosts { get; set; }
        public decimal PriorYearRecoveryRate { get; set; }
        public bool AnyOutstandingUnpaidAuditFees { get; set; }
        public string TypeOfClientActivities { get; set; }
        public string RiskRatingPriorYear { get; set; }
        public bool AnySuspiciousTransactionReportFiled { get; set; }
        public string SuspiciousTransactionReportFiledComment { get; set; }
        public string SafeguardReviewerName { get; set; }
        public decimal ProposedFeeCurrentYear { get; set; }
        public decimal BudgetedTimeCostQC7 { get; set; }
        public decimal ProposedRecoveryRateCurrentYear { get; set; }
        public bool IsPublicInterestEntityQC7 { get; set; }
        public string PublicInterestEntityTypeQC7 { get; set; }
        public bool IsSubForm2NotApplicable { get; set; }
        public bool IsSubForm3NotApplicable { get; set; }
        // Add other properties if needed
    }
}
