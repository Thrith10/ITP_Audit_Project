using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC6Form
    {
        public int QC6FormID { get; set; }
        public required string CreatedBy { get; set; }
        public required string FileReference { get; set; }
        public required string ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public required string EngagementType { get; set; }
        public required string PreparedBy { get; set; }
        public DateTime PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public required string Status { get; set; }
        public bool IsTemplate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public required string PKFEntityProposingService { get; set; }
        public required string SourceOfReferral { get; set; }
        public required string NatureOfServiceForEstimateFee { get; set; }
        [Precision(18, 2)]
        public required decimal EstimatedFee { get; set; }
        [Precision(18, 2)]
        public required decimal BudgetedTimeCost { get; set; }
        [Precision(18, 2)]
        public required decimal BudgetedFeeRecoveryRate { get; set; }
        public string? BudgetedFeeRecoveryRateComment { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        public string? OutstandingUnpaidFeesComment { get; set; }
        [Precision(18, 2)]
        public required decimal AuditFee { get; set; }
        [Precision(18, 2)]
        public required decimal GrandTotal { get; set; }
        [Precision(18, 2)]
        public required decimal FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public required string TypeOfActivities { get; set; }
        public required string ComplexityOfEngagement { get; set; }
        public bool PredecessorAuditor { get; set; }
        public string? ReasonsForDiscontinuance { get; set; }
        public bool PublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public bool IsSubForm2NotApplicable { get; set; }
        public bool IsSubForm3NotApplicable { get; set; }
        public ICollection<QC6FormTest> QC6FormTests { get; set; } // One-to-Many
        public QC6FormConclusion QC6FormConclusion { get; set; } // One-to-One
        public TNATNEAssessment TNATNEAssessment { get; set; } // One-to-One

    }
}
