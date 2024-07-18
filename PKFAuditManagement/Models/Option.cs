using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKFAuditManagement.Models
{
    public class Option
    {
        [Key]
        public int OptionID { get; set; }

        [Required]
        public int QuestionID { get; set; }

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; }

        // Navigation properties
        [ForeignKey("QuestionID")]
        public Questions Question { get; set; }
    }
}
