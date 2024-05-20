namespace PKFAuditManagement.Models
{
    public class EngagementProcedureTest
    {
        public int EngagementProcedureTestID { get; set; }
        public int EngagementObjective { get; set; }
        public int EngagementId { get; set; }
        public string? TestDescription { get; set; }
        public string? Reference { get; set; }
        public string? SignOffBy { get; set; }
        public DateTime? SignOffDate { get; set; }
        public string? Comments { get; set; }
        public Engagement Engagement { get; set; }
    }

}
