using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class QuizResponse
    {
        [Key]
        public int QuizResponseID { get; set; } // Primary Key

        [Required]
        public int AttemptID { get; set; } // Foreign Key

        [Required]
        public int QuestionID { get; set; } // Foreign Key

        [Required]
        public string SelectedOption { get; set; } // This should be a string

        // Computed property
        [NotMapped]
        public bool IsCorrect;

        // Navigation properties
        [ForeignKey("AttemptID")]
        public Attempt Attempt { get; set; }

        [ForeignKey("QuestionID")]
        public Questions Question { get; set; }
    }
}
