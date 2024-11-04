using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class Feedback
    {
        public Guid FeedbackID { get; set; }
        public Guid QuizID { get; set; }  // Foreign key to Quiz
        public string UserID { get; set; } // Foreign key to User

        [Required]
        [StringLength(1000)]
        public string Comments { get; set; } // Accepts "Yes", "No", ratings ("1" to "5"), or short answers

        [Required]
        public FeedbackType Type { get; set; } // Enum representing different feedback types

        // Navigation properties
        public Quiz Quiz { get; set; }
        public User User { get; set; } // Assuming a User model exists
    }
    public enum FeedbackType
    {
        YesNo,
        Rate1To5,
        ShortAnswer
    }
}
