using PKFAuditManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class QC6FormViewModel
    {
        // Start of QC6Form data
        public string? ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string? EngagementType { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public string? PKFEntityProposingService { get; set; }
        public string? SourceOfReferral { get; set; }
        public string? NatureOfServiceForEstimateFee { get; set; }
        public decimal? EstimatedFee { get; set; }
        public decimal? BudgetedTimeCost { get; set; }
        public decimal? BudgetedFeeRecoveryRate { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        public decimal? AuditFee { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public string? TypeOfActivities { get; set; }
        public string? ComplexityOfEngagement { get; set; }
        public string? PredecessorAuditor { get; set; }
        public string? ReasonsForDiscontinuance { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public string? ErrorMessage { get; set; }

        // End of QC6Form data

        // Start of QC6FormConclusion date
        public bool AnySignificantRisk { get; set; }
        public string? SignificantRiskComment { get; set; }
        public string? NewEngagementRiskRating { get; set; }
        public string? NewEngagementRiskRatingReason { get; set; }
        public string? EngagementSubjectedTo { get; set; }
        public string? SafeguardReviewerAssigned { get; set; }
        public string? IsNewEngagementAcceptance { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        public string? Satisfaction { get; set; }
        public string? ConclusionPreparedBy { get; set; }
        public DateTime? ConclusionPreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public bool SubForm1NotApplicable { get; set; }
        public bool SubForm2NotApplicable { get; set; }
        public List<SubFormViewModel> SubFormsDisplay { get; set; }
        public List<FeeDetailViewModel> ServicesDisplay { get; set; }
        public TNATNEAssessmentViewModel TNATNEAssessmentDisplay { get; set; }
        public List<string>? AdminEmails { get; set; }
    }
}
