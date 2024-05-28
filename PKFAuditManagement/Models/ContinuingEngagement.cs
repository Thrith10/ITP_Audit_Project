using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class ContinuingEngagement
    {
        public int ContinuingEngagementId { get; set; }
        public string CreatedBy { get; set; }
        public string? FileReference { get; set; }
        public string? Client { get; set; }
        public DateTime? PeriodEnded { get; set; }
        public string? EngagementType { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? PreparedByDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedByDate { get; set; }
        public string? Status { get; set; }
        public DateTime FormSubmissionDate { get; set; }

    }
}
