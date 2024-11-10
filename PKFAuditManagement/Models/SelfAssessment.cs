using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class SelfAssessment
    {
        [Key]
        public Guid SelfAssessmentID { get; set; }
        public Guid QuizID { get; set; }  // Foreign key to Quiz
        public string UserID { get; set; } // Foreign key to User

        // Ratings stored as separate entries in a related table
        public ICollection<SelfAssessmentRating> BeforeRatings { get; set; } = new List<SelfAssessmentRating>();
        public ICollection<SelfAssessmentRating> AfterRatings { get; set; } = new List<SelfAssessmentRating>();

        // Navigation properties
        public Quiz Quiz { get; set; }

    }

}
