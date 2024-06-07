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
        public required string PreparedBy { get; set; }
        public DateTime PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime ReviewedByDate { get; set; }
        public required string Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public required decimal PriorYearFee { get; set; }
        public required decimal TimeCosts { get; set; }
        public required decimal PriorYearRecoveryRate { get; set; }
        public bool AnyOutstandingUnpaidAuditFees { get; set; }
        public required string TypeOfClientActivities { get; set; }
        public required string RiskRatingPriorYear { get; set; }
        public bool AnySuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledComment { get; set; }
        public required string SafeguardReviewerName { get; set; }
        public bool AnyOutstandingUnpaidNonAuditFees { get; set; }
        public decimal? FeeConcentration { get; set; }
        public required decimal ProposedFeeCurrentYear { get; set; }
        public required decimal BudgetedTimeCost { get; set; }
        public required decimal ProposedRecoveryRateCurrentYear { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public ICollection<QC7FormTest> QC7FormTests { get; set; } // One-to-Many
        public QC7FormConclusion QC7FormConclusion { get; set; } // One-to-One

    }
}
