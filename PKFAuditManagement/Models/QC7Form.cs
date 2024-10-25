using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC7Form
    {
        public int QC7FormID { get; set; }
        public required string CreatedBy { get; set; }
        public required string FileReference { get; set; }
        public required string Client { get; set; }
        public DateTime PeriodEnded { get; set; }
        public required string EngagementType { get; set; }
        public required string Industry { get; set; }
        public required string Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        [Precision(18, 2)]
        public required decimal PriorYearFee { get; set; }
        [Precision(18, 2)]
        public required decimal TimeCosts { get; set; }
        [Precision(18, 2)]
        public required decimal PriorYearRecoveryRate { get; set; }
        public string? PriorYearRecoveryRateComment { get; set; }
        public bool AnyOutstandingUnpaidAuditFees { get; set; }
        public string? AnyOutstandingUnpaidAuditFeesComment { get; set; }
        public required string TypeOfClientActivities { get; set; }
        public required string RiskRatingPriorYear { get; set; }
        public bool AnySuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledComment { get; set; }
        public string? SafeguardReviewerName { get; set; }
        public bool AnyOutstandingUnpaidNonAuditFees { get; set; }
        public string? AnyOutstandingUnpaidNonAuditFeesComment { get; set; }
        [Precision(18, 2)]
        public decimal AuditFee { get; set; }
        [Precision(18, 2)]
        public decimal GrandTotal { get; set; }
        [Precision(18, 2)]
        public decimal FeeConcentration { get; set; }
        [Precision(18, 2)]
        public required decimal ProposedFeeCurrentYear { get; set; }
        [Precision(18, 2)]
        public required decimal BudgetedTimeCost { get; set; }
        [Precision(18, 2)]
        public required decimal ProposedRecoveryRateCurrentYear { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public bool IsSubForm2NotApplicable { get; set; }
        public bool IsSubForm3NotApplicable { get; set; }
        public ICollection<QC7FormFeeDetail> QC7FormFeeDetails { get; set; } // One-to-Many
        public ICollection<QC7FormTest> QC7FormTests { get; set; } // One-to-Many
        public QC7FormConclusion QC7FormConclusion { get; set; } // One-to-One

    }
}
