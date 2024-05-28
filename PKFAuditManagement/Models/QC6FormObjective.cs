using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC6FormObjective
    {
        public int QC6FormObjectiveID { get; set; }
        public int QC6SubFormID { get; set; }
        public required int ObjectiveNo { get; set; }
        public required string Objective { get; set; }
        public QC6SubForm QC6SubForm { get; set; }
        public ICollection<QC6FormTestDescription> QC6FormTestDescriptions { get; set; } // One-to-Many

    }

}
