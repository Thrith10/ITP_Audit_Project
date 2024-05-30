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
        public string? ContinuingEngagementRiskRated { get; set; }
        public string? SafeguardReviewPartnerAssigned { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        public string? EngagementRetainedRejected { get; set; }
        public required string EMPreparedBy { get; set; }
        public required DateTime EMPreparedByDate { get; set; }
        public required string EPHODApprovedBy { get; set; }
        public required DateTime EPHODApprovedByDate { get; set; }
        public required string MPHODQMPApprovedBy { get; set; }
        public required DateTime MPHODQMPApprovedByDate { get; set; }
        public QC7Form? QC7Form { get; set; }
    }
}
