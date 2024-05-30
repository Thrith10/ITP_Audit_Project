using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC7FormObjective
    {
        public int QC7FormObjectiveID { get; set; }
        public int QC7SubFormID { get; set; }
        public required int ObjectiveNo { get; set; }
        public required string Objective { get; set; }
        public QC7SubForm QC7SubForm { get; set; }
        public ICollection<QC7FormTestDescription> QC7FormTestDescriptions { get; set; } // One-to-Many

    }

}
