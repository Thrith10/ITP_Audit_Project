using System;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class FeedbackQuestion
    {
        [Key]
        public Guid FeedbackQuestionID { get; set; }

        [Required]
        public Guid FeedbackFormID { get; set; } // Foreign key to FeedbackForm

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } // The actual feedback question text

        [Required]
        public FeedbackType Type { get; set; } // Enum representing different feedback types (YesNo, Rate1To5, ShortAnswer)

        // Navigation property
        public FeedbackForm FeedbackForm { get; set; }
    }

    public enum FeedbackType
    {
        YesNo,       // For Yes or No responses
        Rate1To5,    // For rating responses (1 to 5)
        ShortAnswer  // For open-ended short answers
    }
}
