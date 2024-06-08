namespace PKFAuditManagement.Models
{
    public class TNATNESectionD
    {
        public int TNATNESectionDID { get; set; }
        public int TNATNEAssessmentID { get; set; }
        public string? Q1Comment { get; set; }
        public string? Q2Comment { get; set; }
        public string? Q3Comment { get; set; }
        public string? Q4Comment { get; set; }
        public string? Q5Comment { get; set; }
        public TNATNEAssessment TNATNEAssessment { get; set; }
    }
}
