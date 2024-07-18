using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class Quiz
    {
        [Key]
        public int QuizID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        // Navigation properties
        public ICollection<Questions> Questions { get; set; } = new List<Questions>();
        public ICollection<Participants>? Participants { get; set; }
  
    }
}
