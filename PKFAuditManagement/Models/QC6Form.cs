using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC6Form
    {
        public int QC6FormID { get; set; }
        public string CreatedBy { get; set; }
        public string? FileReference { get; set; }
        public string? ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string? EngagementType { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public string? PKFEntityProposingService { get; set; }
        public string? SourceOfReferral { get; set; }
        public decimal? NatureOfServiceForEstimateFee { get; set; }
        public decimal? EstimatedFee { get; set; }
        public decimal? BudgetedTimeCost { get; set; }
        public decimal? BudgetedFeeRecoveryRate { get; set; }
        public decimal? FeeFromServices { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public string? TypeOfActivities { get; set; }
        public string? ComplexityOfEngagement { get; set; }
        public string? PredecessorAuditor { get; set; }
        public string? ReasonsForDiscontinuance { get; set; }
        public bool PublicInterestEntity { get; set; }
        public string? TypeOfPIE { get; set; }
        public bool TransnationalEntity { get; set; }
        public bool TransnationalAudit { get; set; }
        public ICollection<QC6FormTest> QC6FormTests { get; set; } // One-to-Many
        public QC6FormConclusion QC6FormConclusion { get; set; } // One-to-One

    }
}
