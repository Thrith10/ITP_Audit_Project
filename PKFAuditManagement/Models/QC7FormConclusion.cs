namespace PKFAuditManagement.Models
{
    public class QC7FormConclusion
    {
        public int QC7FormConclusionID { get; set; }
        public int? QC7FormID { get; set; }
        public bool AnyRiskAssociated { get; set; }
        public string? RiskExplanationCurrentYearPriorYear { get; set; }
        public bool IsSafeguardApplied { get; set; }
        public string? NatureOfSafeguard { get; set; }
        public required string ContinuingEngagementRiskRated { get; set; }
        public string? SafeguardReviewPartnerAssigned { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        public required string EngagementRetainedRejected { get; set; }
        public required string EMPreparedBy { get; set; }
        public required DateTime EMPreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public QC7Form? QC7Form { get; set; }
    }
}
