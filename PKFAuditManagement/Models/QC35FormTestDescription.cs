using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class QC35FormTestDescription
    {
        [Key]
        public int QC35FormTestDescriptionID { get; set; }
        public int QC35FormID { get; set; }
        public string? Description { get; set; }
    }
}
