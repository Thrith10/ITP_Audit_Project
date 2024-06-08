namespace PKFAuditManagement.Models
{
    public class TNATNESectionB
    {
        public int TNATNESectionBID { get; set; }
        public int TNATNEAssessmentID { get; set; }
        public string IsAudit { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public bool Q5 { get; set; }
        public TNATNEAssessment TNATNEAssessment { get; set; }
    }
}
