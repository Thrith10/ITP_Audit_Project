using PKFAuditManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class QC7FormCreationViewModel
    {
        public QC7FormCreationViewModel()
        {
            TNATNEAssessment = new TNATNEAssessmentViewModel();
        }
        // User Details
        public string? UserEmail { get; set; }

        // Start of QC7Form data
        public string? QC7FormID { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? FileReference { get; set; }
        [Required]
        public string? Client { get; set; }
        [Required]
        public DateTime? PeriodEnded { get; set; }
        [Required]
        public string? EngagementType { get; set; }
        [Required]
        public string? PreparedBy { get; set; }
        [Required]
        public DateTime PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime ReviewedByDate { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        [Required]
        public decimal? PriorYearFee { get; set; }
        [Required]
        public decimal? TimeCosts { get; set; }
        public decimal? PriorYearRecoveryRate { get; set; }
        public bool AnyOutstandingUnpaidAuditFees { get; set; }
        [Required]
        public string? TypeOfClientActivities { get; set; }
        [Required]
        public string? RiskRatingPriorYear { get; set; }
        public bool AnySuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledComment { get; set; }
        public string? SafeguardReviewerName { get; set; }
        public bool AnyOutstandingUnpaidNonAuditFees { get; set; }
        [Required]
        public decimal? FeeConcentration { get; set; }
        [Required]
        public decimal? ProposedFeeCurrentYear { get; set; }
        [Required]
        public decimal? BudgetedTimeCost { get; set; }
        [Required]
        public decimal? ProposedRecoveryRateCurrentYear { get; set; }
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public string? ErrorMessage { get; set; }

        // End of QC7Form data

        // Start of QC7FormConclusion date
        public bool AnyRiskAssociated { get; set; }
        public string? RiskExplanationCurrentYearPriorYear { get; set; }
        public bool IsSafeguardApplied { get; set; }
        public string? NatureOfSafeguard { get; set; }
        [Required]
        public string? ContinuingEngagementRiskRated { get; set; }
        public string? SafeguardReviewPartnerAssigned { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        [Required]
        public string? EngagementRetainedRejected { get; set; }
        public string? EMPreparedBy { get; set; }
        public DateTime? EMPreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public bool SubForm1NotApplicable { get; set; }
        public bool SubForm2NotApplicable { get; set; }
        public List<QC7SubFormViewModel> SubForms { get; set; }
        public TNATNEAssessmentViewModel TNATNEAssessment { get; set; }
        public List<string>? AdminEmails { get; set; }
    }

    public class QC7SubFormViewModel
    {
        public int QC7SubFormID { get; set; }
        public string? SubFormType { get; set; }
        public List<QC7ObjectiveViewModel> Objectives { get; set; }
    }

    public class QC7ObjectiveViewModel
    {
        public int QC7FormObjectiveID { get; set; }
        public string? Objective { get; set; }
        public List<QC7TestDescriptionViewModel> TestDescriptions { get; set; }
    }

    public class QC7TestDescriptionViewModel
    {
        public int QC7FormTestDescriptionID { get; set; }
        public string? Description { get; set; }
        [Required]
        public string? SignBy { get; set; }
        [Required]
        public DateTime SignDate { get; set; }
        public string? Comment { get; set; }
    }
}
