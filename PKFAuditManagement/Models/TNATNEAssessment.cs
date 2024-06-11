namespace PKFAuditManagement.Models
{
    public class TNATNEAssessment
    {
        public int TNATNEAssessmentID { get; set; }
        public int? QC6FormID { get; set; }
        public int? QC7FormID { get; set; }
        public required string SectionCEvaluation { get; set; }
        public ICollection<TNATNESectionB> SectionBs { get; set; }
        public ICollection<TNATNESectionD> SectionDs { get; set; }
    }
}
