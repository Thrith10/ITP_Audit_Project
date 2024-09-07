using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Questions
    {
        [Key]
        public int QuestionID { get; set; }

        [Required]
        public Guid QuizID { get; set; }  // Changed from int to Guid

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }

        public string CorrectOptionText { get; set; }

        public ICollection<Option> Options { get; set; } = new List<Option>();
        public ICollection<QuizResponse>? Responses { get; set; }
    }
}
