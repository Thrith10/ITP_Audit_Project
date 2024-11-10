using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class SelfAssessmentRating
    {

        [Key]
        public Guid SelfAssessmentRatingID { get; set; }
        [ForeignKey("SelfAssessment")]
        public Guid SelfAssessmentID { get; set; } // Foreign key to SelfAssessment, ensure unique
        public string Topic { get; set; }
        public int Rating { get; set; } // Value between 1 and 5

        // Navigation property
        public SelfAssessment SelfAssessment { get; set; }
    }
}
