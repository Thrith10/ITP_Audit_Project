using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Participants
    {
        [Key]
        public int ParticipantID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public Guid QuizID { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool ClockedAttendance { get; set; } = false;
        public bool QuizDone { get; set; } = false;
        public bool FeedbackDone { get; set; } = false;
        public bool OverallCompletion { get; set; } = false;

        public DateTime? OverallCompletionDate { get; set; } = null;


        // Navigation properties
        [ForeignKey("QuizID")]
        public Quiz Quiz { get; set; }
    }
}
