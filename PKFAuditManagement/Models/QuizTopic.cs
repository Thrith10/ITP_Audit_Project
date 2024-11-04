using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class QuizTopic
    {
        public Guid TopicID { get; set; }
        public Guid QuizID { get; set; } // Foreign key to Quiz

        [Required]
        [StringLength(255)]
        public string Name { get; set; } // Topic name

        // Navigation property
        public Quiz Quiz { get; set; }
    }
}
