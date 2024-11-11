using System;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class SelfAssessmentResponse
    {
        [Key]
        public Guid SelfAssessmentResponseID { get; set; }

        [Required]
        public Guid QuizID { get; set; } // Foreign key to Quiz

        [Required]
        public Guid SelfAssessmentQuestionID { get; set; } // Foreign key to SelfAssessmentQuestion

        [Required]
        public string SubmittedBy { get; set; } // UserID of the user who submitted the self-assessment

        [Required]
        public int Rating { get; set; } // User's rating response to the question, typically 1 to 5

        [Required]
        public AssessmentStage Stage { get; set; } // Enum to distinguish "Before" and "After" stages

        // Navigation properties
        public Quiz Quiz { get; set; }
        public SelfAssessmentQuestion SelfAssessmentQuestion { get; set; }
    }

    public enum AssessmentStage
    {
        Before, // Before course
        After   // After course
    }
}
