using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Participants
    {
        [Key]
        public int ParticipantID { get; set; } // Primary Key

        [Required]
        public string UserID { get; set; } // Foreign Key to ApplicationUser

        [Required]
        public int QuizID { get; set; } // Foreign Key

        public int TotalScore { get; set; } // Total score for the quiz

        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }
    }
}
