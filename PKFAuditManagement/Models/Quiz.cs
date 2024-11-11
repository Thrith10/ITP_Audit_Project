using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class Quiz
    {
        public Guid QuizID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        // Navigation properties
        public ICollection<Questions> Questions { get; set; } = new List<Questions>();
        public ICollection<Participants>? Participants { get; set; }
        public DateTime QuizStart { get; set; }
        public DateTime QuizEnd { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        // New property: List of topics
        public ICollection<QuizTopic> Topics { get; set; } = new List<QuizTopic>();
        public Guid FeedbackFormID { get; set; } // Foreign key to the associated Feedback Form
        // Foreign key to SelfAssessmentForm
        public Guid? SelfAssessmentFormID { get; set; } // Nullable in case some quizzes don’t require a self-assessment form
        // Navigation properties
        public FeedbackForm FeedbackForm { get; set; } // The feedback form attached to this quiz
        public SelfAssessmentForm SelfAssessmentForm { get; set; }
        public ICollection<FeedbackResponse> FeedbackResponses { get; set; } = new List<FeedbackResponse>(); // Collection of responses linked to this quiz

    }
}
