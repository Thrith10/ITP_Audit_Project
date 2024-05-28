using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class Engagement
    {
        public int EngagementID { get; set; }
        public string CreatedBy { get; set; }
        public string? FileReference { get; set; }
        public string? ProspectiveClient { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string? EngagementType { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public EngagementDetail EngagementDetail { get; set; } // One-to-One
        public ICollection<EngagementProcedureTest> EngagementProcedureTests { get; set; } // One-to-Many

    }
}
