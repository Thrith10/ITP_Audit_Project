using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class Quiz
    {
        [Key]
        public Guid QuizID { get; set; }  // Changed from int to Guid

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
    }
}
