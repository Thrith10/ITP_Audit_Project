using System;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class SelfAssessmentQuestion
    {
        [Key]
        public Guid SelfAssessmentQuestionID { get; set; }

        [Required]
        public Guid SelfAssessmentFormID { get; set; } // Foreign key to SelfAssessmentForm

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } // The actual self-assessment question text

        [Required]
        public SelfAssessmentType Type { get; set; } // Enum representing different self-assessment types (Rate1To5)

        // Navigation property
        public SelfAssessmentForm SelfAssessmentForm { get; set; }
    }

    public enum SelfAssessmentType
    {
        Rate1To5, // Self-assessment will primarily use rating responses from 1 to 5
        YesOrNo
    }
}
