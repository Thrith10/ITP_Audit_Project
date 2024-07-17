namespace PKFAuditManagement.Models
{
    public class QC6FormConclusion
    {
        public int QC6FormConclusionID { get; set; }
        public int? QC6FormID { get; set; }
        public bool AnySignificantRisk { get; set; }
        public string? SignificantRiskComment { get; set; }
        public required string NewEngagementRiskRating { get; set; }
        public string? NewEngagementRiskRatingReason { get; set; }
        public required string EngagementSubjectedTo { get; set; }
        public required string SafeguardReviewerAssigned { get; set; }
        public required string IsNewEngagementAcceptance { get; set; }
        public bool IsSuspiciousTransactionReportFiled { get; set; }
        public string? SuspiciousTransactionReportFiledRationale { get; set; }
        public required string Satisfaction { get; set; }
        public required string PreparedBy { get; set; }
        public DateTime PreparedByDate { get; set; }
        public string? EPHODApprovedBy { get; set; }
        public DateTime? EPHODApprovedByDate { get; set; }
        public string? MPHODQMPApprovedBy { get; set; }
        public DateTime? MPHODQMPApprovedByDate { get; set; }
        public bool IsFirstApproved { get; set; }
        public bool IsSecondApproved { get; set; }
        public QC6Form? QC6Form { get; set; }
    }
}
