using PKFAuditManagement.Models;

namespace PKFAuditManagement.ViewModels
{
    public class QC6FormCreationViewModel
    {
        // Start of QC6Form data
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
        public string? NatureOfServiceForEstimateFee { get; set; }
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
        public bool IsPublicInterestEntity { get; set; }
        public string? PublicInterestEntityType { get; set; }
        public bool TransnationalEntity { get; set; }
        public bool TransnationalAudit { get; set; }
        public string? TransnationalAuditComment { get; set; }
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
        public List<SubFormViewModel> SubForms { get; set; }
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
        public string Reference { get; set; }
        public string SignBy { get; set; }
        public DateTime SignDate { get; set; }
        public string Comment { get; set; }
    }
}
