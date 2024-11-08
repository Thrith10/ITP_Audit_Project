using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class FeedbackForm
    {
        [Key]
        public Guid FeedbackFormID { get; set; }

        [Required]
        public string Title { get; set; } // Name or title for the feedback form

        [Required]
        public string CreatedBy { get; set; } // UserID of the user who created the feedback form

        [Required]
        public DateTime CreatedDate { get; set; } // Date when the feedback form was created

        // Navigation property
        public ICollection<FeedbackQuestion> Questions { get; set; } = new List<FeedbackQuestion>(); // List of feedback questions
    }
}
