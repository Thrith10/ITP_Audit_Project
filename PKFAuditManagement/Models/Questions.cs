using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public enum QuestionType
    {
        TrueFalse = 1,
        SingleAnswerMCQ = 2,
        MultiAnswerMCQ = 3
    }

    public class Questions
    {
        [Key]
        public int QuestionID { get; set; }

        [Required]
        public Guid QuizID { get; set; }  // Changed from int to Guid

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public QuestionType Type { get; set; }  // New property to store question type

        // For True/False and SingleAnswerMCQ, only one correct answer. For MultiAnswerMCQ, store multiple correct answers.
        public string CorrectOptionText { get; set; }

        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }

        public ICollection<Option> Options { get; set; } = new List<Option>();

        public ICollection<QuizResponse>? Responses { get; set; }
    }
}
