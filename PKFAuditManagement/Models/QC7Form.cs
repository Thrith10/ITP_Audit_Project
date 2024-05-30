using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC7Form
    {
        public int QC7FormID { get; set; }
        public string CreatedBy { get; set; }
        public string? FileReference { get; set; }
        public string? Client { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string? EngagementType { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public decimal? PriorYearFee { get; set; }
        public decimal? TimeCosts { get; set; }
        public decimal? PriorYearRecoveryRate { get; set; }
        public bool AnyOutstandingUnpaidAuditFees { get; set; }
        public string? TypeOfClientActivities { get; set; }
        public string? RiskRatingPriorYear { get; set; }
        public bool AnySuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledComment { get; set; }
        public string? SafeguardReviewerName { get; set; }
        public bool AnyOutstandingUnpaidNonAuditFees { get; set; }
        public decimal? FeeConcentration { get; set; }
        public decimal? ProposedFeeCurrentYear { get; set; }
        public decimal? BudgetedTimeCost { get; set; }
        public decimal? ProposedRecoveryRateCurrentYear { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public bool TransnationalEntity { get; set; }
        public bool TransnationalAudit { get; set; }
        public string? TransnationalAuditComment { get; set; }
        public ICollection<QC7FormTest> QC7FormTests { get; set; } // One-to-Many
        public QC7FormConclusion QC7FormConclusion { get; set; } // One-to-One

    }
}
