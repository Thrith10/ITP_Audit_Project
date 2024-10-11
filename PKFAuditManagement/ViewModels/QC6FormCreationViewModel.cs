using MongoDB.Bson;
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

            TNATNEAssessment = new TNATNEAssessmentViewModel();
        }
        // Prospective Client Details
        public List<string>? ProspectiveClients { get; set; }

        // User Details
        public string? UserEmail { get; set; }

        // Start of QC6Form data
        public string? QC6FormID { get; set; }
        public string? FileReference { get; set; }
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
        public string? RejectionReason { get; set; }
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
        public string? OutstandingUnpaidFeesComment { get; set; }
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
        public string? ConclusionPreparedBy { get; set; }
        public DateTime? ConclusionPreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public bool SubForm1NotApplicable { get; set; }
        public bool SubForm2NotApplicable { get; set; }
        public List<SubFormViewModel> SubForms { get; set; }
        public List<FeeDetailViewModel> Services { get; set; }
        public TNATNEAssessmentViewModel TNATNEAssessment { get; set; }
        public List<string>? AdminEmails { get; set; }
        public bool IsFirstApproved { get; set; }
        public bool IsSecondApproved { get; set; }

        // File Uploads
        public IFormFile? OtherDocuments { get; set; }
        public string? OtherDocumentsFileName { get; set; }
        public bool DeleteExistingFile { get; set; }
    }

    public class TNATNEAssessmentViewModel
    {
        public TNATNEAssessmentViewModel()
        {
            SectionB = new SectionBViewModel();
            SectionD = new SectionDViewModel();
        }
        public string? SectionCEvaluation { get; set; }
        public SectionBViewModel SectionB { get; set; }
        public SectionDViewModel SectionD { get; set; }
    }

    public class SectionBViewModel
    {
        public string? IsAudit { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public bool Q5 { get; set; }
    }

    public class SectionDViewModel
    {
        public string? Q1Comment { get; set; }
        public string? Q2Comment { get; set; }
        public string? Q3Comment { get; set; }
        public string? Q4Comment { get; set; }
        public string? Q5Comment { get; set; }
    }

    public class FeeDetailViewModel
    {
        public int QC6FormFeeDetailID { get; set; }
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
        public string? SignBy { get; set; }
        [Required]
        public DateTime SignDate { get; set; }
        public string? Comment { get; set; }
    }
}
