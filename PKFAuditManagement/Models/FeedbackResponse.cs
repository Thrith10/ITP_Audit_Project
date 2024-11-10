using System;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class FeedbackResponse
    {
        [Key]
        public Guid FeedbackResponseID { get; set; }

        [Required]
        public Guid QuizID { get; set; } // Foreign key to Quiz

        [Required]
        public Guid FeedbackQuestionID { get; set; } // Foreign key to FeedbackQuestion

        [Required]
        public string SubmittedBy { get; set; } // UserID of the user who submitted the feedback

        [Required]
        public string Response { get; set; } // User's response to the question

        // Navigation properties
        public Quiz Quiz { get; set; }
        public FeedbackQuestion FeedbackQuestion { get; set; }
    }
}
