namespace PKFAuditManagement.Models
{
    public class SelfAssessmentRating
    {
        public Guid SelfAssessmentRatingID { get; set; }
        public Guid SelfAssessmentID { get; set; } // Foreign key to SelfAssessment
        public string Topic { get; set; }
        public int Rating { get; set; } // Value between 1 and 5

        // Navigation property
        public SelfAssessment SelfAssessment { get; set; }
    }
}
