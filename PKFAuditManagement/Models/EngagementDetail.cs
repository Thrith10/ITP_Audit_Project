namespace PKFAuditManagement.Models
{
    public class EngagementDetail
    {
        public int EngagementDetailID { get; set; }
        public int EngagementId { get; set; }
        public string? PKFEntityProposingService { get; set; }
        public string? SourceOfReferral { get; set; }
        public decimal? EstimatedFee { get; set; }
        public decimal? BudgetedFeeRecoveryRate { get; set; }
        public decimal? FeeFromServices { get; set; }
        public bool? OutstandingUnpaidFees { get; set; }
        public decimal? FeeConcentration { get; set; }
        public bool ConflictsCheckDone { get; set; }
        public string? TypeOfActivities { get; set; }
        public string? ComplexityOfEngagement { get; set; }
        public string? PredecessorAuditor { get; set; }
        public string? ReasonsForDiscontinuance { get; set; }
        public bool? PublicInterestEntity { get; set; }
        public string? TypeOfPIE { get; set; }
        public bool? TransnationalEntity { get; set; }
        public bool? TransnationalAudit { get; set; }
        public Engagement Engagement { get; set; }
    }

}
