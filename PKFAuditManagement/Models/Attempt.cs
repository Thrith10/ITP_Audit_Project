using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Attempt
    {
        [Key]
        public int AttemptID { get; set; } // Primary Key

        [Required]
        public string UserID { get; set; } // Foreign Key to ApplicationUser

        [Required]
        public int QuizID { get; set; } // Foreign Key

        [Required]
        public DateTime AttemptDate { get; set; }

        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }

        public ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
    }
}
