using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class SelfAssessmentForm
    {
        [Key]
        public Guid SelfAssessmentFormID { get; set; }

        [Required]
        public string Title { get; set; } // Name or title for the self-assessment form

        [Required]
        public string CreatedBy { get; set; } // UserID of the user who created the self-assessment form

        [Required]
        public DateTime CreatedDate { get; set; } // Date when the self-assessment form was created

        // Navigation property
        public ICollection<SelfAssessmentQuestion> Questions { get; set; } = new List<SelfAssessmentQuestion>(); // List of self-assessment questions
    }
}
