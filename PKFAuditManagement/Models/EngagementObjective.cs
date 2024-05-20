using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class EngagementObjective
    {
        public int EngagementObjectiveID { get; set; }
        public required string Objective { get; set; }

    }

}
