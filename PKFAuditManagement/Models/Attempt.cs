using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Attempt
    {
        [Key]
        public int AttemptID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public Guid QuizID { get; set; }  // Changed from int to Guid

        [Required]
        public DateTime AttemptDate { get; set; }
        [Required]
        public int Score { get; set; }

        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }

        public ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
    }
}
