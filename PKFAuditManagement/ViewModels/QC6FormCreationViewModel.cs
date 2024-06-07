using PKFAuditManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class QC6FormCreationViewModel
    {
        public QC6FormCreationViewModel()
        {
            // Initialize the Services list with a default service
            Services = new List<FeeDetailViewModel>();
            Services.Add(new FeeDetailViewModel { NatureOfService = "", Fee = 0 });
        }
        // User Details
        public string? UserEmail { get; set; }

        // Start of QC6Form data
        [Required]
        public string? ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        [Required]
        public string? EngagementType { get; set; }
        [Required]
        public string? PreparedBy { get; set; }
        [Required]
        public DateTime PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        [Required]
        public string? PKFEntityProposingService { get; set; }
        [Required]
        public string? SourceOfReferral { get; set; }
        [Required]
        public string? NatureOfServiceForEstimateFee { get; set; }
        [Required]
        public decimal? EstimatedFee { get; set; }
        [Required]
        public decimal? BudgetedTimeCost { get; set; }
        [Required]
        public decimal? BudgetedFeeRecoveryRate { get; set; }
        public bool OutstandingUnpaidFees { get; set; }
        [Required]
        public decimal? AuditFee { get; set; }
        [Required]
        public decimal? GrandTotal { get; set; }
        [Required]
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        [Required]
        public string? TypeOfActivities { get; set; }
        [Required]
        public string? ComplexityOfEngagement { get; set; }
        public string? PredecessorAuditor { get; set; }
        [Required]
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
        [Required]
        public string? EngagementSubjectedTo { get; set; }
        [Required]
        public string? SafeguardReviewerAssigned { get; set; }
        public string? IsNewEngagementAcceptance { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        [Required]
        public string? Satisfaction { get; set; }
        [Required]
        public string? ConclusionPreparedBy { get; set; }
        [Required]
        public DateTime? ConclusionPreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public List<SubFormViewModel> SubForms { get; set; }
        public List<FeeDetailViewModel> Services { get; set; }
    }

    public class FeeDetailViewModel
    {
        public string? NatureOfService { get; set; }
        public string? OtherService { get; set; }
        public decimal? Fee { get; set; }
    }

    public class SubFormViewModel
    {
        public int QC6SubFormID { get; set; }
        public string? SubFormType { get; set; }
        public List<ObjectiveViewModel> Objectives { get; set; }
    }

    public class ObjectiveViewModel
    {
        public int QC6FormObjectiveID { get; set; }
        public string? Objective { get; set; }
        public List<TestDescriptionViewModel> TestDescriptions { get; set; }
    }

    public class TestDescriptionViewModel
    {
        public int QC6FormTestDescriptionID { get; set; }
        public string? Description { get; set; }
        [Required]
        public string? SignBy { get; set; }
        [Required]
        public DateTime SignDate { get; set; }
        public string? Comment { get; set; }
    }
}
